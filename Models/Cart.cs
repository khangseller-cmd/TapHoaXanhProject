using System.ComponentModel.DataAnnotations.Schema;

namespace NguyenDinhMinhKhang_2380600989.Models
{
    [Table("Carts")]
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}