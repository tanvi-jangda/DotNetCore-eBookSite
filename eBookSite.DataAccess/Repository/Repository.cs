﻿using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eBookSite.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDBContext _dbContext;
        internal DbSet<T> _dbSet;
        public Repository(ApplicationDBContext db)
        {
            this._dbContext = db;
        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = _dbSet;
            return query.ToList();
            
        }

        public T GetById(Expression<Func<T, bool>> filter)
        {
           IQueryable<T> query = _dbSet.Where(filter);
            return query.FirstOrDefault(); ;
        }

        public void Remove(T entity)
        {
          _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
           _dbSet.RemoveRange(entities);
        }
    }
}
