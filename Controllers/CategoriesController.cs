using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Models;

namespace NguyenDinhMinhKhang_2380600989.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null) return NotFound();

            // Kiểm tra có đơn hàng nào chứa sản phẩm của danh mục này không
            var hasOrders = _context.OrderDetails
                .Include(od => od.Product)
                .Any(od => od.Product.CategoryId == id && !od.Product.IsDeleted);

            ViewBag.HasOrders = hasOrders;
            ViewBag.ProductCount = category.Products.Count(p => !p.IsDeleted);

            return View(category);
        }

        // POST: Categories/Delete/5 - SOFT DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // SOFT DELETE - Chỉ đánh dấu đã xóa
                category.IsDeleted = true;
                category.DeletedAt = DateTime.Now;

                // Soft delete các sản phẩm thuộc danh mục này
                var products = await _context.Products
                    .Where(p => p.CategoryId == id && !p.IsDeleted)
                    .ToListAsync();

                foreach (var product in products)
                {
                    product.IsDeleted = true;
                    product.DeletedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa danh mục thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}