using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using NguyenDinhMinhKhang_2380600989.Data;

namespace NguyenDinhMinhKhang_2380600989.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public ResetPasswordModel(
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(6, ErrorMessage = "Mã OTP phải có 6 chữ số.", MinimumLength = 6)]
            [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP chỉ bao gồm 6 chữ số.")]
            [Display(Name = "Mã OTP")]
            public string Code { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} ký tự.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet(string email)
        {
            Input = new InputModel { Email = email };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid request.");
                return Page();
            }

            // Verify OTP
            var cacheKey = $"password_reset_{Input.Email}";
            if (!_cache.TryGetValue(cacheKey, out string storedOtp) || storedOtp != Input.Code)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không đúng hoặc đã hết hạn.");
                return Page();
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, Input.Password);

            if (result.Succeeded)
            {
                // Remove OTP from cache
                _cache.Remove(cacheKey);

                TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
                return RedirectToPage("./Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}