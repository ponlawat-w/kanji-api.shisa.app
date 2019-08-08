using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShisaKanjiDatabaseContext;
using System;

namespace ShisaKanjis {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services) {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
        .AddJsonOptions(opts => {
          opts.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
          opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });

      services.AddCors();

      services.AddDbContext<KanjiDbContext>(opts => {
        opts.UseNpgsql(KanjiDbContext.GetDefaultConnectionString());
      });

      services.Configure<ApiBehaviorOptions>(opts => {
        opts.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
          ApiResponse.CreateUnsuccessful(ErrorMessages.InvalidInput));
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      }

      string allowOriginStr = Environment.GetEnvironmentVariable("ALLOW_ORIGINS");

      app.UseCors(opts => {
        if (allowOriginStr != null) {
          string[] allowOrigins = allowOriginStr.Split(';');
          if (allowOrigins.Length > 0) {
            opts.WithOrigins(allowOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
          } else {
            opts.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
          }
        }
      });

      app.UseStatusCodePages(async context => {
        int statusCode = context.HttpContext.Response.StatusCode;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(
          ApiResponse.CreateUnsuccessful(ErrorMessages.RequestError,
            ReasonPhrases.GetReasonPhrase(statusCode))));
      });

      app.UseExceptionHandler(errorAppBuilder => {
        errorAppBuilder.Run(async context => {
          context.Response.StatusCode = StatusCodes.Status500InternalServerError;
          context.Response.ContentType = "application/json";
          await context.Response.WriteAsync(JsonConvert.SerializeObject(ApiResponse.CreateUnsuccessful(ErrorMessages.Error)));
        });
      });

      app.UseMvc();
    }
  }
}
