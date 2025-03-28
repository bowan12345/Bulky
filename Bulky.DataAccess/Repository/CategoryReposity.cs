using Bulky.DataAccess.Repository.IRepository;
using BulkyWeb.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class CategoryReposity : Repository<Category>, ICategoryReposity
    {
        private ApplicationDbContext _db;
        public CategoryReposity(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }


        void ICategoryReposity.Update(Category category)
        {
            _db.Update(category);
        }
    }
}
