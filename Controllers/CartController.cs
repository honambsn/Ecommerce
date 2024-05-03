using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.Helpers;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ShopContext db;

        public CartController(ShopContext context) 
        {
            db = context;
        }

        const string CART_KEY = "MYCART";
        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
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
            
            HttpContext.Session.Set(CART_KEY, myCart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id) 
        {
            var myCart = Cart;
            var item = myCart.SingleOrDefault(p => p.MaHh == id);
            if (item != null) 
            {
                myCart.Remove(item);
                HttpContext.Session.Set(CART_KEY,myCart); 
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}
