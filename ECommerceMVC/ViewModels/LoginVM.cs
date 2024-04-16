
using System.ComponentModel.DataAnnotations;
namespace ECommerceMVC.ViewModels
{
	public class LoginVM
	{
		[Display(Name = "UserName")]
		[MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
		public string UserName { get; set; }

		[Display(Name = "Password")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}
