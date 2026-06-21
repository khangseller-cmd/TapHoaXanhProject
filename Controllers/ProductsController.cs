using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Data;
using NguyenDinhMinhKhang_2380600989.Models;
using NguyenDinhMinhKhang_2380600989.Services;

namespace NguyenDinhMinhKhang_2380600989.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products - Cả Admin và Staff đều xem được
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View(products);
        }

        // GET: Products/Create - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View();
        }

        // POST: Products/Create - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product, IFormFile? mainImage, List<IFormFile>? productImages)
        {
            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

            if (ModelState.IsValid)
            {
                product.Slug = SlugHelper.GenerateSlug(product.Name);

                if (mainImage != null && mainImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await mainImage.CopyToAsync(stream);
                    }

                    product.MainImage = "/uploads/products/" + fileName;
                }

                product.CreatedAt = DateTime.Now;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (productImages != null)
                {
                    int displayOrder = 1;
                    foreach (var image in productImages)
                    {
                        if (image.Length > 0)
                        {
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                            Directory.CreateDirectory(uploadsFolder);

                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = "/uploads/products/" + fileName,
                                DisplayOrder = displayOrder++
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Edit/5 - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            return View(product);
        }

        // POST: Products/Edit/5 - CHỈ ADMIN
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? mainImage)
        {
            ViewBag.Categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    product.Slug = SlugHelper.GenerateSlug(product.Name);
                    product.UpdatedAt = DateTime.Now;

                    if (mainImage != null && mainImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImage.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await mainImage.CopyToAsync(stream);
                        }

                        product.MainImage = "/uploads/products/" + fileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Delete/5 - CHỈ ADMIN
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5 - CHỈ ADMIN
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // SOFT DELETE - Không xóa thật, chỉ đánh dấu
                product.IsDeleted = true;
                product.DeletedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}