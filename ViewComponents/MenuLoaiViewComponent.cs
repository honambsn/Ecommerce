using Ecommerce.Data;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace Ecommerce.ViewComponents
{
    public class MenuLoaiViewComponent: ViewComponent
    {
        private readonly ShopContext db;
        public MenuLoaiViewComponent(ShopContext context) => db = context;
        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(loai => new MenuLoaiVm
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                SoLuong = loai.HangHoas.Count
            }).OrderBy(p=>p.TenLoai);
            return View(data);
        }
    }
}