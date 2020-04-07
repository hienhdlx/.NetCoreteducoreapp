using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Interfaces
{
    public interface IContactService
    {
        void Add(ContactViewModel contactVm);

        void Update(ContactViewModel contactVm);

        void Delete(string id);

        List<ContactViewModel> GetAll();

        PageResult<ContactViewModel> GetAllPaging(string keyword, int page, int pageSize);

        ContactViewModel GetById(string id);

        void SaveChanges();
    }
}
