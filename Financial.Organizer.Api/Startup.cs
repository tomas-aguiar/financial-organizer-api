using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Registry;

namespace Financial.Organizer.Api
{
    public class Startup
    {
        private const string PollyCache = "PollyCache";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();

            services.AddSingleton<IReadOnlyPolicyRegistry<string>, PolicyRegistry>(provider =>
                {
                    var registry = new PolicyRegistry
                    {{PollyCache, Policy.CacheAsync(
                        provider
                            .GetRequiredService<IAsyncCacheProvider>()
                            .AsyncFor<HttpResponseMessage>(),
                        TimeSpan.FromMinutes(5))}};
                    return registry;
                });
            
            services.AddHttpClient("clientName", (provider, client) =>
            {
                var clientUri = provider.GetService<IConfiguration>().GetValue<string>("clientUri");
                client.BaseAddress = new Uri(clientUri);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}