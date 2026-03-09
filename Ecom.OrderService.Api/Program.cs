using Ecom.orderService.Common.Helpers;
using Ecom.OrderService.Common.DependencyInjection;
using Ecom.OrderService.Common.Extensions;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;
// Add services to the container.
builder.Services.AddControllers();
//configure appsettings
builder.Services.AddConfigAppSettingExtensions(builder.Configuration);
//configure swagger gen
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGenConfiguration(builder.Configuration);
//Authentication
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthenticationExtensions(builder.Configuration);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// apication DI
builder.Services.AddApplicationDI(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DisplayRequestDuration());
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
