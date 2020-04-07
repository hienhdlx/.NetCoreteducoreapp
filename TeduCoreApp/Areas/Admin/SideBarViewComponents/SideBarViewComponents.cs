using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Extensions;
using TeduCoreApp.Utilities.Constants;

namespace TeduCoreApp.Areas.Admin.SideBarViewComponents
{
    [ViewComponent(Name = "SideBar")]
    public class SideBarViewComponents: ViewComponent
    {
        private IFunctionService _functionService;
        public SideBarViewComponents(IFunctionService functionService)
        {
            _functionService = functionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            //get role
            var roles = ((ClaimsPrincipal) User).GetSpecificClaim("Roles");
            List<FunctionViewModel> functionView;
            if (roles.Split(";").Contains(CommonConstants.AppRole.AdminRole))
            {
                functionView = await _functionService.GetAll(string.Empty);
            }
            else
            {
                //To Do: get by permission
                functionView = new List<FunctionViewModel>();
            }

            return View(functionView);
        }
    }
}
