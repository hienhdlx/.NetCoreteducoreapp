using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;

namespace TeduCoreApp.Data.EF.Repositories
{
    public class ProductRepository : EFRepository<Product, int>, IProductRepository
    {
        private AppDbContext _context;
        public ProductRepository(AppDbContext context):base(context)
        {
            _context = context;
        }

        public Product FindById(string id, params Expression<Func<Product, object>>[] includeProperties)
        {
            throw new NotImplementedException();
        }

        public void Remove(string id)
        {
            throw new NotImplementedException();
        }
    }
}
