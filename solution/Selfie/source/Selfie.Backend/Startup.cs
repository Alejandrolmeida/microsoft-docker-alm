using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Selfie.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;

namespace Selfie.Backend
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
            services.AddSingleton(provider => Configuration);

            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowAll"));
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });

            services.AddMvc();

            //Config BBDD
            var connection = $"Server={Configuration["mssql:server"]};Database={Configuration["mssql:database"]};User Id={Configuration["mssql:userId"]};Password={Configuration["mssql:password"]};";
            services.AddDbContext<BootCampContext>(options => options.UseSqlServer(connection));

            //Swagger config
            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Selfie Backend",
                    Description = "A simple backend API for Selfie Solution",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "Alejandro Almeida", Email = "", Url = "https://twitter.com/alejandrolmeida" },
                    License = new License { Name = "Use under MIT License", Url = "https://opensource.org/licenses/MIT" }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //// Shows UseCors with named policy.
            //app.UseCors("AllowAll");
            app.UseCors("AllowAll");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BootCamp Assitants V1");
            });

            app.UseMvc();
        }
    }
}