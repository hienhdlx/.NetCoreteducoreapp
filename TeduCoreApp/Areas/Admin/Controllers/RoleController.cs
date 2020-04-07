using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Systems;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class RoleController : BaseController
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAll()
        {
            var model = await _roleService.GetAllAsync();
            return new OkObjectResult(model);
        }

        [HttpPost]
        public IActionResult SavePermission(List<PermissionViewModel> listPermission, Guid roleId)
        {
            _roleService.SavePermission(listPermission, roleId);
            return new OkResult();
        }

        [HttpPost]
        public IActionResult ListAllFunction(Guid roleId)
        {
            var function = _roleService.GetListFunctionWithRole(roleId);
            return new OkObjectResult(function);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            await _roleService.DeleteAsync(id);
            return new OkObjectResult(id);
        }

        [HttpPost]
        public async Task<IActionResult> SaveEntity(AppRoleViewModel roleMd)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> errors = ModelState.Values.SelectMany(x => x.Errors);
                return new BadRequestObjectResult(errors);
            }

            if (roleMd.Id == null)
            {
                await _roleService.AddAsync(roleMd);
            }
            else
            {
                await _roleService.UpdateAsync(roleMd);
            }
            return new OkObjectResult(roleMd);
        }

        [HttpGet]
        public IActionResult GetAllPaging(string keyword, int page, int pageSize)
        {
            var model = _roleService.GetAllPagingAsync(keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var model = await _roleService.GetById(id);
            return new OkObjectResult(model);
        }
    }
}