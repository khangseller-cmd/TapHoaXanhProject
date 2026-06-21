using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NguyenDinhMinhKhang_2380600989.Services;
using System.Security.Claims;

namespace NguyenDinhMinhKhang_2380600989.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Thêm sản phẩm vào giỏ
            await _cartService.AddToCartAsync(userId, productId, quantity);

            // Lấy tổng số lượng mới
            var cartCount = await _cartService.GetCartItemCountAsync(userId);

            // Nếu là AJAX request, trả về JSON
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    cartCount = cartCount,
                    message = "Đã thêm vào giỏ hàng!"
                });
            }

            // Nếu không phải AJAX, redirect như cũ
            TempData["Success"] = "Đã thêm vào giỏ hàng!";
            return RedirectToAction("Detail", "Home", new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);
            TempData["Success"] = "Đã xóa khỏi giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Vui lòng đăng nhập!";
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction(nameof(Index));
            }

            TempData["CartId"] = cart.Id;
            return RedirectToAction("Create", "Orders");
        }
    }
}