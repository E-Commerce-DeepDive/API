using System.Net;
using System.Net.Mail;

using Ecommerce.DataAccess.ApplicationContext;
using Ecommerce.DataAccess.Services.Admin;
using Ecommerce.DataAccess.Services.Auth;
using Ecommerce.DataAccess.Services.Cart;
using Ecommerce.DataAccess.Services.Category;
using Ecommerce.DataAccess.Services.Email;
using Ecommerce.DataAccess.Services.ImageUploading;
using Ecommerce.DataAccess.Services.OAuth;
//using Ecommerce.DataAccess.Services.OTP;
using Ecommerce.DataAccess.Services.Token;
using Ecommerce.DataAccess.Services.Wishlist;
using Ecommerce.Utilities.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.DataAccess.Extensions
{
    public static class DataAccessServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<EcommerceContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DevCS")));

            return services;
        }
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            //services.AddScoped<IOTPService, OTPService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();
            services.AddScoped<ITokenStoreService, TokenStoreService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthGoogleService, AuthGoogleService>();
            // services.AddScoped<IWishlistService, WishlistService>();
            // services.AddScoped<IReviewService, ReviewService>();
             services.AddScoped<ICategoryService, CategoryService>();
            // services.AddScoped<IProductService, ProductService>();
             services.AddScoped<IWishlistService, WishlistService>();    
            // services.AddScoped<IOrderService, OrderService>();
             services.AddScoped<ICartService, CartService>();
            services.AddScoped<IAdminService, AdminService>();
            return services;
        }

        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();

            services.AddFluentEmail(emailSettings.FromEmail)
                .AddSmtpSender(new SmtpClient(emailSettings.SmtpServer)
                {
                    Port = emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
                    EnableSsl = emailSettings.EnableSsl
                });

            return services;
        }
    }
}
