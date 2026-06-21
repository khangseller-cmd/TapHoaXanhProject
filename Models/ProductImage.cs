using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NguyenDinhMinhKhang_2380600989.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Thứ tự")]
        public int DisplayOrder { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}