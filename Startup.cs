using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetcoreVuePwa.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace NetcoreVuePwa
{
  public class Startup
  {
    public Startup(IConfiguration configuration, IHostingEnvironment env)
    {
      Configuration = configuration;
      HostingEnvironment = env;
    }

    public IConfiguration Configuration { get; }
    public IHostingEnvironment HostingEnvironment { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services
      .AddMvc()
      .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
      .AddJsonOptions(opts =>
      {
        // camelCase all JSON results
        opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        // serialize enums to strings instead of numbers
        opts.SerializerSettings.Converters.Add(new StringEnumConverter());
      });

      // configure aspnet core framework behaviors
      services.Configure<ApiBehaviorOptions>(opts =>
      {
        // change the default invalid model error to be less lame
        opts.InvalidModelStateResponseFactory = ctx =>
        {
          var errors = ctx.ModelState
          .Where(e => e.Value.Errors.Count > 0)
          .Select(e => new { name = e.Key, message = e.Value.Errors.First().ErrorMessage })
          .ToArray();

          return new BadRequestObjectResult(errors);
        };
      });

      services.AddHealthChecks();

      // add token authentication as a strongly typed class to the DI container
      services.Configure<TokenConfigurationModel>(Configuration.GetSection("TokenConfiguration"));
      var token = Configuration.GetSection("TokenConfiguration").Get<TokenConfigurationModel>();

      // setup authentication
      services.AddAuthentication(c =>
      {
        c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      }) // add JWT Bearer token
      .AddJwtBearer(c =>
      {
        if (HostingEnvironment.IsDevelopment()) c.RequireHttpsMetadata = false; // don't require https in development
        c.SaveToken = true; // stores token server side for stronger validation
        c.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true, // always validate the issuer key
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)), // generate new key from the secret
          ValidIssuer = token.Issuer,
          ValidAudience = token.Audience,
          ValidateIssuer = true,
          ValidateAudience = true
        };
      });
      // add the token authentication svc
      services.AddScoped<ITokenAuthenticationService, TokenAuthenticationService>();
      services.AddScoped<IUserRepository, UserRepository>();
      // setup SPA path
      services.AddSpaStaticFiles(config =>
      {
        config.RootPath = "wwwroot";
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }
      app.UseHttpsRedirection();
      app.UseHealthChecks("/health");
      app.UseAuthentication();
      app.UseMvc();
      app.UseSpaStaticFiles();
      app.UseSpa(config =>
      {
        config.Options.SourcePath = "client";
        if (HostingEnvironment.IsDevelopment())
        {
          config.UseProxyToSpaDevelopmentServer("http://localhost:8080");
        }
      });
    }
  }
}
