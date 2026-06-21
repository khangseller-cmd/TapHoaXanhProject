using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Models;
using NguyenDinhMinhKhang_2380600989.Services;
using System.Security.Claims;

namespace NguyenDinhMinhKhang_2380600989.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public OrdersController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Orders - Admin/Staff xem tất cả, Customer chỉ xem của mình
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isStaff = User.IsInRole("Staff");

            IQueryable<Order> query;

            if (isAdmin || isStaff)
            {
                query = _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate);
            }
            else
            {
                query = _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate);
            }

            var orders = await query.ToListAsync();
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsStaff = isStaff;
            return View(orders);
        }

        // GET: Orders/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isStaff = User.IsInRole("Staff");

            if (!isAdmin && !isStaff && order.UserId != userId)
            {
                return Forbid();
            }

            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsStaff = isStaff;
            return View(order);
        }

        // GET: Orders/Create - Customer đặt hàng
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order orderModel)
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
                return RedirectToAction("Index", "Cart");
            }

            if (string.IsNullOrEmpty(orderModel.ReceiverName) ||
                string.IsNullOrEmpty(orderModel.Phone) ||
                string.IsNullOrEmpty(orderModel.ShippingAddress))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ thông tin giao hàng!";
                return View(orderModel);
            }

            var order = new Order
            {
                UserId = userId,
                ReceiverName = orderModel.ReceiverName,
                Phone = orderModel.Phone,
                ShippingAddress = orderModel.ShippingAddress,
                Notes = orderModel.Notes,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product!.Price * ci.Quantity),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.CartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductNameSnapshot = item.Product!.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });

                item.Product.StockQuantity -= item.Quantity;
            }

            await _cartService.ClearCartAsync(userId);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt hàng thành công!";
            return RedirectToAction(nameof(Detail), new { id = order.Id });
        }

        // POST: Orders/UpdateStatus - ADMIN VÀ STAFF đều cập nhật được
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Orders/DeleteOrder - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Xóa chi tiết đơn hàng trước
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa đơn hàng thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}