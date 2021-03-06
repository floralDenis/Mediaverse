using System;
using System.Reflection;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Lib.AspNetCore.ServerSentEvents;
using MediatR;
using Mediaverse.Application.Authentication.Services;
using Mediaverse.Application.Common.Services;
using Mediaverse.Domain.Authentication.Entities;
using Mediaverse.Domain.Authentication.Repositories;
using Mediaverse.Domain.ContentSearch.Services;
using Mediaverse.Domain.ContentSearch.Services.Implementation;
using Mediaverse.Domain.JointContentConsumption.Repositories;
using Mediaverse.Infrastructure.Authentication.Repositories;
using Mediaverse.Infrastructure.Authentication.Services;
using Mediaverse.Infrastructure.Common.Persistence;
using Mediaverse.Infrastructure.Common.Services;
using Mediaverse.Infrastructure.ContentSearch.Repositories.YouTube;
using Mediaverse.Infrastructure.JointContentConsumption.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContentRepository = Mediaverse.Infrastructure.ContentSearch.Repositories.ContentRepository;
using IContentRepository = Mediaverse.Domain.ContentSearch.Repositories.IContentRepository;

namespace Mediaverse.Web
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
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
            
            services.AddMediatR(Assembly.Load("Mediaverse.Application"));
            services.AddAutoMapper(Assembly.Load("Mediaverse.Application"));
            services.AddAutoMapper(Assembly.Load("Mediaverse.Infrastructure"));
            services.AddLogging();

            services.AddServerSentEvents(options =>
            {
                options.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;
            });
            
            services.AddIdentity<User, IdentityRole<Guid>>(
                    options =>
                    {
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequiredUniqueChars = 3;
                    }).AddEntityFrameworkStores<ApplicationDbContext>();
            
            services.AddDbContext<ApplicationDbContext>();
            services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
            services.AddScoped(typeof(IIdentifierProvider), typeof(DefaultIdentifierProvider));
            services.AddScoped(typeof(INameGenerator), typeof(NameGenerator));
            services.AddScoped(typeof(IEmailService), typeof(EmailService));
            services.AddScoped(typeof(IViewerRepository), typeof(ViewerRepository));
            services.AddScoped(typeof(IRoomRepository), typeof(RoomRepository));
            services.AddScoped(typeof(IContentRepository), typeof(ContentRepository));
            services.AddScoped(typeof(Mediaverse.Domain.JointContentConsumption.Repositories.IContentRepository), 
                typeof(Mediaverse.Infrastructure.JointContentConsumption.Repositories.ContentRepository));
            services.AddScoped(typeof(IQueryStringProcessor), typeof(QueryStringProcessor));
            services.AddScoped(typeof(IPlaylistRepository), typeof(PlaylistRepository));
            services.AddScoped<YouTubeRepository>();

            var authInitializer = new BaseClientService.Initializer
            {
                ApiKey = "",
                ApplicationName = ""
            };
            services.AddScoped(x => new YouTubeService(authInitializer));
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                app.MapServerSentEvents("/default-sse-endpoint");
                
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}