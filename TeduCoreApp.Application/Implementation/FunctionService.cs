using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;

namespace TeduCoreApp.Application.Implementation
{
    public class FunctionService: IFunctionService
    {
        private IFunctionRepository _functionRepository;
        private IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public FunctionService(IFunctionRepository functionRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _functionRepository = functionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        public void Add(FunctionViewModel functions)
        {
            var function = _mapper.Map<Function>(functions);
            _functionRepository.Add(function);
        }

        public Task<List<FunctionViewModel>> GetAll(string fillter)
        {
            var query = _functionRepository.FindAll(x => x.Status == Status.Active);
            if (!string.IsNullOrEmpty(fillter))
            {
                query = _functionRepository.FindAll(x => x.Name.Contains(fillter));
            }

            return query.OrderBy(x => x.ParentId).ProjectTo<FunctionViewModel>().ToListAsync();
        }

        public IEnumerable<FunctionViewModel> GetAllWithParentId(string parentId)
        {
            return _functionRepository.FindAll(x => x.ParentId == parentId).ProjectTo<FunctionViewModel>();
        }

        public FunctionViewModel GetById(string id)
        {
            var function = _functionRepository.FindSingle(x => x.Id == id);
            return Mapper.Map<Function, FunctionViewModel>(function);
        }

        public void Update(FunctionViewModel functionVM)
        {
            var functionDb = _functionRepository.FindById(functionVM.Id);
            var function = _mapper.Map<Function>(functionDb);
        }

        public void Delete(string id)
        {
            _functionRepository.Remove(id);
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public bool CheckExistedId(string id)
        {
            return _functionRepository.FindById(id) != null ;
        }

        public void UpdateParentId(string sourceId, string targetId, Dictionary<string, int> items)
        {
            //update parent id for source
            var category = _functionRepository.FindById(sourceId);
            category.ParentId = targetId;
            _functionRepository.Update(category);

            //get all sibling
            var sibling = _functionRepository.FindAll(x => items.ContainsKey(x.Id));
            foreach (var child in sibling)
            {
                child.SortOrder = items[child.Id];
                _functionRepository.Update(child);
            }
        }

        public void ReOrder(string sourceId, string targetId)
        {
            var source = _functionRepository.FindById(sourceId);
            var target = _functionRepository.FindById(targetId);
            int temOrder = source.SortOrder;

            source.SortOrder = target.SortOrder;
            target.SortOrder = temOrder;

            _functionRepository.Update(target);
            _functionRepository.Update(source);
        }
    }
}
