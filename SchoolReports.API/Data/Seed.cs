using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using SchoolReports.API.Models;

namespace SchoolReports.API.Data
{
  public class Seed
  {
    public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
      if (userManager.Users.Any()) return;
      var userData = System.IO.File.ReadAllText("Data/SeedData.json");
      var users = JsonConvert.DeserializeObject<List<User>>(userData);

      var roles = new List<Role>
      {
        new Role{Name = "Member"},
        new Role{Name = "Admin"},
        new Role{Name = "Teacher"}
      };

      foreach(var role in roles)
      {
        roleManager.CreateAsync(role).Wait();
      }

      foreach (var user in users)
      {
        userManager.CreateAsync(user, "Password@01").Wait();
        userManager.AddToRoleAsync(user, "Member").Wait();
      }

      var adminUser = new User{UserName = "Admin"};
      var reuslt = userManager.CreateAsync(adminUser, "Password@01").Result;

      if(reuslt.Succeeded)
      {
        var admin = userManager.FindByNameAsync("Admin").Result;
        userManager.AddToRolesAsync(admin, new [] {"Admin"});
      }
    }

    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }
  }
}