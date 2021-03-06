﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolReports.API.Data;
using SchoolReports.API.Models;

namespace SchoolReports.API
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
      ConfigurePersistence(services);
      ConfigurePersistence(services);
      ConfigureMvc(services);

      services.AddCors();
    }

    private void ConfigurePersistence(IServiceCollection services)
    {
      services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));
    }

    private void ConfigureIdentity(IServiceCollection services)
    {
      IdentityBuilder builder = services.AddIdentityCore<User>(opt =>
      {
        opt.Password.RequireDigit = true;
        opt.Password.RequiredLength = 8;
        opt.Password.RequireNonAlphanumeric = true;
        opt.Password.RequireUppercase = true;
      });

      builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
      builder.AddEntityFrameworkStores<DataContext>(); //User EF as the Store
      builder.AddRoleValidator<RoleValidator<Role>>();
      builder.AddRoleManager<RoleManager<Role>>();
      builder.AddSignInManager<SignInManager<User>>(); //Manul set up as we are using AddIdentityCore not AddIdentity

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
              Configuration.GetSection("AppSettings:Token").Value)),
          ValidateIssuer = false,
          ValidateAudience = false
        };
      });

      services.AddAuthorization(options =>
      {
        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
      });
    }

    private void ConfigureMvc(IServiceCollection services)
    {
      services.AddMvc(options =>
      {
        var policy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();

        options.Filters.Add(new AuthorizeFilter(policy));
      }) // instead of using [Authorize] attributes
      .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
      .AddJsonOptions(opt =>
      {
        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
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

      app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
      app.UseAuthentication();
      app.UseHttpsRedirection();
      app.UseMvc();
    }
  }
}
