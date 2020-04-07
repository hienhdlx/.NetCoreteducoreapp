using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Application.ViewModels.Systems;

namespace TeduCoreApp.Models.ProductViewModels
{
    public class DetailViewModel
    {
        public ProductViewModel Product { get; set; }
        public List<ProductViewModel> RelatedProducts { get; set; }
        public ProductCategoryViewModel Category { get; set; }
        public List<ProductImageViewModel> ProductImages { get; set; }
        public List<ProductViewModel> LastestProducts { get; set; }
        public List<ProductViewModel> UpsellProducts { get; set; }
        public List<TagViewModel> Tags { get; set; }
        public List<SelectListItem> Color { get; set; }
        public List<SelectListItem> Size { get; set; }
    }
}
