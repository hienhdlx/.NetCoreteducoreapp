using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Utilities.Helpers;

namespace TeduCoreApp.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        private IProductService _productService;
        private IProductCategoryService _productCategoryService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IWholePriceRepository _wholePriceRepository;

        public ProductController(IProductService productService, IProductCategoryService productCategoryService, IHostingEnvironment hostingEnvironment, IWholePriceRepository wholePriceRepository)
        {
            _productService = productService;
            _productCategoryService = productCategoryService;
            _hostingEnvironment = hostingEnvironment;
            _wholePriceRepository = wholePriceRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region Ajax API
        [HttpGet]
        public IActionResult GetAll()
        {
            var model = _productService.GetAll();
            return new OkObjectResult(model);
        }

        public IActionResult GetAllPaging(int? categoryId, string keyword, int page, int pageSize)
        {
            var model = _productService.GetAllPaging(categoryId, keyword, page, pageSize);
            return new OkObjectResult(model);
        }

        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var model = _productCategoryService.GetAll();
            return new OkObjectResult(model);
        }

        [HttpPost]
        public IActionResult SaveQuantities(int productId, List<ProductQuantityViewModel> quantities)
        {
            _productService.AddQuantity(productId, quantities);
            _productService.Save();
            return new OkObjectResult(quantities);
        }

        [HttpGet]
        public IActionResult GetQuantities(int productId)
        {
            var model = _productService.GetQuantities(productId);
            return new OkObjectResult(model);
        }

        [HttpPost]
        public IActionResult SaveEntity(ProductViewModel productVmol)
        {
            if (!ModelState.IsValid)
            {
                IEnumerable<ModelError> errors = ModelState.Values.SelectMany(x => x.Errors);
                return new BadRequestObjectResult(errors);
            }
            else
            {
                productVmol.SeoAlias = TextHelper.ToUnsignString(productVmol.Name);
                if (productVmol.Id == 0)
                {
                    _productService.Add(productVmol);
                }
                else
                {
                    _productService.Update(productVmol);
                }
                _productService.Save();
                return new OkObjectResult(productVmol);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            else
            {
                _productService.Delete(id);
                _productService.Save();
            return new OkObjectResult(id);
            }
        }

        [HttpGet]
        public IActionResult GetById(int id)
        {
            var product = _productService.GetById(id);
            return new OkObjectResult(product);
        }

        [HttpPost]
        public IActionResult ImportExcel(List<IFormFile> files, int categoryId)
        {
            if (files != null && files.Count > 0)
            {
                var file = files[0];
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                string folder = _hostingEnvironment.WebRootPath + @"\uploaded\excels";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string filePath = Path.Combine(folder, fileName);
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                }
                _productService.ImportExcel(filePath, categoryId);
                _productService.Save();
                return new OkObjectResult(filePath);
            }
            return new NoContentResult();
        }

        [HttpPost]
        public IActionResult ExportExcel()
        {
            string sWebrootFolder = _hostingEnvironment.WebRootPath;
            string directory = Path.Combine(sWebrootFolder, "export-file");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string sFileName = $"Product_{DateTime.Now:yyyyMMddhhmmss}.xlsx";
            string fileUrl = $"{Request.Scheme}://{Request.Host}/export-files/{sFileName}";
            FileInfo fileInfo = new FileInfo(Path.Combine(directory, sFileName));
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
                fileInfo = new FileInfo(Path.Combine(sWebrootFolder, sFileName));
            }

            var product = _productService.GetAll();
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                // add new worksheet into the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells["A1"].LoadFromCollection(product, true, TableStyles.Light1);
                worksheet.Cells.AutoFitColumns();
                package.Save(); // save workbook
            }
            return new RedirectResult("/Admin/Product/Index");
        }

        [HttpPost]
        public IActionResult SaveImages(int productId, string[] images)
        {
            _productService.AddImages(productId, images);
            _productService.Save();
            return new OkObjectResult(images);
        }

        [HttpGet]
        public IActionResult GetImages(int productId)
        {
            var model = _productService.GetImages(productId);
            return new OkObjectResult(model);
        }

        [HttpPost]
        public IActionResult SaveWholePrice(int productId, List<WholePriceViewModel> wholePriceVm)
        {
            _productService.AddWholePrice(productId, wholePriceVm);
            _productService.Save();
            return new OkObjectResult(wholePriceVm);
        }

        [HttpGet]
        public IActionResult GetWholePrices(int productId)
        {
            var model = _productService.GetWholePrices(productId);
            return new OkObjectResult(model);
        }

        #endregion

    }
}