# 🛒 E-Commerce Microservices Architecture

## 📝 Giới thiệu
Dịch vụ xử lý nghiệp vụ trung tâm cho hệ sinh thái thương mại điện tử Microservices. Tài liệu này cung cấp các tiêu chuẩn về triển khai logic, bảo mật và giao tiếp liên dịch vụ được áp dụng xuyên suốt hệ thống.

### 🔗 Core Security & Implementation (Liên kết kỹ thuật trọng tâm)

> **Tổng quan dự án xem tại đây:** [Xem đầy đủ kiến trúc tại đây](https://github.com/nguyenthinh28902/mini-project-ecommerce)

Để đi sâu vào các cấu hình bảo mật hệ thống, bạn có thể tham khảo trực tiếp tại các module sau:

* **Client Security:** Triển khai OIDC Middleware, quản lý Secure Cookie và luồng Challenge.
  * [Cấu hình tại Web CMS](https://github.com/nguyenthinh28902/ecommerce-cms-web)
* **Identity Provider:** Định nghĩa Resource, Scope và Custom Profile Service để mapping Claims.
  * [Cấu hình tại Identity Server](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms)
* **API Gateway (YARP):** Quản lý Reverse Proxy Routing và thiết lập Auth Policy tập trung.
  * [Cấu hình tại Gateway CMS](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms)
* **Resource Server:** Cấu hình JWT Bearer và phân quyền dựa trên Policy (Policy-based Authorization).
  * [Cấu hình tại Product Service](https://github.com/nguyenthinh28902/Ecom.ProductService)

---
## 🛠 Công nghệ cốt lõi
* **Core Framework:** .NET 8 API, gRPC.
* **Message Broker:** RabbitMQ (MassTransit).
* **Security:** IdentityServer4 (OIDC & OAuth 2.0).
* **Caching:**  Cache.

## 🔐 Cơ chế Bảo mật & Xác thực
Dịch vụ hỗ trợ xác thực đa nguồn (Multi-scheme), cho phép nhận diện song song người dùng cuối (Web Portal) và các yêu cầu từ hệ thống nội bộ.

### 1. Cấu hình xác thực đa lớp (Hybrid Auth)
Tự động kiểm tra Token từ nguồn Web (WebScheme) và Hệ thống (Bearer).
*File code:* [AuthenticationExtensions.cs](https://github.com/nguyenthinh28902/Ecom.ProductService/blob/main/Ecom.ProductService/Common/Helpers/AuthenticationExtensions.cs).

```csharp
services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "WebScheme";
})
.AddJwtBearer("Bearer", options => { // Hệ thống nội bộ
    options.Authority = _internalAuth.Issuer;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidIssuer = _internalAuth.Issuer,
        ValidAudience = _internalAuth.Audience
    };
})
.AddJwtBearer("WebScheme", options => { // Web Portal người dùng
    options.Authority = _internalAuthWeb.Issuer;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidIssuer = _internalAuthWeb.Issuer,
        ValidAudience = _internalAuthWeb.Audience
    };
});
```

### 2. Phân quyền dựa trên Policy
Tách biệt quyền hạn rõ ràng giữa các luồng dữ liệu khác nhau.

```csharp
services.AddAuthorization(options => {
    options.AddPolicy(PolicyNames.ProductReadWeb, policy => {
        policy.AddAuthenticationSchemes("WebScheme");
        policy.AddRequirements(new InternalOrPermissionRequirement("product.read.web"));
    });

    options.AddPolicy(PolicyNames.ProductWrite, policy => {
        policy.AddAuthenticationSchemes("Bearer");
        policy.AddRequirements(new InternalOrPermissionRequirement("product.write"));
    });
});
```
## 🏗 Kiến trúc & Mẫu thiết kế
Dự án áp dụng mô hình 3-Layer Architecture kết hợp với Unit of Work & Repository Pattern để quản lý DB Transaction tập trung.

### Unit of Work & Repository Implementation
*File code:* [Repositories Folder](https://github.com/nguyenthinh28902/Ecom.ProductService/tree/main/Ecom.ProductService.Infrastructure/Repositories).

```csharp
try {
    var cart = await _unitOfWork.Repository<Cart>().GetById(customerId);
    await _unitOfWork.BeginTransactionAsync();

    // Mục đích: Quyết định tăng số lượng hoặc thêm mới item dựa trên trạng thái giỏ hàng
    _logger.LogInformation("Processing cart for customer: {Id}", customerId);

    if (existingItem != null) {
        existingItem.Quantity += request.Quantity;
        _unitOfWork.Repository<CartItem>().Update(existingItem);
    } else {
        await _unitOfWork.Repository<CartItem>().AddAsync(newItem);
    }

    await _unitOfWork.CommitAsync();
    return Result<bool>.Success(true);
} catch (Exception) {
    await _unitOfWork.RollbackAsync();
    return Result<bool>.Failure("Error processing data");
}
```

## 🛰 Tích hợp gRPC (High-Performance Communication)

### 1. Bảo mật phía Client
Tự động đính kèm User Context và Internal API Key vào Metadata của mỗi request.
*File code:* [GrpcClientExtensions.cs](https://github.com/nguyenthinh28902/ecom-order-service/blob/main/Ecom.OrderService.Application/Common/Extension/GrpcClientExtensions.cs)

```csharp
return builder.AddCallCredentials((context, metadata, serviceProvider) => {
    // Tự động đính kèm User ID/Email nếu người dùng đã đăng nhập
    if (currentUser.IsAuthenticated) {
        metadata.Add("X-User-Id", currentUser.UserId.ToString());
        metadata.Add("X-User-Email", currentUser.Email);
    }
    // Đính kèm API Key nội bộ từ cấu hình hệ thống
    metadata.Add("x-internal-key", configuration["InternalGrpcApiKey"]);
    return Task.CompletedTask;
});
```

### 2. Cấu hình & Đăng ký Client
*File code:* [DependencyInjectionWebApplication.cs](https://github.com/nguyenthinh28902/ecom-order-service/blob/main/Ecom.OrderService.Application/DependencyInjection/DependencyInjectionWebApplication.cs)

```csharp
// Đăng ký các service client kèm cấu hình bảo mật chung
services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(o => o.Address = new Uri(productUrl))
        .AddCommonCallCredentials(configuration);

services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o => o.Address = new Uri(paymentUrl))
        .AddCommonCallCredentials(configuration);
```

### 3. Xử lý phía Server (Interceptors)
Kiểm tra API Key nội bộ để đảm bảo request đến từ các service hợp lệ.
*File code:* [GrpcApiKeyInterceptor.cs](https://github.com/nguyenthinh28902/ecom-payment/blob/main/Ecom.PaymentService.Api/Common/Requirement/GrpcApiKeyInterceptor.cs)

```csharp
public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(...) {
    // Kiểm tra tính hợp lệ của Internal API Key từ header
    var headerKey = context.RequestHeaders.GetValue("x-internal-key");
    if (headerKey != _apiKey) {
        throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid API Key"));
    }
    return await continuation(request, context);
}
```

## ✉️ Tích hợp RabbitMQ & MassTransit (Async Messaging)

### 1. Cấu hình hạ tầng
Thiết lập kết nối Host và đăng ký các Consumer xử lý tin nhắn.
*File code:* [RabbitMQInfrastructure.cs](https://github.com/nguyenthinh28902/ecom-notification-service/blob/main/Ecom.Notification.Application/DependencyInjection/RabbitMQInfrastructure.cs)

```csharp
services.AddMassTransit(x => {
    x.AddConsumers(typeof(NotificationConsumer).Assembly);
    x.UsingRabbitMq((context, cfg) => {
        cfg.Host(settings.Host, h => {
            h.Username(settings.UserName);
            h.Password(settings.Password);
        });
        cfg.ConfigureEndpoints(context);
    });
});
```

### 2. Triển khai Consumer & Retry Policy
Đảm bảo tin nhắn được xử lý tin cậy với cơ chế thử lại tự động.
*File code:* [NotificationConsumer.cs](https://github.com/nguyenthinh28902/ecom-notification-service/blob/main/Ecom.Notification.Application/Service/Consumer/NotificationConsumer.cs)

```csharp
// Cấu hình thử lại: 3 lần, mỗi lần cách nhau 5 giây trước khi vào Queue lỗi
endpointConfigurator.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

// Logic gửi thông báo bất đồng bộ
public async Task Consume(ConsumeContext<NotificationRequestDto> context) {
    await _notificationService.DispatchNotificationAsync(context.Message);
}
```
