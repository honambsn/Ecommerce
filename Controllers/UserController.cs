using AutoMapper;
using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ecommerce.Controllers
{
    public class UserController : Controller
    {
        private readonly ShopContext db;
		private readonly IMapper _mapper;

		public UserController(ShopContext context, IMapper mapper) 
        {
            db = context;
            _mapper = mapper;
        }

		#region Register

		[HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterVM model, IFormFile Hinh) 
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = _mapper.Map<KhachHang>(model);
                    user.RandomKey = MyUtil.GenerateRandomKey();
                    user.MatKhau = model.MatKhau.ToMd5Hash(user.RandomKey);
                    user.HieuLuc = true;
                    user.VaiTro = 0;

                    if (Hinh != null)
                    {
                        user.Hinh = MyUtil.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index", "HangHoa");
                } catch(Exception ex) { var mess = $"{ex.Message} shh"; }
            }
            else { return Redirect("/404"); }
            return View();
        }
        #endregion
	}
}
