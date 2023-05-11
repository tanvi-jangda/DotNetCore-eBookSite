using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBookSite.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDBContext _dbContext;
        public ICategoryRepository Category { get; private set; }
        public UnitOfWork(ApplicationDBContext db)
        {
            _dbContext = db;
            Category = new CategoryRepository(db);
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
