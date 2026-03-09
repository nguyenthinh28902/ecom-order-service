using AutoMapper;
using Ecom.OrderService.Core.Entities;
using Ecom.OrderService.Core.Enum;
using Ecom.OrderService.Core.Models.Web.Dtos.Checkout;
using Ecom.OrderService.Core.Models.Web.Dtos.Order;
using Ecom.PaymentService.Grpc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.AutoMappings
{
    public class ApplicatinoOrderWebProfile : Profile
    {
        public ApplicatinoOrderWebProfile() {

            CreateMap<Order, OrderDto>()
            // Chỉ comment dòng quan trọng: Tự động chuyển đổi byte Status sang chuỗi tên Enum trong Mapping
            .ForMember(dest => dest.StatusName,
                       opt => opt.MapFrom(src => src.Status.HasValue
                           ? ((OrderStatus)src.Status.Value).ToString()
                           : string.Empty));
            CreateMap<OrderItem, OrderItemDto>();

            //danh sach
            
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.HasValue
                    ? ((OrderStatus)src.Status.Value).ToString() : string.Empty));

            CreateMap<OrderItem, OrderItemSummaryDto>();

            // Map từ CartItem entity sang Dto
            CreateMap<CartItem, CheckoutItemDto>()
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // Giá sẽ lấy từ gRPC
                .ForMember(dest => dest.TotalLine, opt => opt.Ignore());

            // Map từ Cart sang CheckoutDto
            CreateMap<Cart, CheckoutDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            // Map từ gRPC sang DTO
            CreateMap<PaymentGrpcResponse, PaymentResultDto>();

            // Nếu ông cần map ngược lại (ít dùng nhưng cứ để đây)
            CreateMap<PaymentResultDto, PaymentGrpcResponse>();
        }
    }
}
