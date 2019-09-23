using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SchoolReports.API.Models
{
    public class User : IdentityUser<int>
    {
        public string KnownAs { get; set; }
        public DateTime Create { get; set; }
        public DateTime LastActive { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}