using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.OrderService.Core.Entities;

[Index("OrderCode", Name = "UQ__Orders__999B52297D4390C1", IsUnique = true)]
public partial class Order
{
    [Key]
    public int Id { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string OrderCode { get; set; } = null!;

    public int CustomerId { get; set; }

    public int? UserId { get; set; }

    public int WorkplaceId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalDiscount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ShippingFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string? Currency { get; set; }

    [StringLength(255)]
    public string FullName { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string PhoneNumber { get; set; } = null!;

    [StringLength(500)]
    public string ShippingAddress { get; set; } = null!;

    public byte? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderStatusLog> OrderStatusLogs { get; set; } = new List<OrderStatusLog>();
}
