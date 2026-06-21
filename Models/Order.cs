using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NguyenDinhMinhKhang_2380600989.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        Shipping,
        Completed,
        Cancelled
    }

    [Table("Orders")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(200)]
        [Display(Name = "Người nhận")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(500)]
        [Display(Name = "Địa chỉ")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(1000)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}