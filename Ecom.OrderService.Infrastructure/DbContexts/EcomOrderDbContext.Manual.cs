using Ecom.OrderService.Core.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Infrastructure.DbContexts
{
    public partial class EcomOrderDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Đưa logic dùng biến hằng của bạn vào đây
                optionsBuilder.UseSqlServer(ConnectionStrings.EcomOrderConnectionString);
            }
        }
    }
}
