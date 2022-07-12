using DataAccess;
using DataAccess.Entities;
using Repositories.Interfaces;

namespace Repositories
{
    public class AuthorsBooksRepository : GenericRepository<AuthorsBooks>, IAuthorsBooksRepository
    {
        public AuthorsBooksRepository(LibraryDbContext context)
           : base(context)
        {
        }

        public void DeleteAuthorEntriesForBook(Guid bookId)
        {
            var entries = this.context.AuthorsBooks
                .Where(x => x.BookEntityId == bookId)
                .ToList();

            if (entries.Count > 0)
            { 
                this.context.AuthorsBooks.RemoveRange(entries);
            }
        }
    }
}
