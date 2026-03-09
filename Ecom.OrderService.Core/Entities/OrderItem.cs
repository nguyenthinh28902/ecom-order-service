using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.OrderService.Core.Entities;

public partial class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int VariantId { get; set; }

    [StringLength(255)]
    public string ProductName { get; set; } = null!;

    [StringLength(255)]
    public string ProductMainImage { get; set; } = null!;

    [Column("SKU")]
    [StringLength(50)]
    [Unicode(false)]
    public string Sku { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BasePrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CurrentPrice { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? PromotionDiscount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? MemberDiscount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CouponDiscount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(29, 2)")]
    public decimal? TotalLineAmount { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order Order { get; set; } = null!;
}
