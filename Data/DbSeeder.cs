using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NguyenDinhMinhKhang_2380600989.Models;

namespace NguyenDinhMinhKhang_2380600989.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // 1. Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Thực phẩm", Description = "Đồ ăn các loại" },
                    new Category { Name = "Đồ uống", Description = "Nước uống các loại" },
                    new Category { Name = "Gia vị", Description = "Gia vị nấu ăn" },
                    new Category { Name = "Đồ gia dụng", Description = "Đồ dùng gia đình" },
                    new Category { Name = "Chăm sóc cá nhân", Description = "Sản phẩm vệ sinh" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 2. Seed Products
            if (!context.Products.Any())
            {
                var categories = context.Categories.ToList();
                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Gạo ST25",
                        Slug = "gao-st25",
                        Price = 25000,
                        StockQuantity = 100,
                        CategoryId = categories[0].Id,
                        Description = "Gạo thơm ngon nhất thế giới",
                        MainImage = "https://via.placeholder.com/400x400/2ecc71/ffffff?text=Gạo+ST25"
                    },
                    new Product
                    {
                        Name = "Nước mắm Phú Quốc",
                        Slug = "nuoc-mam-phu-quoc",
                        Price = 35000,
                        StockQuantity = 50,
                        CategoryId = categories[2].Id,
                        Description = "Nước mắm truyền thống",
                        MainImage = "https://via.placeholder.com/400x400/3498db/ffffff?text=Nước+mắm"
                    },
                    new Product
                    {
                        Name = "Coca Cola 1.5L",
                        Slug = "coca-cola-1-5l",
                        Price = 18000,
                        StockQuantity = 80,
                        CategoryId = categories[1].Id,
                        Description = "Nước ngọt có gas",
                        MainImage = "https://via.placeholder.com/400x400/e74c3c/ffffff?text=Coca+Cola"
                    },
                    new Product
                    {
                        Name = "Bánh mì tươi",
                        Slug = "banh-mi-tuoi",
                        Price = 5000,
                        StockQuantity = 30,
                        CategoryId = categories[0].Id,
                        Description = "Bánh mì mới ra lò",
                        MainImage = "https://via.placeholder.com/400x400/f39c12/ffffff?text=Bánh+mì"
                    }
                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }

            // 3. Create Roles
            string[] roleNames = { "Admin", "Staff", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 4. Create Admin
            var adminEmail = "admin@taphoa.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0909090909"
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 5. Create Staff
            var staffEmail = "staff@taphoa.com";
            var staffUser = await userManager.FindByEmailAsync(staffEmail);
            if (staffUser == null)
            {
                staffUser = new ApplicationUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0909090908"
                };
                var result = await userManager.CreateAsync(staffUser, "Staff@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staffUser, "Staff");
                }
            }

            // 6. Create Customer
            var customerEmail = "customer@taphoa.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0909090907"
                };
                var result = await userManager.CreateAsync(customerUser, "Customer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }
        }
    }
}