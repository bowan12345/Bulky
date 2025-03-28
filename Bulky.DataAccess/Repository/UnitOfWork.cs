using Bulky.DataAccess.Repository.IRepository;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public ICategoryReposity categoryReposity { get; private set; }

        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            categoryReposity = new CategoryReposity(_db);
        }

        public void Save() 
        {
            _db.SaveChanges();
        }

    }
}
