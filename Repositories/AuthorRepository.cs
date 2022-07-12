using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess;
using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Repositories.Mappers;

namespace Repositories
{
    public class AuthorRepository : GenericRepository<AuthorEntity>, IAuthorRepository
    {
        public AuthorRepository(LibraryDbContext context)
           : base(context)
        {
        }

        public async Task<AuthorEntity> GetByIdAsync(Guid id)
        {
            var existAuthor = this.context.Authors.FirstOrDefault(x => x.Id == id);

            return existAuthor!;
        }

        public async Task<List<AuthorOutput>> GetAllAuthorsAsync()
        {
            var authors = await this.context.Authors
                .Select(x => Mapper.ToAuthorOutput(x))
                .ToListAsync();

            return authors;
        }

        public AuthorEntity FindAuthorByName(string author)
        {
            return this.context.Authors
                .Where(x => x.AuthorName == author)
                .FirstOrDefault()!;
        }

        public async Task<List<Guid>> FindMultipleAuthorsByNameAsync(string name)
        {
            var result = await this.context.Authors
                .Where(x => x.AuthorName.Contains(name))
                .Select(x => x.Id)
                .ToListAsync();

            return result;
        }

        public List<string> FindAuthorsByBookId(Guid bookId)
        {
            var authorNames = new List<string>();

            var authorIds = this.context.AuthorsBooks
                .Where(x => x.BookEntityId == bookId)
                .Select(x => x.AuthorEntityId)
                .ToList();

            foreach (Guid id in authorIds)
            {
                var authorName = this.context.Authors.FirstOrDefault(a => a.Id == id)!.AuthorName;

                authorNames.Add(authorName);
            }

            return authorNames;
        }

        public async Task<bool> ContainsAuthor(Guid id, string name)
        {
            var author = this.FindAuthorByName(name);

            if (author is null || author.Id == id)
            {
                return false;
            }

            return true;
        }

        public async Task<(List<AuthorOutput>, int)> SearchForAuthorsAsync(SearchAuthorDto input, PaginatorInputDto pagination)
        {
            var query = this.context.Authors
                .Where(x => input.AuthorName == null || x.AuthorName.Contains(input.AuthorName));

            var result = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .AsSplitQuery()
                .Select(x => Mapper.ToAuthorOutput(x))
                .ToListAsync();

            var totalCount = query.Count();

            return (result, totalCount);
        }
    }
}
