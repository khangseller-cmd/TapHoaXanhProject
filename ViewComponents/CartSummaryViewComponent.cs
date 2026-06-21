using Microsoft.AspNetCore.Mvc;
using NguyenDinhMinhKhang_2380600989.Services;
using System.Security.Claims;

namespace NguyenDinhMinhKhang_2380600989.ViewComponents
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly CartService _cartService;

        public CartSummaryViewComponent(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy userId từ HttpContext
            var userId = HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartCount = await _cartService.GetCartItemCountAsync(userId);
            return View(cartCount);
        }
    }
}