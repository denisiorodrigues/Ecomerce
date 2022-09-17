using Ecomerce.Identidade.API.Data;
using Ecomerce.Identidade.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ecomerce.Identidade.API.Configurations
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddErrorDescriber<IdentityTranslatePtBr>() // Tradução
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var appSettingSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingSection);

            var appSettings = appSettingSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            //JTW
            //Configurando o modelo de nautenticação para usar o JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                bearerOptions.RequireHttpsMetadata = true;//somente acesso ao http
                bearerOptions.SaveToken = true;//Salvar o token na instância assim que o login for realizado comc uscesso
                bearerOptions.TokenValidationParameters = new TokenValidationParameters //Configuyração do token
                {
                    ValidateIssuerSigningKey = true,//Validar o emissor com base na assinatura
                    IssuerSigningKey = new SymmetricSecurityKey(key),//Criando o emissor
                    ValidateIssuer = true, // Validar sendo da API que foi configurada
                    ValidateAudience = true, // Onde esse token vai ser válido(em que site)
                    ValidAudience = appSettings.ValidadoEm, //Criando a audiencia
                    ValidIssuer = appSettings.Emissor //Criando um Issuer
                };
            });
            //Fim da documentação

            return services;
        }

        public static IApplicationBuilder UseIdentityConfig(this IApplicationBuilder app)
        {
            app.UseAuthorization();
            app.UseAuthentication();

            return app;
        }
    }
}
