using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Contracts;
using Data.ContextDb;
using Data.Models;

namespace Business.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;

        public UserRepository(SportsStoreDbContext sportsStoreDbContext) : base(sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
        }

    }
}
