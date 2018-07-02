using Ass2WithOAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ass2WithOAuth.Data
{
    public class RoleSeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { Constants.OwnerRole, Constants.CBDHolderRole, Constants.NorthHolderRole,
                Constants.SouthHolderRole, Constants.EastHolderRole, Constants.WestHolderRole,Constants.CustomerRole};

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = role
                    });
                }
            }

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            await EnsureUserHasRole(userManager, "Owner@example.com", Constants.OwnerRole);
            await EnsureUserHasRole(userManager, "CBDHolder@example.com", Constants.CBDHolderRole);
            await EnsureUserHasRole(userManager, "NorthHolder@example.com", Constants.NorthHolderRole);
            await EnsureUserHasRole(userManager, "SouthHolder@example.com", Constants.SouthHolderRole);
            await EnsureUserHasRole(userManager, "EastHolder@example.com", Constants.EastHolderRole);
            await EnsureUserHasRole(userManager, "WestHolder@example.com", Constants.WestHolderRole);
            await EnsureUserHasRole(userManager, "s3584414@student.rmit.edu.au", Constants.OwnerRole);
            await EnsureUserHasRole(userManager, "haha401136@gmail.com", Constants.EastHolderRole);
            await EnsureUserHasRole(userManager, "Customer@example.com", Constants.CustomerRole);
        }


        private static async Task EnsureUserHasRole(
            UserManager<ApplicationUser> userManager, string userName, string role)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user != null && !await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
    
}
