using log4net;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using Repositories.Interfaces;
using Repositories.Mappers;
using Services.Interfaces;
using static Common.ExceptionMessages;

namespace Services.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository genreRepository;
        private readonly IBookService bookService;
        private readonly ILog log = LogManager.GetLogger(typeof(GenreService));

        public GenreService(IGenreRepository genreRepository, IBookService bookService)
        {
            this.genreRepository = genreRepository;
            this.bookService = bookService;
        }

        public async Task<GenreOutput> AddGenreAsync(Genre input)
        {
            input.Name = input.Name?.Trim()!;

            if (genreRepository.ContainsGenreName(input.Name))
            {
                log.Error($"AddGenre method throws exception {GENRE_EXISTS}");
                throw new ArgumentException(GENRE_EXISTS);
            }

            var genreEntity = Mapper.ToGenreEntity(input);

            var createdEntity = await genreRepository.InsertAsync(genreEntity);
            await genreRepository.SaveAsync();

            var result = Mapper.ToGenreOutput(createdEntity);
            return result;
        }

        public async Task<GenreOutput> UpdateGenreAsync(Guid genreId, Genre input)
        {
            input.Name = input.Name?.Trim()!;

            var existingEntity = await genreRepository.GetByIdAsync(genreId);

            if (existingEntity == null)
            {
                log.Error($"UpdateGenre method throws exception {GENRE_NOT_FOUND}");
                throw new NullReferenceException(GENRE_NOT_FOUND);
            }

            if (await genreRepository.ContainsGenreName(genreId, input.Name))
            {
                log.Error($"UpdateGenre method throws exception {GENRE_EXISTS}");
                throw new ArgumentException(GENRE_EXISTS);
            }

            existingEntity.Name = input.Name;

            var updatedEntity = await genreRepository.UpdateAsync(existingEntity);
            await genreRepository.SaveAsync();

            var result = Mapper.ToGenreOutput(updatedEntity);

            return result;
        }

        public async Task DeleteGenreAsync(Guid genreId)
        {
            var existingEntity = await genreRepository.GetByIdAsync(genreId);

            if (existingEntity == null)
            {
                log.Error($"Delete method throws exception {GENRE_NOT_FOUND}");
                throw new NullReferenceException(GENRE_NOT_FOUND);
            }

            var genreBooks = await bookService.GetBooksNumberForGenreAsync(genreId);

            if (genreBooks > 0)
            {
                log.Error($"Delete method throws exception {GENRE_HAS_BOOKS}");
                throw new ArgumentException(GENRE_HAS_BOOKS);
            }

            await genreRepository.DeleteAsync(genreId);
            await genreRepository.SaveAsync();
        }

        public async Task<(List<GenreOutput>, int)> GetGenresAsync(PaginatorInputDto input)
        {
            var (entities, totalCount) = await genreRepository.GetEntityPageAsync(input);

            if (totalCount == 0)
            {
                log.Error($"GetGenres method throws exception {NO_GENRES_FOUND}");
                throw new NullReferenceException(NO_GENRES_FOUND);
            }

            var result = new List<GenreOutput>();

            entities.ForEach(entity => result.Add(Mapper.ToGenreOutput(entity)));

            return (result, totalCount);
        }

        public async Task<List<GenreOutput>> GetAllGenresAsync()
        {
            var result = await genreRepository.GetAllGenresAsync();

            if (result.Count == 0)
            {
                log.Error($"GetAllGenres method throws exception {NO_GENRES_FOUND}");
                throw new NullReferenceException(NO_GENRES_FOUND);
            }

            return result;
        }

        public async Task<GenreOutput> GetGenreByIdAsync(Guid genreId)
        {
            var existingEntity = await genreRepository.GetByIdAsync(genreId);

            if (existingEntity == null)
            {
                log.Error($"GetGenreById method throws exception {GENRE_NOT_FOUND}");
                throw new NullReferenceException(GENRE_NOT_FOUND);
            }

            var result = Mapper.ToGenreOutput(existingEntity);

            return result;
        }

        public async Task<int> GetCountOfAllGenresAsync()
        {
            var allGenresCount = await this.genreRepository.GetCountOfAllGenresAsync();

            return allGenresCount;
        }

        public async Task<(List<GenreOutput>, int)> SearchForGenresAsync(SearchGenreDto input, PaginatorInputDto pagination)
        {
            var (result, totalCount) = await this.genreRepository.SearchForGenresAsync(input, pagination);

            if (totalCount == 0)
            {
                throw new NullReferenceException(NO_GENRES_FOUND);
            }

            return (result, totalCount);
        }
    }
}
