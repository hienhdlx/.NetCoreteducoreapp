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
    public class FeedbackService: IFeedbackService
    {
        private IFeedbackRepository _feedbackRepository;
        private IUnitOfWork _unitOfWork;

        public FeedbackService(IFeedbackRepository feedbackRepository, IUnitOfWork unitOfWork)
        {
            _feedbackRepository = feedbackRepository;
            _unitOfWork = unitOfWork;
        }

        public void Add(FeedbackViewModel feedbackVm)
        {
            var page = Mapper.Map<FeedbackViewModel,Feedback>(feedbackVm);
            _feedbackRepository.Add(page);

        }

        public void Update(FeedbackViewModel feedbackVm)
        {
            var page = Mapper.Map<FeedbackViewModel, Feedback>(feedbackVm);
            _feedbackRepository.Update(page);
        }

        public void Delete(int id)
        {
            _feedbackRepository.Remove(id);
        }

        public List<FeedbackViewModel> GetAll()
        {
            return _feedbackRepository.FindAll().ProjectTo<FeedbackViewModel>().ToList();
        }

        public PageResult<FeedbackViewModel> GetAllPaging(string keyword, int page, int pageSize)
        {
            var query = _feedbackRepository.FindAll();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }

            int totalPage = query.Count();
            var data = query.OrderByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize);

            var paginationSet = new PageResult<FeedbackViewModel>()
            {
                Results = data.ProjectTo<FeedbackViewModel>().ToList(),
                CurrentPage = page,
                RowCount = totalPage,
                PageSize = pageSize
            };

            return paginationSet;
        }

        public FeedbackViewModel GetById(int id)
        {
            return Mapper.Map<Feedback, FeedbackViewModel>(_feedbackRepository.FindById(id));
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
