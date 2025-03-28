using Bulky.DataAccess.Repository.IRepository;
using BulkyWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext db)
        {
            this._db = db;
            dbset = _db.Set<T>();
        }
        void IRepository<T>.Add(T entity)
        {
            dbset.Add(entity);
        }

        T IRepository<T>.Get(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = dbset;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        IEnumerable<T> IRepository<T>.GetAll()
        {
            IQueryable<T> query =dbset;
            return query.ToList();
        }

        void IRepository<T>.Remove(T entity)
        {
            dbset.Remove(entity);
        }

        void IRepository<T>.RemoveRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }
    }
}
