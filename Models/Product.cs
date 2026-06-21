using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NguyenDinhMinhKhang_2380600989.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Display(Name = "Số lượng tồn")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [StringLength(500)]
        [Display(Name = "Ảnh đại diện")]
        public string? MainImage { get; set; }

        [StringLength(500)]
        [Display(Name = "Video giới thiệu")]
        public string? VideoUrl { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        [Display(Name = "Danh mục")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}