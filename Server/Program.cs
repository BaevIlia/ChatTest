
using Server.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<User> users = new List<User>
            {
                new User {Id =1, Name = "Игорь", Login ="ivanov_ii", Password ="123", Token ="" }
            };
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        ValidateIssuerSigningKey = true
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // если запрос направлен хабу
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                            {
                                // получаем токен из строки запроса
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });


            builder.Services.AddSignalR();
            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseDefaultFiles();
            app.UseStaticFiles();

         

            app.MapGet("/", () => "Hello World!");

            app.MapPost("/login", (User logUser) =>
            {
                User? user = users.FirstOrDefault(p => p.Login.Equals(logUser.Login) && p.Password.Equals(logUser.Password));
                if (user is null)
                {
                    return Results.Unauthorized();
                }

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, logUser.Login) };

                var jwt = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);


                var responce = new
                {
                    access_token = encodedJwt,
                    username = user.Login
                };


                return Results.Json(responce);
            });

            app.MapGet("/user/{id}", (int id) =>
            {
                if (id == 1)
                {
                    return Results.Json(users[0]);
                }
                else 
                {
                   return Results.Json(1);
                }
            });

            app.MapHub<ChatHub>("/chat");
            app.Run();

         
    }
        public class AuthOptions
        {
            public const string ISSUER = "MyAuthServer"; // издатель токена
            public const string AUDIENCE = "MyAuthClient"; // потребитель токена
            const string KEY = "mysupersecret_secretsecretsecretkey!123";   // ключ для шифрации
            public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}