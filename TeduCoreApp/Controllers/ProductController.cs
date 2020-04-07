using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Models.ProductViewModels;

namespace TeduCoreApp.Controllers
{
    public class ProductController : Controller
    {
        private IProductService _productService;
        private IProductCategoryService _productCategoryService;
        private IConfiguration _configuration;
        private IBillService _billService;

        public ProductController(IProductService productService, IProductCategoryService productCategoryService, IConfiguration configuration, IBillService billService)
        {
            _productService = productService;
            _productCategoryService = productCategoryService;
            _configuration = configuration;
            _billService = billService;
        }

        [Route("products.html")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("{alias}-c.{id}.html")]
        public IActionResult Catalog(int id, string keyword, int? pageSize, string sortBy, int page = 1)
        {
            var catalog = new CatalogViewModel();
            ViewData["BodyClass"] = "shop_grid_full_width_page";
            if (pageSize == null)
            {
                pageSize = _configuration.GetValue<int>("pageSize");
            }
            catalog.PageSize = pageSize;
            catalog.SortType = sortBy;
            catalog.Data = _productService.GetAllPaging(id, string.Empty, page, pageSize.Value);
            catalog.Category = _productCategoryService.GetById(id);
            return View(catalog);
        }

        [Route("search.html")]
        public IActionResult Search(string keyword, int? pageSize, string sortBy, int page = 1)
        {
            var catalog = new SearchResultViewModel();
            ViewData["BodyClass"] = "shop_grid_full_width_page";
            if (pageSize == null)
            {
                pageSize = _configuration.GetValue<int>("pageSize");
            }
            catalog.PageSize = pageSize;
            catalog.SortType = sortBy;
            catalog.Data = _productService.GetAllPaging(null, keyword, page, pageSize.Value);
            catalog.Keyword = keyword;
            return View(catalog);
        }

        [Route("{alias}-p.{id}.html", Name = "ProductDetail")]
        public IActionResult Details(int id)
        {
            ViewData["BodyClass"] = "product-page";
            var model = new DetailViewModel();
            model.Product = _productService.GetById(id);
            model.Category = _productCategoryService.GetById(model.Product.CategoryId);
            model.RelatedProducts = _productService.GetRelatedProducts(id, 9);
            model.UpsellProducts = _productService.GetUpsellProducts(6);
            model.ProductImages = _productService.GetImages(id);
            model.Tags = _productService.GetProductTags(id);
            model.Color = _billService.GetColors().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            model.Size = _billService.GetSizes().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();
            return View(model);
        }
    }
}