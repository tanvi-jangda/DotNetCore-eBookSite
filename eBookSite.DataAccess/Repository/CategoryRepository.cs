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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDBContext _dbContext;
        public CategoryRepository(ApplicationDBContext _db):base(_db)
        {
            _dbContext = _db;
        }

        public void Update(Category obj)
        {
        _dbContext.Update(obj);
        }
    }
}
