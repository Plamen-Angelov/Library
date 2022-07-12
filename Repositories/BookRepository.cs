using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Mappers;

namespace Repositories
{
    public class BookRepository : GenericRepository<BookEntity>, IBookRepository
    {
        public BookRepository(LibraryDbContext context) 
            : base(context)
        {
        }

        public BookEntity FindBookByTitle(AddBookDto book)
        {
           return this.context.Books.Where(x => x.Title == book.BookTitle).FirstOrDefault()!;
        }

        public async Task<List<BookOutput>> GetAllBooksAsync()
        {
            var books = await context.Books
                .Select(x => Mapper.ToBookOutput(x))
                .ToListAsync();

            return books;
        }

        public async Task<bool> ContainsBookName(Guid bookId, string bookTitle)
        {
            var result = await context.Books.FirstOrDefaultAsync(x => x.Title == bookTitle);

            if (result == null || result.Id == bookId)
            {
                return false;
            }

            return true;
        }

        public async Task<int> GetBooksNumberForGenre(Guid genreId)
        {
            var result = await this.context.GenresBooks
                .Where(x => x.GenreEntityId == genreId)
                .CountAsync();

            return result;
        }

        public async Task<int> GetBooksNumberForAuthor(Guid authorId)
        {
            var result = await this.context.AuthorsBooks
                .Where(x => x.AuthorEntityId == authorId)
                .CountAsync();

            return result;
        }

        public async Task<int> GetCountOfAllBooksAsync()
        {
            var totalCount = await this.context.Books
                .CountAsync();

            return totalCount;
        }

        public async Task<(List<LastBooksOutput>, int)> GetBooksForLastTwoWeeksAsync(PaginatorInputDto pagination)
        {
            DateTime today = DateTime.Today;
            DateTime fourteenDaysEarlier = today.AddDays(-14);

            var lastBooks = context.Books
                .Where(x => x.CreatedOn >= fourteenDaysEarlier)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => Mapper.ToLastBooksOutput(x));

            var result = await lastBooks
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var lastBooksCount = lastBooks.Count();

            return (result, lastBooksCount);
        }

        public async Task<(List<BookOutput>, int)> SearchForBooksAsync(SearchBookDto input, List<Guid> authors, List<Guid> genres, PaginatorInputDto pagination)
        {
            var query = this.context.Books
                .Where(x => input.Title == null || x.Title.Contains(input.Title))
                .Where(x => input.Description != null ? x.Description!.Contains(input.Description) : true)
                .Include(x => x.AuthorsBooks)
                .Where(x => input.Author != null ? x.AuthorsBooks.Any(y => authors.Contains(y.AuthorEntityId)) : true)
                .Include(x => x.GenresBooks)
                .Where(x => input.Genre != null ? x.GenresBooks.Any(y => genres.Contains(y.GenreEntityId)) : true);

            var result = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .AsSplitQuery()
                .Select(x => Mapper.ToBookOutput(x))
                .ToListAsync();

            var totalCount = query.Count();

            return (result, totalCount);
        }
    }
}

