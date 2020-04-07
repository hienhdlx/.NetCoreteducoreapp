using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TeduCoreApp.Application.ViewModels.Systems;

namespace TeduCoreApp.Application.Interfaces
{
    public interface IFunctionService : IDisposable
    {
        void Add(FunctionViewModel function);

        Task<List<FunctionViewModel>> GetAll(string fillter);

        IEnumerable<FunctionViewModel> GetAllWithParentId(string parentId);

        FunctionViewModel GetById(string id);

        void Update(FunctionViewModel functionVM);
        void Delete(string id);
        void Save();

        bool CheckExistedId(string id);

        void UpdateParentId(string sourceId, string targetId, Dictionary<string, int> items);

        void ReOrder(string sourceId, string targetId);
    }
}
