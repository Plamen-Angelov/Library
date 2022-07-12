using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess;
using DataAccess.Entities;
using NUnit.Framework;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.RepositoryTests
{
    [TestFixture]
    public class AuthorsBooksRepositoryTests
    {
        private LibraryDbContext? inMemoryContext;
        private AuthorsBooksRepository? authorsBooksRepository;

        [SetUp]
        public void Init()
        {
            inMemoryContext = InMemoryDbContext.GetInMemoryDbContext("InMemoryDb");
            inMemoryContext.Database.EnsureDeleted();
            authorsBooksRepository = new AuthorsBooksRepository(inMemoryContext);


            List<AuthorEntity> authors = new List<AuthorEntity>
            {
                new AuthorEntity { AuthorName = "John Smith",  Id = Guid.Parse("1117baea-311f-4387-9b9b-ef4c6ec8b5ce") },
                new AuthorEntity { AuthorName = "Manuel Alvarez",  Id = Guid.Parse("2227baea-311f-4387-9b9b-ef4c6ec8b5ce") },
                new AuthorEntity { AuthorName = "Author 1", Id = Guid.Parse("3337baea-311f-4387-9b9b-ef4c6ec8b5ce") },
            };

            List<BookEntity> books = new List<BookEntity>
            {
                new BookEntity { Id = Guid.Parse("1cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"), Title = "Book 1" },
                new BookEntity { Id = Guid.Parse("2cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"), Title = "Book 2" },
                new BookEntity { Id = Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"), Title = "Book 3" },
            };

            List<AuthorsBooks> authorsBooks = new List<AuthorsBooks>
            {
                new AuthorsBooks {
                    BookEntityId = Guid.Parse("1cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[0],
                    AuthorEntityId = Guid.Parse("1117baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    AuthorsEntity = authors[0]
                },
                new AuthorsBooks {
                    BookEntityId = Guid.Parse("2cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[1],
                    AuthorEntityId = Guid.Parse("2227baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    AuthorsEntity = authors[1]
                },
                new AuthorsBooks {
                    BookEntityId = Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[2],
                    AuthorEntityId = Guid.Parse("3337baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    AuthorsEntity = authors[2]
                },
                new AuthorsBooks {
                    BookEntityId = Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[2],
                    AuthorEntityId = Guid.Parse("1117baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    AuthorsEntity = authors[0]
                },
            };

            inMemoryContext.Authors.AddRange(authors);
            inMemoryContext.Books.AddRange(books);
            inMemoryContext.AuthorsBooks.AddRange(authorsBooks);

            inMemoryContext.AuthorsBooks.AddRange(authorsBooks);
            inMemoryContext.SaveChanges();
        }

        [Test]
        public async Task Should_Delete_OneRecord_When_SearchingForExistingBookIdWithOneAuthor()
        {
            var initialCount = inMemoryContext!.AuthorsBooks.Count();
            authorsBooksRepository!.DeleteAuthorEntriesForBook(Guid.Parse("1cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"));
            await authorsBooksRepository.SaveAsync();

            var resultCount = inMemoryContext!.AuthorsBooks.Count();

            Assert.AreEqual(initialCount, resultCount + 1);
        }

        [Test]
        public async Task Should_Delete_TwoRecords_When_SearchingForExistingBookIdWithTwoAuthors()
        {
            var initialCount = inMemoryContext!.AuthorsBooks.Count();
            authorsBooksRepository!.DeleteAuthorEntriesForBook(Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"));
            await authorsBooksRepository.SaveAsync();

            var resultCount = inMemoryContext!.AuthorsBooks.Count();

            Assert.AreEqual(initialCount, resultCount + 2);
        }

        [Test]
        public async Task Should_NotDelete_When_SearchingForNonExistingBookId()
        {
            var initialCount = inMemoryContext!.AuthorsBooks.Count();
            authorsBooksRepository!.DeleteAuthorEntriesForBook(Guid.Parse("1243baea-311f-4387-9b9b-ef4c6ec8b5ce"));
            await authorsBooksRepository.SaveAsync();

            var resultCount = inMemoryContext!.AuthorsBooks.Count();

            Assert.AreEqual(initialCount, resultCount);
        }
    }
}
