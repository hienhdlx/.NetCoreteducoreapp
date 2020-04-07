﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<bool> AddAsync(AppUserViewModel userVm);
        Task DeleteAsync(string id);
        Task<List<AppUserViewModel>> GetAllAsync();
        PageResult<AppUserViewModel> GetAllPagingAsync(string keyword, int page, int pageSize);
        Task<AppUserViewModel> GetById(string id);
        Task UpdateAsync(AppUserViewModel userVm);
    }
}
