using System;
using System.Collections.Generic;
using System.Text;
using TeduCoreApp.Data.Entities;
using TeduCoreApp.Data.IRepositories;

namespace TeduCoreApp.Data.EF.Repositories
{
    public class WholePriveRepository: EFRepository<WholePrice, int > , IWholePriceRepository
    {
        public WholePriveRepository(AppDbContext context) : base(context)
        {
        }
    }
}
