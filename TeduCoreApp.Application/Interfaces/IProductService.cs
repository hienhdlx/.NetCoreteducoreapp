using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Utilities.Dtos;

namespace TeduCoreApp.Application.Interfaces
{
    public interface IProductService : IDisposable
    {
        List<ProductViewModel> GetAll();
        PageResult<ProductViewModel> GetAllPaging(int? categoryId, string keyword, int page, int pageSize);
        ProductViewModel Add(ProductViewModel productViewModel);
        void Update(ProductViewModel productViewModel);
        void Delete(int id);
        ProductViewModel GetById(int id);
        void ImportExcel(string filePath, int categoryId);
        void Save();
        void AddQuantity(int productId,List<ProductQuantityViewModel> quantity);
        List<ProductQuantityViewModel> GetQuantities(int productId);
        void AddImages(int productId, string[] images);
        List<ProductImageViewModel> GetImages(int productId);
        void AddWholePrice(int productId, List<WholePriceViewModel> wholePriceVm);
        List<WholePriceViewModel> GetWholePrices(int productId);
        List<ProductViewModel> GetHotProduct(int top);
        List<ProductViewModel> GetLastest(int top);
        List<ProductViewModel> GetRelatedProducts(int id, int top);
        List<ProductViewModel> GetUpsellProducts(int top);
        List<TagViewModel> GetProductTags(int productId);
        bool CheckAvailablility(int productId, int color, int size);
    }
}
