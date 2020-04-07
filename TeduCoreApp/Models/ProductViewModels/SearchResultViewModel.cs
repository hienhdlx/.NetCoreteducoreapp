using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Models.ProductViewModels
{
    public class SearchResultViewModel : CatalogViewModel
    {
        public string Keyword { get; set; }
    }
}
