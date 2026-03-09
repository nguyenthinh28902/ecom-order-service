using AutoMapper;
using Ecom.OrderService.Core.Entities;
using Ecom.OrderService.Core.Enum;
using Ecom.OrderService.Core.Models.Cms.Dtos.Order;
using Ecom.OrderService.Core.Models.Dto.Cms;
using Ecom.PaymentService.Grpc;


namespace Ecom.OrderService.Application.AutoMappings
{
    public class ApplicatinoOrderCmsProfile : Profile
    {
        public ApplicatinoOrderCmsProfile() {

            CreateMap<Order, OrderManagerDto>()
            // Chỉ comment dòng quan trọng: Tự động chuyển đổi byte Status sang chuỗi tên Enum trong Mapping
            .ForMember(dest => dest.StatusName,
                       opt => opt.MapFrom(src => src.Status.HasValue
                           ? ((OrderStatus)src.Status.Value).ToString()
                           : string.Empty));
            CreateMap<OrderItem, OrderItemManagerDto>();

            //danh sach
            
            CreateMap<Order, OrderSummaryManagerDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.HasValue
                    ? ((OrderStatus)src.Status.Value).ToString() : string.Empty));

            CreateMap<OrderItem, OrderItemSummaryManagerDto>();

            CreateMap<TransactionLogManagerGrpc, TransactionLogManagerDto>()
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
        src.CreatedAt != null ? src.CreatedAt.ToDateTime() : (DateTime?)null));

            CreateMap<TransactionManagerGrpcResponse, TransactionManagerDto>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => (decimal)src.Amount))

                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src =>
                    src.CreatedAt != null ? src.CreatedAt.ToDateTime() : (DateTime?)null))

                .ForMember(dest => dest.FinishedAt, opt => opt.MapFrom(src =>
                    src.FinishedAt != null ? src.FinishedAt.ToDateTime() : (DateTime?)null))

                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (byte?)src.Status))

                .ForMember(dest => dest.TransactionLog, opt => opt.MapFrom(src => src.TransactionLog));

        }
    }
}
