using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly ShopContext db;
        public HangHoaController(ShopContext context) 
        {
            db = context;
        }
        public IActionResult Index(int? loai)
        {
            var HangHoas = db.HangHoas.AsQueryable();

            if (loai.HasValue)
            {
                HangHoas = HangHoas.Where(p => p.MaLoai == loai.Value);
            }

            var result = HangHoas.Select(p => new HangHoaVm
            {
                MaHH = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "", 
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai

            }) ;
            return View(result);
        }

        public IActionResult Search(string? query)
        {
            var HangHoas = db.HangHoas.AsQueryable();

            if (query !=null)
            {
                HangHoas = HangHoas.Where(p => p.TenHh.Contains(query));
            }

            var result = HangHoas.Select(p => new HangHoaVm
            {
                MaHH = p.MaHh,
                TenHH = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? "",
                MoTaNgan = p.MoTaDonVi ?? "",
                TenLoai = p.MaLoaiNavigation.TenLoai

            });
            return View(result);
        }

        public IActionResult Detail(int id)
        {
            var data = db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);
            if (data == null)
            {
                TempData["Message"] = $"Can not found item {id}";
                return Redirect("/404");
            }

            var result = new ChiTietHangHoaVm
            {
                MaHH = data.MaHh,
                TenHH = data.TenHh,
                DonGia = data.DonGia ?? 0,
                ChiTiet = data.MoTa ?? String.Empty,
                Hinh = data.Hinh ?? string.Empty,
                MoTaNgan = data.MoTaDonVi ?? String.Empty,
                TenLoai = data.MaLoaiNavigation.TenLoai,
                SoLuongTon = 10,
                DiemDanhGia = 5,
            };
        
            return View(result);
        }
    }
}
