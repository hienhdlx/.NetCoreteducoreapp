using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Utilities.Constants;

namespace TeduCoreApp.Authorization
{
    public class BaseResourceAuthorizationHandler: AuthorizationHandler<OperationAuthorizationRequirement, string>
    {
        public IRoleService _roleService;

        public BaseResourceAuthorizationHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement,
            string resource)
        {
            var role = ((ClaimsIdentity)context.User.Identity).Claims.FirstOrDefault(x =>
                x.Type == CommonConstants.UserClaims.Roles);

            if (role != null)
            {
                var listrole = role.Value.Split(";");
                var hasPermission = await _roleService.CheckPermission(resource, requirement.Name, listrole);
                if (hasPermission || listrole.Contains(CommonConstants.AppRole.AdminRole))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    context.Fail();
                }
            }
            else
            {
                context.Fail();
            }
        }
    }
}
