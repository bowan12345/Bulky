using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

 /*       void IProductRepository.Save()
        {
            _db.SaveChanges();
        }*/

        public void Update(Product product)
        {
            var productfromDb = _db.Products.FirstOrDefault(p => p.Id == product.Id);
            if (productfromDb != null)
            {
                productfromDb.Title = product.Title;
                productfromDb.ISBN = product.ISBN;
                productfromDb.Price = product.Price;
                productfromDb.ListPrice = product.ListPrice;
                productfromDb.Price50 = product.Price50;
                productfromDb.Price100 = product.Price100;
                productfromDb.Author = product.Author;
                productfromDb.Description = product.Description;
                productfromDb.CategoryId = product.CategoryId;
                if (product.ImageUrl != null)
                {
                    productfromDb.ImageUrl = product.ImageUrl;
                }

                _db.Update(productfromDb);
            }
        }
    }
}
