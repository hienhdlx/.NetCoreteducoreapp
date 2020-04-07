using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TeduCoreApp.Data.Entities;

namespace TeduCoreApp.Helpers
{
    public class CustomClaimPrincipalFactory: UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        private UserManager<AppUser> _userManager;

        public CustomClaimPrincipalFactory(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager,
            IOptions<IdentityOptions> options): base(userManager, roleManager, options)
        {
            _userManager = userManager;
        }

        public async override Task<ClaimsPrincipal> CreateAsync(AppUser user)
        {
            var pricipal = await base.CreateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            ((ClaimsIdentity)pricipal.Identity).AddClaims(new[]
            {
                new Claim("Email", user.Email),
                new Claim("FullName", user.FullName),
                new Claim("Avatar", user.Avatar?? string.Empty),
                new Claim("Roles", string.Join(";",roles )),
                new Claim("UserId", user.Id.ToString()), 
            });
            return pricipal;
        }
    }
}
