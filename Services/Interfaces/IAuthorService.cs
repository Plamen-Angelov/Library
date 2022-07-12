using Common.Models.InputDTOs;
using Common.Models.OutputDtos;

namespace Services.Interfaces
{
    public interface IAuthorService
    {
        Task<AuthorOutput> AddAuthorAsync(AuthorDto input);

        Task<List<AuthorOutput>> GetAllAuthorsAsync();

        Task<(List<AuthorOutput>, int)> GetAuthorsAsync(PaginatorInputDto input);

        Task<AuthorOutput> GetAuthorByIdAsync(Guid authorId);

        Task<AuthorOutput> UpdateAuthorAsync(AuthorDto input, Guid authorId);

        Task<(List<AuthorOutput>, int)> SearchForAuthorsAsync(SearchAuthorDto input, PaginatorInputDto pagination);

        Task DeleteAuthorAsync(Guid authorId);
    }
}
