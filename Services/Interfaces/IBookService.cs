using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;

namespace Services.Interfaces
{
    public interface IBookService
    {
        Task<BookOutput> GetBookByIdAsync(Guid bookId);
        Task<BookEntity> AddBookAsync(AddBookDto book);
        Task<BookOutput> UpdateBookAsync(Guid bookId, AddBookDto input);
        Task DeleteBookAsync(Guid bookId);
        Task<(List<BookOutput>, int)> GetBooksAsync(PaginatorInputDto input);
        Task<List<BookOutput>> GetAllBooksAsync();
        Task<int> GetBooksNumberForGenreAsync(Guid genreId);
        Task<int> GetBooksNumberForAuthorAsync(Guid genreId);
        Task<int> GetCountOfAllBooksAsync();
        Task<(List<LastBooksOutput>, int)> GetBooksForLastTwoWeeksAsync(PaginatorInputDto pagination);
        Task<(List<BookOutput>, int)> SearchForBooksAsync(SearchBookDto input, PaginatorInputDto pagination);
    }
}