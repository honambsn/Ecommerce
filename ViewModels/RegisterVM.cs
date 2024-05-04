using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
    public class RegisterVM
    {
        [Key]
        [Display(Name = "Username")]
        [Required(ErrorMessage= "*")]
        [MaxLength(20, ErrorMessage = "Maximum are 20 characters")]
        public string MaKh { get; set; } = null!;
        
        [Display(Name = "Password")]
        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Display(Name = "Full name")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Maximum are 50 characters")]
        public string HoTen { get; set; }

        public bool GioiTinh { get; set; } = true;

        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }


        [Display(Name ="Address")]
        [MaxLength(60, ErrorMessage = "Maximum are 60 characters")]
        public string DiaChi { get; set; }


        [Display(Name ="Phone number")]
        [MaxLength(24, ErrorMessage = "Maximum are 24 characters")]
        [RegularExpression(@"0[9875]\d{8}", ErrorMessage = "Not valid")]
        public string DienThoai { get; set; }


        [Display(Name ="Email address")]
        [EmailAddress(ErrorMessage ="Not valid")]
        public string Email { get; set; }

		[Display(Name = "Picture")]
		public string? Hinh { get; set; }

        //        public bool HieuLuc { get; set; }

        //        public int VaiTro { get; set; }

        //        public string? RandomKey { get; set; }
    }
}
