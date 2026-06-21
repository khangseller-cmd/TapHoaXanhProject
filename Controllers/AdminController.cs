using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Models;

namespace NguyenDinhMinhKhang_2380600989.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Dashboard Admin
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.LowStockProducts = await _context.Products
                .Where(p => !p.IsDeleted && p.StockQuantity < 10)
                .ToListAsync();

            return View();
        }

        // Quản lý người dùng
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = string.Join(", ", roles)
                });
            }

            return View(userList);
        }

        // Thêm nhân viên mới
        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(AddStaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Staff");
                    TempData["Success"] = "Thêm nhân viên thành công!";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        // Xóa nhân viên - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStaff(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Staff"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Staff");
                    await _userManager.DeleteAsync(user);
                    TempData["Success"] = "Đã xóa nhân viên!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa người dùng này!";
                }
            }
            return RedirectToAction(nameof(Users));
        }

        // Thống kê doanh thu
        public async Task<IActionResult> Statistics()
        {
            var orders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Completed)
                .ToListAsync();

            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
            ViewBag.TotalOrders = orders.Count;
            ViewBag.AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;

            // Doanh thu theo tháng
            ViewBag.MonthlyRevenue = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(m => m.Year).ThenBy(m => m.Month)
                .ToList();

            return View();
        }
    }

    // ViewModels
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
    }

    public class AddStaffViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Total { get; set; }
        public int OrderCount { get; set; }
    }
}