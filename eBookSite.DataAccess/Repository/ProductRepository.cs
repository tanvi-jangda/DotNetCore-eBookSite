using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eBookSite.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>,IProductRepository
    {
        private ApplicationDBContext _dbContext;
        public ProductRepository(ApplicationDBContext db):base(db)
        {
            _dbContext = db;
        }
        public void Update(Product obj)
        {
           _dbContext.Update(obj);
        }
    }
}
