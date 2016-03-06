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
    public class InMemoryRepository<T> : IRepository<T> where T : IEntity
    {
        private readonly List<T> _memSet;
        private int _idCounter;

        public InMemoryRepository()
        {
            _memSet = new List<T>();
            _idCounter = 1;
        }

        public IQueryable<T> AsQueryable(bool globalScope = false)
        {
            return _memSet.AsQueryable();
        }

        public IQueryable<TChildType> AsQueryable<TChildType>(bool globalScope = false)
        {
            return typeof(TChildType).IsSubclassOf(typeof(T)) ? _memSet.OfType<TChildType>().AsQueryable() : null;
        }

        public IEnumerable<T> GetAll(bool globalScope = false)
        {
            return _memSet;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _memSet.Where(where.Compile());
        }

        public T Single(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _memSet.Single(where.Compile());
        }

        public bool Exist(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _memSet.FirstOrDefault(where.Compile()) != null;
        }

        public T First(Expression<Func<T, bool>> where, bool globalScope = false)
        {
            return _memSet.FirstOrDefault(where.Compile());
        }

        public int CountAll(bool globalScope = false)
        {
            return _memSet.Count();
        }

        public void Delete(T entity)
        {
            _memSet.Remove(entity);
        }

        public T Add(T entity)
        {
            entity.SetId(_idCounter++);
            _memSet.Add(entity);
            return entity;
        }

        public void Attach(T entity)
        {
            _memSet.Add(entity);
        }
    }
}