using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;

namespace Business.Contracts
{
    public interface ICategoryRepository: IRepository<Category>
    {
        void Update(Category category);
    }
}
