using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Systems;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class FunctionController : BaseController
    {
        #region Initialize

        private IFunctionService _functionService;

        public FunctionController(IFunctionService functionService)
        {
            _functionService = functionService;
        }


        #endregion
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFillter(string fillter)
        {
            var model = await _functionService.GetAll(fillter);
            return new ObjectResult(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var model = await _functionService.GetAll(string.Empty);
            var rootFunction = model.Where(x => x.ParentId == null);
            var items = new List<FunctionViewModel>();
            foreach (var function in rootFunction)
            {
                //add parent category to the items list
                items.Add(function);
                //now get all child (separate category in case you need recursion)
                GetByParentId(model.ToList(), function, items);
            }

            return new OkObjectResult(items);
        }

        #region Private Functions

        private void GetByParentId(IEnumerable<FunctionViewModel> allFunction, FunctionViewModel parent,
            IList<FunctionViewModel> items)
        {
            var functionEntities = allFunction as FunctionViewModel[] ?? allFunction.ToArray();
            var subFunctions = functionEntities.Where(x => x.ParentId == parent.Id);
            foreach (var cat in subFunctions)
            {
                //add this category
                items.Add(cat);
                //recursive call in case your have a hierarchy more than 1 lever deep
                GetByParentId(functionEntities,cat, items);
            }
        }

        #endregion

        [HttpPost]
        public IActionResult Deleted(string id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            else
            {
                _functionService.Delete(id);
                _functionService.Save();
            }
            return new OkObjectResult(id);
        }

        [HttpPost]
        public IActionResult ReOrder(string sourceId, string targetId)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            else
            {
                if (sourceId == targetId)
                {
                    return new BadRequestResult();
                }
                else
                {
                    _functionService.ReOrder(sourceId, targetId);
                    _functionService.Save();
                    return new OkObjectResult(sourceId);
                }
            }
        }

        [HttpPost]
        public IActionResult UpdateParentId(string sourceId, string targetId, Dictionary<string, int> items)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            else
            {
                if (sourceId == targetId)
                {
                    return new BadRequestObjectResult(sourceId);
                }
                else
                {
                    _functionService.UpdateParentId(sourceId, targetId, items);
                    _functionService.Save();
                    return new OkResult();
                }
            }
        }

        [HttpPost]
        public IActionResult SaveEntity(FunctionViewModel functionVm)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> errors = ModelState.Values.SelectMany(x => x.Errors);
                return new BadRequestObjectResult(errors);
            }
            else
            {
                if (string.IsNullOrEmpty(functionVm.Id))
                {
                    _functionService.Add(functionVm);
                }
                else
                {
                    _functionService.Update(functionVm);
                }
                _functionService.Save();
            }
            return new OkObjectResult(functionVm);
        }

        [HttpGet]
        public IActionResult GetById(string id)
        {
            var model = _functionService.GetById(id);
            return new OkObjectResult(model);
        }



    }
}