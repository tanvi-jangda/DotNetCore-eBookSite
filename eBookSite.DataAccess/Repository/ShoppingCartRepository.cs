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
    public class ShoppingCartRepository : Repository<ShoppingCart>,IShoppingCartRepository
    {
        private ApplicationDBContext _dbContext;
        public ShoppingCartRepository(ApplicationDBContext dBContext):base(dBContext)
        {
            _dbContext = dBContext;
        }
        public void update(ShoppingCart cart)
        {
            _dbContext.Update(cart);
        }
    }
}
