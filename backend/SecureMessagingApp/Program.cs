using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecureMessagingApp.Extensions;
using SecureMessagingApp.Hubs;
using SecureMessagingApp.Models;
using SecureMessagingApp.Services;
using SecureMessagingApp.Shared;

namespace SecureMessagingApp;

public class Program
{
    public static void Main(string[] args)
    {
        const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: myAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader();
                });
        });

        builder.Services.AddControllers();
        builder.Services.AddOpenApiDocument();
        builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSignalR(options => { options.EnableDetailedErrors = true; });

        builder.Services.AddDbContextPool<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException()))
            };
        });

        builder.Services.AddScoped<ITokenService, TokenService>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();


        WebApplication app = builder.Build();

        // app.UseResponseCompression();


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseOpenApi();
            app.UseSwaggerUi();

            app.ApplyMigrations();
        }

        // Warm up critical services
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;

            // Warm up the database connection and model
            var db = services.GetRequiredService<AppDbContext>();
            db.Database.OpenConnection();
            _ = db.Users.Any(); // Force model compilation

            // Warm up other services (e.g., JWT validation)
            var jwtBearerOptions = services.GetRequiredService<IOptions<JwtBearerOptions>>();
            _ = jwtBearerOptions.Value.TokenValidationParameters;
        }

        // app.UseHttpsRedirection();

        app.UseCors(myAllowSpecificOrigins);

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<ChatHub>(HubRoutes.ChatHub);

        app.Run();
    }
}