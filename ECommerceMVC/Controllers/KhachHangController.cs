using AutoMapper;
using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
	public class KhachHangController : Controller
	{
		private readonly Hshop2023Context db;
		private readonly IMapper _mapper;

		public KhachHangController(Hshop2023Context context, IMapper mapper)
		{
			db = context;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult DangKy()
		{
			return View();
		}

		[HttpPost]
		public IActionResult DangKy(RegisterVM model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var khachHang = _mapper.Map<KhachHang>(model);
					khachHang.RandomKey = MyUtil.GenerateRamdomKey();
					khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
					khachHang.HieuLuc = true;//sẽ xử lý khi dùng Mail để active
					khachHang.VaiTro = 0;
					db.Add(khachHang);
					db.SaveChanges();
					return RedirectToAction("Index", "HangHoa");
				}
				catch (Exception ex)
				{
					var mess = $"{ex.Message} shh";
				}
			}
			return View();
		}


		#region Login
		[HttpGet]
		public IActionResult DangNhap(string? ReturnUrl)
		{
			ViewBag.ReturnUrl = ReturnUrl;
			return View();
		}


		[HttpPost]
		public async Task<IActionResult> DangNHap(LoginVM model, string? returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			if (ModelState.IsValid)
			{
				var khachHang = db.KhachHangs.SingleOrDefault(kh =>
				 kh.MaKh == model.UserName);
				if (khachHang == null)
				{
					ModelState.AddModelError("Lỗi", "Tài khoản hoặc email không tồn tại");
				}
				else
				{
					if (!khachHang.HieuLuc)
					{
						ModelState.AddModelError("Lỗi", "Tài khoản đã bị khóa");
					}
					else
					{
						if (khachHang.MatKhau != model.Password.ToMd5Hash(khachHang.RandomKey))
						{
							ModelState.AddModelError("Lỗi", "sai mật khẩu");
						}
						else
						{
							var calims = new List<Claim> {
								new Claim(ClaimTypes.Email, khachHang.Email),
								new Claim(ClaimTypes.Name, khachHang.HoTen),
								new Claim("CustomerID", khachHang.MaKh),

								new Claim(ClaimTypes.Role, "Customer")
							};
							var claimsIdentity = new ClaimsIdentity(calims,
								CookieAuthenticationDefaults.AuthenticationScheme);
							var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

							await HttpContext.SignInAsync(claimsPrincipal);

							if (Url.IsLocalUrl(returnUrl))
							{
								return Redirect(returnUrl);
							}
							else
							{
								return Redirect("/HangHoa");
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
	}
}
