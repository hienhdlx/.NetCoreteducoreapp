using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Application.ViewModels.Blog;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Application.ViewModels.Product;
using TeduCoreApp.Application.ViewModels.Systems;
using TeduCoreApp.Data.Entities;

namespace TeduCoreApp.Application.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<ProductCategory, ProductCategoryViewModel>();
            CreateMap<Function, FunctionViewModel>();
            CreateMap<Product, ProductViewModel>();
            CreateMap<AppUser, AppUserViewModel>();
            CreateMap<AppRole, AppRoleViewModel>();
            CreateMap<Bill, BillViewModel>().MaxDepth(2);
            CreateMap<BillDetail, BillDetailViewModel>().MaxDepth(2);
            CreateMap<Color, ColorViewModel>().MaxDepth(2);
            CreateMap<Size, SizeViewModel>().MaxDepth(2);
            CreateMap<ProductQuantity, ProductQuantityViewModel>().MaxDepth(2);
            CreateMap<ProductImage, ProductImageViewModel>().MaxDepth(2);
            CreateMap<WholePrice, WholePriceViewModel>().MaxDepth(2);

            CreateMap<Blog, BlogViewModel>().MaxDepth(2);
            CreateMap<BlogTag, BlogTagViewModel>().MaxDepth(2);
            CreateMap<SystemConfig, SystemConfigViewModel>().MaxDepth(2);
            CreateMap<Slide, SlideViewModel>().MaxDepth(2);
            CreateMap<Contact, ContactViewModel>().MaxDepth(2);
            CreateMap<Feedback, FeedbackViewModel>().MaxDepth(2);
            CreateMap<Page, PageViewModel>().MaxDepth(2);
        }
    }
}
