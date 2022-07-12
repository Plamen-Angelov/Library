using DataAccess;
using DataAccess.Entities;
using Repositories.Interfaces;

namespace Repositories
{
    public class GenresBooksRepository : GenericRepository<GenresBooks>, IGenresBooksRepository
    {
        public GenresBooksRepository(LibraryDbContext context)
           : base(context)
        {
        }

        public void DeleteGenreEntriesForBook(Guid bookId)
        {
            var entries = this.context.GenresBooks
                .Where(x => x.BookEntityId == bookId)
                .ToList();

            if (entries.Count > 0)
            {
                this.context.GenresBooks.RemoveRange(entries);
            }
        }
    }
}
