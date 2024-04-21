using Api.Data;
using Api.Hubs;
using Api.Infrastructure;
using Api.Services;
using Api.Utils;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json.Serialization;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; 
});

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 104857600;
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.PayloadSerializerOptions.WriteIndented = true;
}); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

TokenOptions.Initialize(builder.Configuration);

var myAuthenticationScheme = "JWT_OR_COOKIE";
builder.Services.AddAuthentication(myAuthenticationScheme)
    .AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = TokenOptions.ISSUER,
        ValidateAudience = true,
        ValidAudience = TokenOptions.AUDIENCE,
        ValidateLifetime = true,
        IssuerSigningKey = TokenOptions.GetSymmeetricSecurityKey(),
        ValidateIssuerSigningKey = true,
    };
})
    .AddCookie(options =>
    {
        options.Cookie.Name = "AuthorizeCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.Path = "/";
    }).AddPolicyScheme(myAuthenticationScheme, myAuthenticationScheme, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            string authorization = context.Request.Headers[HeaderNames.Authorization];
            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                return "Bearer";

            return "Cookies";
        };
    });



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator", policy => policy.RequireClaim(ClaimTypes.Role, "Administrator"));
    options.AddPolicy("User", policy => policy.RequireClaim(ClaimTypes.Role, "User"));
});

var MyPolicy = "MyPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyPolicy,
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                      });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

DependencyInjection.RegisterDependencies(builder.Services);

builder.Services.AddHostedService<UserCleanupService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(MyPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
//app.MapHub<CallHub>("/callHub");

app.Run();