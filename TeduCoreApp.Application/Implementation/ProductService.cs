using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using OfficeOpenXml;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Constants;
using TeduCoreApp.Utilities.Dtos;
using TeduCoreApp.Utilities.Helpers;

namespace TeduCoreApp.Application.Implementation
{
    public class ProductService: IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductTagRepository _productTagRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductQuantityRepository _productQuantity;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IWholePriceRepository _wholePriceRepository;

        public ProductService(IProductRepository productRepository, 
            IProductTagRepository productTagRepository, 
            ITagRepository tagRepository, 
            IUnitOfWork unitOfWork, 
            IProductQuantityRepository productQuantity,
            IProductImageRepository productImageRepository,
            IWholePriceRepository wholePriceRepository)
        {
            _productRepository = productRepository;
            _productTagRepository = productTagRepository;
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
            _productQuantity = productQuantity;
            _productImageRepository = productImageRepository;
            _wholePriceRepository = wholePriceRepository;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public List<ProductViewModel> GetAll()
        {
            return _productRepository.FindAll(x =>x.ProductCategory).ProjectTo<ProductViewModel>().ToList();
        }

        public PageResult<ProductViewModel> GetAllPaging(int? categoryId, string keyword, int page, int pageSize)
        {
            var query = _productRepository.FindAll(x => x.Status == Status.Active);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == categoryId.Value);
            }

            int totalCount = query.Count();

            query = query.OrderByDescending(x => x.DateCreated).Skip((page-1)*pageSize).Take(pageSize);

            var data = query.ProjectTo<ProductViewModel>().ToList();

            var paginationSet = new PageResult<ProductViewModel>()
            {
                Results = data,
                CurrentPage = page,
                RowCount = totalCount,
                PageSize = pageSize
            };
            return paginationSet;
        }

        public ProductViewModel Add(ProductViewModel productViewModel)
        {
            List<ProductTag> productTags = new List<ProductTag>();
            if (!string.IsNullOrEmpty(productViewModel.Tags))
            {
                string[] tags = productViewModel.Tags.Split(",");
                foreach (string t in tags)
                {
                    var tagId = TextHelper.ToUnsignString(t);
                    if (!_tagRepository.FindAll(x => x.Id == tagId).Any())
                    {
                        Tag tag = new Tag
                        {
                            Id = tagId,
                            Name = t,
                            Type = CommonConstants.ProductTag
                        };
                        _tagRepository.Add(tag);
                    }

                    ProductTag productTag = new ProductTag
                    {
                        ProductId = productViewModel.Id,
                        TagId = tagId
                    };
                    productTags.Add(productTag);
                }
                var product = Mapper.Map<ProductViewModel, Product>(productViewModel);
                foreach (var productTag in productTags)
                {
                    product.ProductTags.Add(productTag);
                }
                _productRepository.Add(product);
            }
            return productViewModel;
        }

        public void Update(ProductViewModel productViewModel)
        {
            List<ProductTag> productTags = new List<ProductTag>();

            if (!string.IsNullOrEmpty(productViewModel.Tags))
            {
                string[] tags = productViewModel.Tags.Split(',');
                foreach (string t in tags)
                {
                    var tagId = TextHelper.ToUnsignString(t);
                    if (!_tagRepository.FindAll(x => x.Id == tagId).Any())
                    {
                        Tag tag = new Tag();
                        tag.Id = tagId;
                        tag.Name = t;
                        tag.Type = CommonConstants.ProductTag;
                        _tagRepository.Add(tag);
                    }
                    _productTagRepository.RemoveMultiple(_productTagRepository.FindAll(x => x.Id == productViewModel.Id).ToList());
                    ProductTag productTag = new ProductTag
                    {
                        TagId = tagId
                    };
                    productTags.Add(productTag);
                }
            }

            var product = Mapper.Map<ProductViewModel, Product>(productViewModel);
            foreach (var productTag in productTags)
            {
                product.ProductTags.Add(productTag);
            }
            _productRepository.Update(product);
        }

        public void Delete(int id)
        {
            _productRepository.Remove(id);
        }

        public ProductViewModel GetById(int id)
        {
            return Mapper.Map<Product, ProductViewModel>(_productRepository.FindById(id));
        }

        public void ImportExcel(string filePath, int categoryId)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                Product product;
                for (int i = workSheet.Dimension.Start.Row + 1; i < workSheet.Dimension.End.Row; i++)
                {
                    product = new Product();
                    product.CategoryId = categoryId;

                    product.Name = workSheet.Cells[i, 1].Value.ToString();

                    product.Description = workSheet.Cells[i, 2].Value.ToString();

                    decimal.TryParse(workSheet.Cells[i, 3].Value.ToString(), out var originalPrice);
                    product.OriginalPrice = originalPrice;

                    decimal.TryParse(workSheet.Cells[i, 4].Value.ToString(), out var price);
                    product.Price = price;
                    decimal.TryParse(workSheet.Cells[i, 5].Value.ToString(), out var promotionPrice);
                    product.PromotionPrice = promotionPrice;

                    product.Content = workSheet.Cells[i, 6].Value.ToString();

                    product.SeoKeywords = workSheet.Cells[i, 7].Value.ToString();

                    product.SeoDescription = workSheet.Cells[i, 8].Value.ToString();
                    product.Status = Status.Active;

                    bool.TryParse(workSheet.Cells[i, 9].Value.ToString(), out var homeFlag);
                    product.HomeFlag = homeFlag;

                    bool.TryParse(workSheet.Cells[i, 10].Value.ToString(), out var hotFlag);
                    product.HotFlag = hotFlag;

                    _productRepository.Add(product);
                }
            }
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public void AddQuantity(int productId, List<ProductQuantityViewModel> quantities)
        {
            _productQuantity.RemoveMultiple(_productQuantity.FindAll(x => x.ProductId == productId).ToList());
            foreach (var quantity in quantities)
            {
                _productQuantity.Add(new ProductQuantity()
                {
                    ProductId = productId,
                    Quantity = quantity.Quantity,
                    SizeId = quantity.SizeId,
                    ColorId = quantity.ColorId,
                });
            }
        }

        public List<ProductQuantityViewModel> GetQuantities(int productId)
        {
            return _productQuantity.FindAll(x => x.ProductId == productId).ProjectTo<ProductQuantityViewModel>().ToList();
        }

        public void AddImages(int productId, string[] images)
        {
            _productImageRepository.RemoveMultiple(_productImageRepository.FindAll(x => x.ProductId == productId).ToList());
            foreach (var image in images)
            {
                _productImageRepository.Add(new ProductImage()
                {
                    ProductId = productId,
                    Caption = string.Empty,
                    Path = image
                });
            }
        }

        public List<ProductImageViewModel> GetImages(int productId)
        {
            return _productImageRepository.FindAll(x => x.ProductId == productId).ProjectTo<ProductImageViewModel>().ToList();
        }

        public void AddWholePrice(int productId, List<WholePriceViewModel> wholePriceVm)
        {
            _wholePriceRepository.RemoveMultiple(_wholePriceRepository.FindAll(x => x.ProductId == productId).ToList());
            foreach (var wholePrice in wholePriceVm)
            {
                _wholePriceRepository.Add(new WholePrice()
                {
                    FromQuantity = wholePrice.FromQuantity,
                    ToQuantity = wholePrice.ToQuantity,
                    Price = wholePrice.Price,
                    ProductId = productId
                });
            }
        }

        public List<WholePriceViewModel> GetWholePrices(int productId)
        {
            return _wholePriceRepository.FindAll(x => x.ProductId == productId).ProjectTo<WholePriceViewModel>().ToList();
        }

        public List<ProductViewModel> GetHotProduct(int top)
        {
            return _productRepository.FindAll(x => x.Status == Status.Active && x.HotFlag == true).OrderByDescending(x => x.DateCreated)
                .Take(top).ProjectTo<ProductViewModel>().ToList();
        }

        public List<ProductViewModel> GetLastest(int top)
        {
            return _productRepository.FindAll(x => x.Status == Status.Active).OrderByDescending(x => x.DateCreated)
                .Take(top).ProjectTo<ProductViewModel>().ToList();
        }

        public List<ProductViewModel> GetRelatedProducts(int id, int top)
        {
            var product = _productRepository.FindById(id);
            return _productRepository.FindAll(x =>
                    x.Status == Status.Active && x.Id != id && x.CategoryId == product.CategoryId)
                .OrderByDescending(x => x.DateCreated)
                .Take(top)
                .ProjectTo<ProductViewModel>()
                .ToList();
        }

        public List<ProductViewModel> GetUpsellProducts(int top)
        {
            return _productRepository.FindAll(x => x.PromotionPrice != null).OrderByDescending(x => x.DateCreated)
                .Take(top)
                .ProjectTo<ProductViewModel>().ToList();
        }

        public List<TagViewModel> GetProductTags(int productId)
        {
            var tags = _tagRepository.FindAll();
            var productTag = _productTagRepository.FindAll();

            var query = from t in tags
                join pt in productTag on t.Id equals pt.TagId
                where pt.ProductId == productId
                select new TagViewModel()
                {
                    Id = t.Id,
                    Name = t.Name
                };
            return query.ToList();
        }

        public bool CheckAvailablility(int productId, int color, int size)
        {
            var quantity =
                _productQuantity.FindSingle(x => x.ProductId == productId && x.ColorId == color && x.SizeId == size);
            if (quantity == null)
            {
                return false;
            }

            return quantity.Quantity > 0;
        }
    }
}
