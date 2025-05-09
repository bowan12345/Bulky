using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository categoryRepository { get; }

        IProductRepository productRepository { get; }
        ICompanyRepository companyRepository { get; }
        IShoppingCartRepository shoppingCartRepository { get; }
        IApplicationUserRepository applicationUserRepository { get; }
        IOrderHeaderRepository orderHeaderRepository { get; }
        IOrderDetailRepository orderDetailRepository { get; }
        void Save();
    }
}
