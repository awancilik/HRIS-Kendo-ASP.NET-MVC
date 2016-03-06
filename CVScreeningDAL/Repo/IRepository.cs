using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CVScreeningDAL.Repo
{
    public interface IRepository<T>
    {
        IQueryable<T> AsQueryable(bool globalScope=false);
        IQueryable<TChildType> AsQueryable<TChildType>(bool globalScope = false);
        IEnumerable<T> GetAll(bool globalScope = false);
        IEnumerable<T> Find(Expression<Func<T, bool>> where, bool globalScope = false);
        T Single(Expression<Func<T, bool>> where, bool globalScope = false);
        T First(Expression<Func<T, bool>> where, bool globalScope=false);
        bool Exist(Expression<Func<T, bool>> where, bool globalScope=false);
        Int32 CountAll(bool globalScope = false);
        void Delete(T entity);
        T Add(T entity);
        void Attach(T entity);
    }
}