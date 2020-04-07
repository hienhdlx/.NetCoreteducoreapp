using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Authorization;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private IAuthorizationService _authorizationService;

        public UserController(IUserService userService, IAuthorizationService authorizationService)
        {
            _userService = userService;
            _authorizationService = authorizationService;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _authorizationService.AuthorizeAsync(User, "USER", Operations.Create);
            if (result.Succeeded == false)
            {
                return new RedirectResult("/Admin/Login/Index");
            }
            else
            {
                return View();
            }
        }

        public IActionResult GetAll()
        {
            var model = _userService.GetAllAsync();
            return new OkObjectResult(model);
        }
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _userService.GetById(id);
            return new OkObjectResult(result);
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _userService.GetAllPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEntity(AppUserViewModel userVm)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
                return new BadRequestObjectResult(allErrors);
            }
            else
            {
                if (userVm.Id == null)
                {
                    await _userService.AddAsync(userVm);
                }
                else
                {
                    await _userService.UpdateAsync(userVm);
                }
            }

            return new OkObjectResult(userVm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(x => x.Errors);
                return new BadRequestObjectResult(allErrors);
            }
            else
            {
                await _userService.DeleteAsync(id);
                
            }
            return new OkObjectResult(id);
        }
    }
}