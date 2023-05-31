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

        public IProductRepository Product {get; private set;}

        public IShoppingCartRepository ShoppingCart { get; private set;}
        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; } 

        public UnitOfWork(ApplicationDBContext db)
        {
            _dbContext = db;
            Category = new CategoryRepository(db);
            Product = new ProductRepository(db);
            ShoppingCart = new ShoppingCartRepository(db);
            ApplicationUser = new ApplicationUserRepository(db);
            OrderHeader=new OrderHeaderRepository(db);
            OrderDetail=new OrderDetailRepository(db);
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }
    }
}
