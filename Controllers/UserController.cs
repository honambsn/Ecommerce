using AutoMapper;
using Ecommerce.Data;
using Ecommerce.Helpers;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Security.Claims;

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


        #region Login
        [HttpGet]
        public IActionResult Login(string? ReturnUrl) 
        {
            
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var user = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("Error", "Wrong login info");
                }
                else
                {
                    if (!user.HieuLuc)
                    {
                        ModelState.AddModelError("Error", "Account is expired, contact for more info");
                    }
                    else
                    {
                        if (user.MatKhau != model.Password.ToMd5Hash(user.RandomKey))
                        {
                            ModelState.AddModelError("Error", "Wrong password info");
                        }
                        else
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Name, user.HoTen),
                                new Claim("CustomerId", user.MaKh),

                                //claim - dynamic role
                                new Claim(ClaimTypes.Role, "Customer")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme);
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if(Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                    
                }
                
            }
            return View();
        }
        #endregion
        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/HangHoa");
        }
    }
}
