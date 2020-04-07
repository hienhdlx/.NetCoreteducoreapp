using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Blog;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Implementation
{
    public class PageService : IPageService
    {
        private IPageRepository _pageRepository;
        private IUnitOfWork _unitOfWork;

        public PageService(IPageRepository pageRepository, IUnitOfWork unitOfWork)
        {
            _pageRepository = pageRepository;
            _unitOfWork = unitOfWork;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void Add(PageViewModel pageVm)
        {
            var page = Mapper.Map<PageViewModel, Page>(pageVm);
            _pageRepository.Add(page);
        }

        public void Update(PageViewModel pageVm)
        {
            var page = Mapper.Map<PageViewModel, Page>(pageVm);
            _pageRepository.Update(page);
        }

        public void Delete(int id)
        {
            _pageRepository.Remove(id);
        }

        public List<PageViewModel> GetAll()
        {
            return _pageRepository.FindAll().ProjectTo<PageViewModel>().ToList();
        }

        public PageResult<PageViewModel> GetAllPaging(string keyword, int page, int pageSize)
        {
            var query = _pageRepository.FindAll();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword) || x.Content.Contains(keyword));
            }

            int totalRow = query.Count();
            var data = query.OrderByDescending(x => x.Alias).Skip((page - 1) * pageSize).Take(pageSize);

            var paginationSet = new PageResult<PageViewModel>()
            {
                Results = data.ProjectTo<PageViewModel>().ToList(),
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };

            return paginationSet;
        }

        public PageViewModel GetByAlias(string alias)
        {
            return Mapper.Map<Page, PageViewModel>(_pageRepository.FindSingle(x => x.Alias == alias));
        }

        public PageViewModel GetById(int id)
        {
            return Mapper.Map<Page, PageViewModel>(_pageRepository.FindById(id));
        }

        public void SaveChanges()
        {
            _unitOfWork.Commit();
        }
    }
}
