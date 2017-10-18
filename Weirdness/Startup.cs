using System.Net;
using Funq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Data;
using ServiceStack.Mvc;
using ServiceStack.OrmLite;

namespace Weirdness
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
            services.AddMvc();
            services.AddSingleton<IDbConnectionFactory>(
                new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseServiceStack(new AppHost());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() :
            base("Weird Al", typeof(SomeService).Assembly)
        {
        }


        public override void Configure(Container container)
        {
            Plugins.Add(new RazorFormat());

            //Works but recommend handling 404 at end of .NET Core pipeline
            //this.CustomErrorHttpHandlers[HttpStatusCode.NotFound] = new RazorHandler("/notfound");
            this.CustomErrorHttpHandlers[HttpStatusCode.Unauthorized] = new RazorHandler("/login");
            
            Plugins.Add(new AutoQueryFeature { MaxLimit = 100 });

            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
                new IAuthProvider[] { 
                    new BasicAuthProvider(), //Sign-in with HTTP Basic Auth
                    new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
                })
            {
                IncludeAssignRoleServices = false,
                IncludeRegistrationService = false,
                GenerateNewSessionCookiesOnAuthentication = true,
                DeleteSessionCookiesOnLogout = true
            });

            Plugins.Add(new RegistrationFeature());

            container.Register<ICacheClient>(new MemoryCacheClient());
            var userRep = new InMemoryAuthRepository();
            container.Register<IUserAuthRepository>(userRep);

            userRep.CreateUserAuth(new UserAuth
            {
                FirstName = "Jonathan",
                LastName = "Someone",
                DisplayName = "Jonno",
                Email = "john@example.com"
            }, "Secure1");

            SetConfig(new HostConfig
            {
                HandlerFactoryPath = "api",
                ApiVersion = "1"
            });
        }
    }

    public class SomeService : Service
    {
        
    }
}
