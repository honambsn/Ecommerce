using System.ComponentModel.DataAnnotations;

namespace Ecommerce.ViewModels
{
	public class LoginVM
	{
		[Display(Name = "Username")]
		[Required(ErrorMessage = "Enter username")]
		[MaxLength(20, ErrorMessage = "20 characters are maximum")]
		public string UserName { get; set; }
		

		[Display(Name = "Password")]
		[Required(ErrorMessage = "Enter password")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
