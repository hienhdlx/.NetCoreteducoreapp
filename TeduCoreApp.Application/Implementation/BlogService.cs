using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using TeduCoreApp.Application.Interfaces;
using TeduCoreApp.Application.ViewModels.Blog;
using TeduCoreApp.Application.ViewModels.Common;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.Enums;
using TeduCoreApp.Data.IRepositories;
using TeduCoreApp.Infrastructure.Interfaces;
using TeduCoreApp.Utilities.Constants;
using TeduCoreApp.Utilities.Dtos;
using TeduCoreApp.Utilities.Helpers;

namespace TeduCoreApp.Application.Implementation
{
    public class BlogService: IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IBlogTagRepository _blogTagRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BlogService(IBlogRepository blogRepository, ITagRepository tagRepository, IBlogTagRepository blogTagRepository, IUnitOfWork unitOfWork)
        {
            _blogRepository = blogRepository;
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
            _blogTagRepository = blogTagRepository;
        }

        public BlogViewModel Add(BlogViewModel blogVM)
        {
            var blog = Mapper.Map<BlogViewModel, Blog>(blogVM);
            if (!string.IsNullOrEmpty(blog.Tags))
            {
                var tags = blog.Tags.Split(",");
                foreach (var t in tags)
                {
                    var tagId = TextHelper.ToUnsignString(t);
                    if (!DynamicQueryableExtensions.Any(_tagRepository.FindAll(x => x.Id == tagId)))
                    {
                        Tag tag = new Tag()
                        {
                            Id = tagId,
                            Name = t,
                            Type = CommonConstants.BlogTag
                        };
                        _tagRepository.Add(tag);
                    }
                    var blogTag = new BlogTag()
                    {
                        TagId = tagId
                    };
                    blog.BlogTags.Add(blogTag);
                }
            }
            _blogRepository.Add(blog);
            return blogVM;
        }

        public void Update(BlogViewModel blogVm)
        {
            _blogRepository.Update(Mapper.Map<BlogViewModel, Blog>(blogVm));
            if (!string.IsNullOrEmpty(blogVm.Tags))
            {
                string[] tags = blogVm.Tags.Split(",");
                foreach (var t in tags)
                {
                    var tagId = TextHelper.ToUnsignString(t);
                    if (!DynamicQueryableExtensions.Any(_tagRepository.FindAll(x => x.Id == tagId)))
                    {
                        Tag tag = new Tag()
                        {
                            Id = tagId,
                            Name = t,
                            Type = CommonConstants.BlogTag
                        };
                        _tagRepository.Add(tag);
                    }
                    _blogTagRepository.RemoveMultiple(_blogTagRepository.FindAll(x => x.Id == blogVm.Id).ToList());
                    BlogTag blogtag = new BlogTag()
                    {
                        BlogId = blogVm.Id,
                        TagId = tagId
                    };
                    _blogTagRepository.Add(blogtag);
                }

            }
        }

        public void Delete(int id)
        {
            _blogRepository.Remove(id);
        }

        public List<BlogViewModel> GetAll()
        {
            return _blogRepository.FindAll(x => x.BlogTags).ProjectTo<BlogViewModel>().ToList();
        }

        public PageResult<BlogViewModel> GetAllPaging(string keyword, int pageSize, int page)
        {
            var query = _blogRepository.FindAll();
            if (!string.IsNullOrEmpty(keyword)) query = _blogRepository.FindAll(x => x.Name.Contains(keyword));

            int totalRow = query.Count();
            var data = query.OrderByDescending(x => x.DateCreated).Skip((page - 1) * pageSize).Take(pageSize);
            var paginationSet =  new PageResult<BlogViewModel>()
            {
                Results = data.ProjectTo<BlogViewModel>().ToList(),
                CurrentPage = page,
                RowCount = totalRow,
                PageSize = pageSize
            };
            return paginationSet;
        }

        public List<BlogViewModel> GetLastest(int top)
        {
            return _blogRepository.FindAll(x => x.Status == Status.Active).OrderByDescending(x => x.DateCreated)
                .Take(top).ProjectTo<BlogViewModel>().ToList();
        }

        public List<BlogViewModel> GetHotProduct(int top)
        {
            return _blogRepository.FindAll(x => x.Status == Status.Active && x.HotFlag == true)
                .OrderByDescending(x => x.DateCreated)
                .Take(top)
                .ProjectTo<BlogViewModel>().ToList();
        }

        public List<BlogViewModel> GetListPaging(int page, int pageSize, string sort, out int totalRow)
        {
            var query = _blogRepository.FindAll(x => x.Status == Status.Active);
            switch (sort)
            {
                case "popular":
                    query = query.OrderByDescending(x => x.ViewCount);
                    break;
                default:
                    query = query.OrderByDescending(x => x.DateCreated);
                    break;
            }

            totalRow = query.Count();
            return query.Skip((page - 1) * pageSize).Take(pageSize).ProjectTo<BlogViewModel>().ToList();
        }

        public List<BlogViewModel> Search(string keyword, int page, int pageSize, string sort, out int totalRow)
        {
            var query = _blogRepository.FindAll(x => x.Status == Status.Active);
            if (!string.IsNullOrEmpty(keyword))
                query = _blogRepository.FindAll(x => x.Name.Contains(keyword) && x.Status == Status.Active);
            switch (sort)
            {
                case "popular":
                    query = query.OrderByDescending(x => x.ViewCount);
                    break;
                default:
                    query = query.OrderByDescending(x => x.DateCreated);
                    break;
            }

            totalRow = query.Count();
            return query.Skip((page - 1) * pageSize).Take(pageSize).ProjectTo<BlogViewModel>().ToList();
        }

        public List<BlogViewModel> GetList(string keyword)
        {
            var query = !string.IsNullOrEmpty(keyword)
                ? _blogRepository.FindAll(x => x.Name.Contains(keyword) && x.Status == Status.Active)
                    .ProjectTo<BlogViewModel>().ToList()
                : _blogRepository.FindAll(x => x.Status == Status.Active).ProjectTo<BlogViewModel>().ToList();
            return query;
        }

        public List<BlogViewModel> GetReatedBlogs(int id, int top)
        {
            return _blogRepository.FindAll(x => x.Status == Status.Active && x.Id != id)
                .OrderByDescending(x => x.DateCreated).Take(top).ProjectTo<BlogViewModel>().ToList();
        }

        public List<string> GetListByName(string name)
        {
            return _blogRepository.FindAll(x => x.Status == Status.Active && x.Name.Contains(name)).Select(x => x.Name).ToList();
        }

        public BlogViewModel GetById(int id)
        {
            return Mapper.Map<Blog, BlogViewModel>(_blogRepository.FindById(id));
        }

        public void Save()
        {
            _unitOfWork.Commit();
        }

        public List<TagViewModel> GetListTagById(int id)
        {
            return _blogTagRepository.FindAll(x => x.BlogId == id, c => c.Tag).Select(a => a.Tag)
                .ProjectTo<TagViewModel>().ToList();
        }

        public TagViewModel GetTag(string tagId)
        {
            return Mapper.Map<Tag, TagViewModel>(_tagRepository.FindSingle(x => x.Id == tagId));
        }

        public void IncreaseView(int id)
        {
            var product = _blogRepository.FindById(id);
            if (product.ViewCount.HasValue)
            {
                product.ViewCount += 1;
            }
            else
            {
                product.ViewCount = 1;
            }
        }

        public List<BlogViewModel> GetListByTag(string tagId, int page, int pagesize, out int totalRow)
        {
            var query = from p in _blogRepository.FindAll()
                join pt in _blogTagRepository.FindAll() on p.Id equals pt.BlogId
                where pt.TagId == tagId && p.Status == Status.Active
                orderby p.DateCreated descending
                select p;

            totalRow = query.Count();
            query = query.Skip((page - 1) * pagesize).Take(pagesize);
            var model = query.ProjectTo<BlogViewModel>();
            return model.ToList();
        }

        public List<TagViewModel> GetListTag(string searchText)
        {
            return _tagRepository.FindAll(x => x.Type == CommonConstants.BlogTag && searchText.Contains(x.Name))
                .ProjectTo<TagViewModel>().ToList();
        }
    }
}
