using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Hubs;
using backend.Models.Options;
using backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace backend
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
      services.AddCors(o =>
      {
        o.AddPolicy("AllowAll", builder =>
        {
          builder
          .WithOrigins("http://192.168.0.2:19006", "http://localhost:19006")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
        });
      });

      services.Configure<JWTOptions>(Configuration.GetSection(nameof(JWTOptions)));
      services.AddSingleton<IJWTOptions>(sp => sp.GetRequiredService<IOptions<JWTOptions>>().Value);

      var secretKey = Configuration.GetValue<string>($"{nameof(JWTOptions)}:SecretKey");

      services
      .AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = false,
          ValidateAudience = false,
          ValidateIssuerSigningKey = true,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
      });

      services.Configure<MongoDbOptions>(Configuration.GetSection(nameof(MongoDbOptions)));
      services.AddSingleton<IMongoDbOptions>(sp => sp.GetRequiredService<IOptions<MongoDbOptions>>().Value);

      services.AddSingleton<MongoClient>(sp =>
      {
        var mongoDbOptions = sp.GetRequiredService<IMongoDbOptions>();
        var mongoClient = new MongoClient(mongoDbOptions.ConnectionString);
        return mongoClient;
      });

      services.AddSingleton<SessionsServices>();
      services.AddSingleton<AuthenticationTokensServices>();

      services.AddSignalR();
      services.AddControllers();
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "backend", Version = "v1" });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

      app.UseCors("AllowAll");
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "backend v1"));
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<ChatHub>("/chatHub");
      });
    }
  }
}
