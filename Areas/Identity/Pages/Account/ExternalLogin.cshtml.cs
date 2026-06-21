// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NguyenDinhMinhKhang_2380600989.Data;

namespace NguyenDinhMinhKhang_2380600989.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public string? ProviderDisplayName { get; set; }

        [BindProperty]
        public string? ReturnUrl { get; set; }

        [BindProperty]
        public InputModel? Input { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string? Email { get; set; }
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            _logger.LogInformation("Redirecting to {Provider} with redirectUrl: {RedirectUrl}", provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            _logger.LogInformation("External login callback with returnUrl: {ReturnUrl}, remoteError: {RemoteError}",
                returnUrl, remoteError);

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                _logger.LogError("Failed to load external login info");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, create a new account
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                _logger.LogInformation("Creating account for email: {Email}", email);

                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        // User exists, add external login
                        var addLoginResult = await _userManager.AddLoginAsync(user, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("Added external login to existing user: {Email}", email);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            _logger.LogError("Failed to add login: {Errors}",
                                string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        // User doesn't exist, create new account
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true  // Google đã verify email rồi
                        };

                        var createResult = await _userManager.CreateAsync(newUser);
                        if (createResult.Succeeded)
                        {
                            var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
                            if (addLoginResult.Succeeded)
                            {
                                // Gán role Customer mặc định
                                await _userManager.AddToRoleAsync(newUser, "Customer");

                                await _signInManager.SignInAsync(newUser, isPersistent: false);
                                _logger.LogInformation("Created new user with external login: {Email}", email);
                                return LocalRedirect(returnUrl);
                            }
                            else
                            {
                                _logger.LogError("Failed to add login: {Errors}",
                                    string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to create user: {Errors}",
                                string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // If we got here, something failed
                ErrorMessage = "Failed to create account or add external login.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
        }
    }
}