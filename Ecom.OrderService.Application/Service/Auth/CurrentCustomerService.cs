using Ecom.OrderService.Application.Interface.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.OrderService.Application.Service.Auth
{
    public class CurrentCustomerService : ICurrentCustomerService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentCustomerService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int Id // Sửa lại đúng chính tả từ CusomerId thành Id
        {
            get
            {
                // 1. Ưu tiên lấy từ Header do Gateway gán vào
                var headerId = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].ToString();
                if (int.TryParse(headerId, out int id)) return id;



                return 0; // Trả về 0 nếu là khách vãng lai
            }
        }

        /// <summary>
        /// tùy trường hợp khách hàng đăng ký bằng phương thức gì
        /// </summary>
        public string? EmailOrPhone
        {
            get
            {
                // 1. Lấy từ Header Gateway
                var EmailOrPhone = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Email"].ToString();
                if (string.IsNullOrEmpty(EmailOrPhone))
                {
                    EmailOrPhone = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Phone"].ToString();
                }


                return EmailOrPhone;
            }

        }
        public string? Email
        {
            get
            {
                // 1. Lấy từ Header Gateway
                var email = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Email"].ToString();
                return email;
            }
        }

        public string? PhoneNumber
        {
            get
            {
                // 1. Lấy từ Header Gateway
                var phone = _httpContextAccessor.HttpContext?.Request.Headers["X-User-Phone"].ToString();
                return phone;
            }
        }

        // Bổ sung thêm thuộc tính kiểm tra đăng nhập nhanh
        public bool IsAuthenticated => Id > 0;
    }
}
