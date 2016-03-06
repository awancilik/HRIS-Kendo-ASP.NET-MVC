using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using CVScreeningCore.Models;
using CVScreeningDAL.EntityFramework;

namespace CVScreeningDAL.Repo
{
    public class EntityRepository<T> : IRepository<T> where T : class, IEntity
    {
        private readonly IDbSet<T> _dbSet;

        /// <summary>
        /// Tenant ID used for this repository
        /// </summary>
        private readonly Byte _tenantId;

        /// <summary>
        /// Filter expression used for multitnenat
        /// </summary>
        private readonly Expression<Func<T, bool>> _filter;

        public EntityRepository(CVScreeningEFContext dbContext, 
            Byte tenantId, 
            Expression<Func<T, bool>> filter)
        {
            _dbSet = dbContext.Set<T>();
            _tenantId = tenantId;
            _filter = filter;
        }

        public EntityRepository(CVScreeningEFContext dbContext)
        {
            _dbSet = dbContext.Set<T>();
            _filter = null;
        }
        
        public IQueryable<T> AsQueryable(bool globalScope = false)
        {

            return _filter == null
                ? _dbSet
                : _dbSet.Where(_filter);
        }

        public IQueryable<TChildType> AsQueryable<TChildType>(bool globalScope = false)
        {
            if (_filter == null)
            {
                return typeof(TChildType).IsSubclassOf(typeof(T)) ? _dbSet.OfType<TChildType>() : null;
            }
            else
            {
                return typeof(TChildType).IsSubclassOf(typeof(T)) ? _dbSet.Where(_filter).OfType<TChildType>() : null;
            }

        }

        public IEnumerable<T> GetAll(bool globalScope = false)
        {
            return _filter == null
                ? _dbSet.ToList()
                : _dbSet.Where(_filter).ToList();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _filter == null
                ? _dbSet.Where(where)
                : _dbSet.Where(where).Where(_filter);
        }

        public T Single(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _filter == null
                ? _dbSet.Single(where)
                : _dbSet.Where(_filter).Single(where);
        }

        public bool Exist(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _filter == null
                ? _dbSet.FirstOrDefault(where) != null
                : _dbSet.Where(_filter).FirstOrDefault(where) != null;
        }

        public T First(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _filter == null
                ? _dbSet.FirstOrDefault(where)
                : _dbSet.Where(_filter).FirstOrDefault(where);
        }

        public int CountAll(bool globalScope=false)
        {
            return _filter == null
                ? _dbSet.Count()
                : _dbSet.Count(_filter);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public T Add(T entity)
        {
            _dbSet.AddOrUpdate(entity);
            return entity;
        }

        public void Attach(T entity)
        {
            _dbSet.Attach(entity);
        }
    }
}