using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Dashboard.Infrastructure.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dashboard.Web.Data;
using Dashboard.Web.Hubs;
using Dashboard.Web.Services;

namespace Dashboard.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSignalR();
            services.AddSingleton<Channel<ConsumedKafkaMessage>>(_ => Channel.CreateBounded<ConsumedKafkaMessage>(10));
            services.AddInfrastructure(configuration: Configuration);
            services.AddHostedService<SignalRPublisher>();
            services.AddHostedService<KafkaConsumer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapHub<KafkaMessagesHub>("/hubs/kafka");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
