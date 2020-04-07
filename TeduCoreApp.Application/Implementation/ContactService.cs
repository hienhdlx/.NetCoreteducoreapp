using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Implementation
{
    public class ContactService: IContactService
    {
        private IContactRepository _contactRepository;
        private IUnitOfWork _unitOfWork;

        public ContactService(IContactRepository contactRepository, IUnitOfWork unitOfWork)
        {
            _contactRepository = contactRepository;
            _unitOfWork = unitOfWork;
        }
        public void Add(ContactViewModel contactVm)
        {
            var page = Mapper.Map<ContactViewModel, Contact>(contactVm);
            _contactRepository.Add(page);
        }

        public void Update(ContactViewModel contactVm)
        {
            var page = Mapper.Map<ContactViewModel, Contact>(contactVm);
            _contactRepository.Update(page);
        }

        public void Delete(string id)
        {
            _contactRepository.Remove(id);
        }

        public List<ContactViewModel> GetAll()
        {
            return _contactRepository.FindAll().ProjectTo<ContactViewModel>().ToList();
        }

        public PageResult<ContactViewModel> GetAllPaging(string keyword, int page, int pageSize)
        {
            var query = _contactRepository.FindAll();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }

            int totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize);

            var paginnartionSet = new PageResult<ContactViewModel>()
            {
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize,
                Results = data.ProjectTo<ContactViewModel>().ToList()
            };
            return paginnartionSet;
        }

        public ContactViewModel GetById(string id)
        {
            return Mapper.Map<Contact, ContactViewModel>(_contactRepository.FindById(id));
        }


        public void SaveChanges()
        {
            _unitOfWork.Commit();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
