using Humanizer.Localisation.DateToOrdinalWords;

namespace Ecommerce.ViewModels
{
    public class CartItem
    {
        public int MaHh { get; set; }
        public string Hinh { get; set; }
        public string TenHH { get; set; }
        public double DonGia { get; set; }
        public int SoLuong { get; set; }
        public double ThanhTien => SoLuong * DonGia;
        //public double FlatRate { get; } = 3.00;

        //public getFlatRate()
        //{
        //    if ( /*condition*/)
        //        FlatRate = 5.00;
        //    else
        //        FlatRate = 10.00;
        //}
        
    }
}
