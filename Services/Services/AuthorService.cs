using Services.Interfaces;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;

using Microsoft.Extensions.Logging;

using Repositories.Interfaces;
using Repositories.Mappers;
using static Common.ExceptionMessages;

namespace Services.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository authorRepository;
        private readonly IBookService bookService;
        private readonly ILogger<AuthorService> logger;

        public AuthorService(IAuthorRepository authorRepository, IBookService bookService, ILogger<AuthorService> logger)
        {
            this.authorRepository = authorRepository;
            this.bookService = bookService;
            this.logger = logger;
        }

        public async Task<AuthorOutput> AddAuthorAsync(AuthorDto input)
        {
            input.AuthorName = input.AuthorName?.Trim()!;

            if (authorRepository.FindAuthorByName(input.AuthorName) is not null)
            {
                logger.LogError($"Service throws exception: {AUTHOR_EXISTS}");
                throw new ArgumentException(AUTHOR_EXISTS);
            }

            AuthorEntity authorEntity = Mapper.ToAuthorEntity(input.AuthorName);
            AuthorEntity created = await authorRepository.InsertAsync(authorEntity);
            await authorRepository.SaveAsync();
            logger.LogInformation($"Author {input.AuthorName} was added to the database.");

            AuthorOutput authorOutput = Mapper.ToAuthorOutput(created);
            return authorOutput;
        }

        public async Task<List<AuthorOutput>> GetAllAuthorsAsync()
        {
            var authors = await authorRepository.GetAllAuthorsAsync();

            if (authors.Count == 0)
            {
                logger.LogError($"Service received 0 authors.");
                throw new ArgumentException(AUTHOR_NOT_FOUND);
            }

            return authors;
        }

        public async Task<(List<AuthorOutput>, int)> GetAuthorsAsync(PaginatorInputDto input)
        {
            var (entities, totalCount) = await authorRepository.GetEntityPageAsync(input);

            if (totalCount == 0)
            {
                logger.LogError("Service got total count of authors = 0");
                throw new NullReferenceException(NO_AUTHORS_FOUND);
            }

            var result = new List<AuthorOutput>();

            entities.ForEach(entity => result.Add(Mapper.ToAuthorOutput(entity)));

            return (result, totalCount);
        }

        public async Task<AuthorOutput> GetAuthorByIdAsync(Guid authorId)
        {
            var entity = await authorRepository.GetByIdAsync(authorId);

            if (entity is null)
            {
                logger.LogError($"Service didn't receive an author with id {authorId}");
                throw new ArgumentException(AUTHOR_NOT_FOUND);
            }

            var authorOutput = Mapper.ToAuthorOutput(entity);
            return authorOutput;
        }

        public async Task<AuthorOutput> UpdateAuthorAsync(AuthorDto input, Guid authorId)
        {
            input.AuthorName = input.AuthorName.Trim();

            var entity = await authorRepository.GetByIdAsync(authorId);

            if (entity is null)
            {
                logger.LogError("Service didn't receive the author entitiy that have to be updated");
                throw new ArgumentNullException(AUTHOR_NOT_FOUND);
            }

            if (await authorRepository.ContainsAuthor(authorId, input.AuthorName))
            {
                logger.LogError("Service throws exception, the new name of the author already exists");
                throw new ArgumentException(AUTHOR_EXISTS);
            }

            entity.AuthorName = input.AuthorName;
            AuthorEntity updated = await authorRepository.UpdateAsync(entity);
            await authorRepository.SaveAsync();

            AuthorOutput authorOutput = Mapper.ToAuthorOutput(updated);

            return authorOutput;
        }

        public async Task<(List<AuthorOutput>, int)> SearchForAuthorsAsync(SearchAuthorDto input, PaginatorInputDto pagination)
        {
            var (result, totalCount) = await this.authorRepository.SearchForAuthorsAsync(input, pagination);

            if (totalCount == 0)
            {
                logger.LogError("Servie didn't receive found authors.");
                throw new NullReferenceException(NO_AUTHORS_FOUND);
            }

            return (result, totalCount);
        }

        public async Task DeleteAuthorAsync(Guid authorId)
        {
            var entity = await authorRepository.GetByIdAsync(authorId);

            if (entity is null)
            {
                logger.LogError($"Service throws exception: {AUTHOR_NOT_FOUND}");
                throw new NullReferenceException(AUTHOR_NOT_FOUND);
            }

            var authorBooks = await bookService.GetBooksNumberForAuthorAsync(authorId);

            if (authorBooks > 0)
            {
                logger.LogError("Service error - Author can't be deleted, because it is assigned to book(s)");
                throw new ArgumentException(AUTHOR_HAS_BOOKS);
            }

            await authorRepository.DeleteAsync(authorId);
            await authorRepository.SaveAsync();
        }
    }
}
