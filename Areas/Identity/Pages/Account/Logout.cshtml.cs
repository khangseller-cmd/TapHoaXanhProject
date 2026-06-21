using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NguyenDinhMinhKhang_2380600989.Data;
using Microsoft.AspNetCore.Authentication;

namespace NguyenDinhMinhKhang_2380600989.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {

            // 1. Đăng xuất khỏi ứng dụng Identity
            await _signInManager.SignOutAsync();

            // 2. Xóa external authentication cookie (nếu có)
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // 3. Xóa tất cả cookies authentication
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            _logger.LogInformation("User logged out.");

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}