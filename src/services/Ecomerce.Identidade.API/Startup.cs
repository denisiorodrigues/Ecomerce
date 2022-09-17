using Ecomerce.Identidade.API.Data;
using Ecomerce.Identidade.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace Ecomerce.Identidade.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var appSettingSection = Configuration.GetSection("AppSettings");
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

            services.AddControllers();

            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Ecomerce Identidade API",
                    Description = "Está API faz parte dos estudos e para compor meu portifólio",
                    Contact = new OpenApiContact() { Name = "Denisio Rodrigues", Email = "denisio@ymail.com" },
                    License = new OpenApiLicense() { Name = "MIT", Url= new Uri("https://opensource.org/licenses/MIT") }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", "v1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
