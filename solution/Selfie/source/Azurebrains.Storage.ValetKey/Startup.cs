using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Swashbuckle.AspNetCore.Swagger;

namespace Azurebrains.Storage.ValetKey
{   
    public static class Azure
    {
        public static CloudStorageAccount StorageAccount { get; set; }
    }

    public class Startup
    {
        #region Constructors
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // EndPoint AZURE Emulator
            //storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");

            var host = configuration["storage:host"];
            var account = configuration["storage:account"];
            var key = configuration["storage:key"];

            if (bool.Parse(configuration["storage:emulator"]))
            {
                Azure.StorageAccount = new CloudStorageAccount(
                                        new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account, key),
                                        new Uri(host + @":10000/devstoreaccount1"),
                                        new Uri(host + @":10001/devstoreaccount1"),
                                        new Uri(host + @":10002/devstoreaccount1"),
                                        new Uri(host + @":10003/devstoreaccount1"));
            }
            else
            {
                Azure.StorageAccount = new CloudStorageAccount(
                                        new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(account, key), true);
            }

            // Configure CORS
            //InitializeCors(storageAccount.CreateCloudBlobClient()); 
        }

        private static void InitializeCors(CloudBlobClient BlobClient)
        {
            // CORS should be enabled once at service startup
            // Given a BlobClient, download the current Service Properties 
            ServiceProperties blobServiceProperties = BlobClient.GetServicePropertiesAsync().Result;

            // Enable and Configure CORS
            ConfigureCors(blobServiceProperties);

            // Commit the CORS changes into the Service Properties
            BlobClient.SetServicePropertiesAsync(blobServiceProperties);
        }

        private static void ConfigureCors(ServiceProperties serviceProperties)
        {
            serviceProperties.Cors = new CorsProperties();
            serviceProperties.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedHeaders = new List<string>() { "*" },
                AllowedMethods = CorsHttpMethods.Put | CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Post,
                AllowedOrigins = new List<string>() { "*" },
                ExposedHeaders = new List<string>() { "*" },
                MaxAgeInSeconds = 1800 // 30 minutes
            });
        }
        #endregion

        #region PROPERTIES
        public IConfiguration Configuration { get; }

        #endregion

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

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Valet Key Demo",
                    Description = "A simple example Valet Key pattern implementation",
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

            app.UseCors("AllowAll");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc();
        }
    }
}
