using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecom.OrderService.Core.Entities;

[Index("OrderId", Name = "IX_OrderStatusLogs_OrderId")]
public partial class OrderStatusLog
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public byte? OldStatus { get; set; }

    public byte NewStatus { get; set; }

    public string? Note { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderStatusLogs")]
    public virtual Order Order { get; set; } = null!;
}
