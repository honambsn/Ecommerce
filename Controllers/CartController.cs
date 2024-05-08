using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Helpers;
using Microsoft.AspNetCore.Authorization;
using Ecommerce.Services;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
		private readonly PaypalClient _paypalClient;
		private readonly ShopContext db;
		private readonly IVnPayService _vnPayService;

		public CartController(ShopContext context, PaypalClient paypalCient, IVnPayService vnPayService) 
        {
            _paypalClient = paypalCient;
            db = context;
            _vnPayService = vnPayService;
        }

        
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();
        public IActionResult Index()
        {
            return View(Cart);
        }
        
        public IActionResult AddToCart(int id, int quanity = 1)
        {
            var myCart = Cart;
            var item = myCart.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Can not find item has id {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHH = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quanity
                };
                myCart.Add(item);
            }
            else
            {
                item.SoLuong += quanity;
            }
            
            HttpContext.Session.Set(MySetting.CART_KEY, myCart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id) 
        {
            var myCart = Cart;
            var item = myCart.SingleOrDefault(p => p.MaHh == id);
            if (item != null) 
            {
                myCart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY,myCart); 
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult CheckOut() 
        {
            if(Cart.Count == 0)
            {
                return Redirect("/");
            }

            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(Cart);
        }
        [Authorize]
        [HttpPost]
        public IActionResult CheckOut(CheckoutVM model, string payment ="COD") 
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.ThanhTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 100000),
                    };
                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }
                var customerID = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;

                var customer = new KhachHang();
                if(model.GiongKhachHang)
                {
                    customer = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerID);
                }

                var receipt = new HoaDon
                {
                    MaKh = customerID,
                    HoTen = model.HoTen ?? customer.HoTen,
                    DiaChi = model.DiaChi ?? customer.DiaChi,
                    DienThoai = model.DienThoai ?? customer.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = "COD",
                    CachVanChuyen = "Marcus GrabFood",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu
                };

                db.Database.BeginTransaction();
                try
                {
                    db.Database.CommitTransaction();
                    db.Add(receipt);
                    db.SaveChanges();

                    var receiptInfo = new List<ChiTietHd>();
                    foreach( var item in Cart)
                    {
                        receiptInfo.Add(new ChiTietHd
                        {
                            MaHd = receipt.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }
                    db.AddRange(receiptInfo);
                    db.SaveChanges();

                    HttpContext.Session.Set < List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                } catch {
                    db.Database.RollbackTransaction();
                }

                

            }
            return View(Cart);
        }

		[Authorize]
		public IActionResult PaymentSuccess()
		{
			return View("Success");
		}

		#region Paypal payment
		[Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            var tongTien = Cart.Sum(p=> p.ThanhTien).ToString();
            var currency = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, currency, maDonHangThamChieu);

                return Ok(response);

            } catch (Exception ex) {
                var error = new {ex.GetBaseException().Message};
                return BadRequest(error);
            }
        }

		[Authorize]
		[HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken )
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                return Ok(response);
            }catch (Exception ex) {
				var error = new { ex.GetBaseException().Message };
				return BadRequest(error);
			}
        }
		#endregion


		[Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

		[Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"payment error: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            //save order to db

			TempData["Message"] = $"payment succes";
            return RedirectToAction("PaymentSuccess");

		}
	}
}
