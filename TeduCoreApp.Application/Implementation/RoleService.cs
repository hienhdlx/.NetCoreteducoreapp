using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Implementation
{
    public class RoleService : IRoleService
    {
        private RoleManager<AppRole> _roleManager;
        private IFunctionRepository _functionRepository;
        private IPermissionRepository _permissionRepository;
        private IUnitOfWork _unitOfWork;

        public RoleService(RoleManager<AppRole> roleManager, IFunctionRepository functionRepository, IPermissionRepository permissionRepository, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _functionRepository = functionRepository;
            _unitOfWork = unitOfWork;
            _permissionRepository = permissionRepository;
        }
        public async Task<bool> AddAsync(AppRoleViewModel userVm)
        {
            var role = new AppRole()
            {
                Name = userVm.Name,
                Description = userVm.Description
            };
            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded;
        }

        public async Task DeleteAsync(Guid id)
        {
            var model = await _roleManager.FindByIdAsync(id.ToString());
            await _roleManager.DeleteAsync(model);
        }

        public async Task<List<AppRoleViewModel>> GetAllAsync()
        {
            return await _roleManager.Roles.ProjectTo<AppRoleViewModel>().ToListAsync();
        }

        public PageResult<AppRoleViewModel> GetAllPagingAsync(string keyword, int page, int pageSize)
        {
            var query = _roleManager.Roles;
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(x => x.Name.Contains(keyword)
                                         || x.Description.Contains(keyword));

            int totalRow = query.Count();
            query = query.Skip((page - 1) * pageSize)
                .Take(pageSize);

            var data = query.ProjectTo<AppRoleViewModel>().ToList();
            var paginationSet = new PageResult<AppRoleViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }

        public async Task<AppRoleViewModel> GetById(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            return Mapper.Map<AppRole, AppRoleViewModel>(role);
        }

        public async Task UpdateAsync(AppRoleViewModel userVm)
        {
            var role = await _roleManager.FindByIdAsync(userVm.Id.ToString());
            role.Description = userVm.Description;
            role.Name = userVm.Name;
            await _roleManager.UpdateAsync(role);
        }

        public List<PermissionViewModel> GetListFunctionWithRole(Guid roleId)
        {
            var functions = _functionRepository.FindAll();
            var permissions = _permissionRepository.FindAll();

            var query = from f in functions
                join p in permissions on f.Id equals p.FunctionId into fp
                from p in fp.DefaultIfEmpty()
                where p != null && p.RoleId == roleId
                select new PermissionViewModel()
                {
                    RoleId = roleId,
                    FunctionId = f.Id,
                    CanCreate = p != null ? p.CanCreate : false,
                    CanDelete = p != null ? p.CanDelete : false,
                    CanRead = p != null ? p.CanRead : false,
                    CanUpdate = p != null ? p.CanUpdate : false
                };
            return query.ToList();
        }

        public void SavePermission(List<PermissionViewModel> permissionsVm, Guid roleId)
        {
            var permission = Mapper.Map<List<PermissionViewModel>, List<Permission>>(permissionsVm);
            var oldPermistion = _permissionRepository.FindAll().Where(x => x.RoleId == roleId).ToList();
            if (oldPermistion.Count > 0)
            {
                _permissionRepository.RemoveMultiple(oldPermistion);
            }

            foreach (var pmt in permission)
            {
                _permissionRepository.Add(pmt);
            }
            _unitOfWork.Commit();
        }

        public Task<bool> CheckPermission(string functionId, string action, string[] roles)
        {
            var functions = _functionRepository.FindAll();
            var permissions = _permissionRepository.FindAll();
            var query = from f in functions
                join p in permissions on f.Id equals p.FunctionId
                join r in _roleManager.Roles on p.RoleId equals r.Id
                where roles.Contains(r.Name) && f.Id == functionId
                                             && ((p.CanCreate && action == "Create")
                                                 || (p.CanUpdate && action == "Update")
                                                 || (p.CanDelete && action == "Delete")
                                                 || (p.CanRead && action == "Read"))
                select p;
            return query.AnyAsync();
        }

    }
}
