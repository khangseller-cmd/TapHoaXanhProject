using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Services;

namespace NguyenDinhMinhKhang_2380600989.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _emailService = emailService;
            _cache = cache;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Tìm user theo email
                var user = await _userManager.FindByEmailAsync(Input.Email);

                // Kiểm tra user tồn tại và email đã được xác nhận
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Tạo mã OTP 6 số ngẫu nhiên
                    var otpCode = new Random().Next(100000, 999999).ToString();

                    // Lưu OTP vào cache với thời hạn 15 phút
                    var cacheKey = $"password_reset_{Input.Email}";
                    _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(15));

                    // Gửi email chứa mã OTP
                    await _emailService.SendPasswordResetCodeAsync(Input.Email, otpCode);

                    // Chuyển đến trang nhập mã OTP
                    TempData["Success"] = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.";
                    return RedirectToPage("./ResetPassword", new { email = Input.Email });
                }

                // Không hiển thị lỗi cụ thể để tránh lộ thông tin user
                ModelState.AddModelError(string.Empty, "Vui lòng kiểm tra lại email hoặc xác nhận email đã được kích hoạt.");
            }

            return Page();
        }
    }
}