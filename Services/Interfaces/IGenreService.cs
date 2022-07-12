using Common.Models.InputDTOs;
using Common.Models.OutputDtos;

namespace Services.Interfaces
{
    public interface IGenreService
    {
        Task<GenreOutput> AddGenreAsync(Genre input);
        Task<GenreOutput> UpdateGenreAsync(Guid genreId, Genre input);
        Task DeleteGenreAsync(Guid genreId);
        Task<(List<GenreOutput>, int)> GetGenresAsync(PaginatorInputDto input);
        Task<List<GenreOutput>> GetAllGenresAsync();
        Task<GenreOutput> GetGenreByIdAsync(Guid genreId);
        Task<int> GetCountOfAllGenresAsync();
        Task<(List<GenreOutput>, int)> SearchForGenresAsync(SearchGenreDto input, PaginatorInputDto pagination);
    }
}
