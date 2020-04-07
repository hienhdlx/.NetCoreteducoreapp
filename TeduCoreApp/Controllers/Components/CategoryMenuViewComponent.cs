using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Caching.Memory;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Infrastructure.Enums;

namespace TeduCoreApp.Controllers.Components
{
    [ViewComponent(Name = "CategoryMenu")]
    public class CategoryMenuViewComponent : ViewComponent
    {
        private IProductCategoryService _productCategoryService;
        private IMemoryCache _memoryCache;

        public CategoryMenuViewComponent(IProductCategoryService productCategoryService, IMemoryCache memoryCache)
        {
            _productCategoryService = productCategoryService;
            _memoryCache = memoryCache;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cacheEntry = _memoryCache.GetOrCreate(CacheKeys.ProductCategories, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(3);
                    return _productCategoryService.GetAll();
                });
            //var resutl = _productCategoryService.GetAll();
            return View(cacheEntry);
        }
    }
}
