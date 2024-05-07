using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
		private readonly PaypalClient _paypalClient;
		private readonly ShopContext db;

        public CartController(ShopContext context, PaypalClient paypalCient) 
        {
            _paypalClient = paypalCient;
            db = context;
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
        
        [HttpPost]
        public IActionResult CheckOut(CheckoutVM model) 
        {
            if (ModelState.IsValid)
            {
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





	}
}
