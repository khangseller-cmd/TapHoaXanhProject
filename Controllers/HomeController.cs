using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Models;

namespace NguyenDinhMinhKhang_2380600989.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Products(int? categoryId, string? search)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.StockQuantity > 0);

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            var products = await query.ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = search;

            return View(products);
        }

        // Action tìm kiếm và chuyển hướng
        [HttpPost]
        public async Task<IActionResult> QuickSearch(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return RedirectToAction(nameof(Products));
            }

            // Tìm sản phẩm trùng khớp chính xác hoặc gần đúng
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => !p.IsDeleted &&
                    (p.Name.ToLower().Contains(search.ToLower()) ||
                     p.Slug.ToLower() == search.ToLower().Replace(" ", "-")));

            if (product != null)
            {
                // Nếu có sản phẩm → chuyển đến trang chi tiết
                return RedirectToAction(nameof(Detail), new { id = product.Id });
            }
            else
            {
                // Nếu không có → chuyển đến trang danh sách với thông báo
                TempData["SearchNotFound"] = $"Không tìm thấy sản phẩm nào có tên \"{search}\"";
                return RedirectToAction(nameof(Products), new { search = search });
            }
        }

        public async Task<IActionResult> Detail(int id, string? slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            // Nếu slug không khớp, redirect đến slug đúng
            if (!string.IsNullOrEmpty(slug) && slug != product.Slug)
            {
                return RedirectToAction(nameof(Detail), new { id = product.Id, slug = product.Slug });
            }

            ViewBag.RelatedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && !p.IsDeleted)
                .Take(4)
                .ToListAsync();

            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}