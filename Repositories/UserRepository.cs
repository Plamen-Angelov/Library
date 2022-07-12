using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;

namespace Repositories
{
    public class UserRepository : GenericRepository<UserEntity>, IUserRepository
    {
        public UserRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<int> GetCountOfAllReadersAsync()
        {
            var allReaders = await this.context.Users
                .CountAsync();

            return allReaders;
        }
    }
}
