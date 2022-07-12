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
    public class GenresBooksRepositoryTests
    {
        private LibraryDbContext? inMemoryContext;
        private GenresBooksRepository? genresBooksRepository;

        [SetUp]
        public void Init()
        {
            inMemoryContext = InMemoryDbContext.GetInMemoryDbContext("InMemoryDb");
            inMemoryContext.Database.EnsureDeleted();
            genresBooksRepository = new GenresBooksRepository(inMemoryContext);

            List<GenreEntity> genres = new List<GenreEntity>
            {
                new GenreEntity { Name = "Thriller", Id = Guid.Parse("6cd7baea-311f-4387-9b9b-ef4c6ec8b5ce") },
                new GenreEntity { Name = "Sci-Fi", Id = Guid.Parse("7cd7baea-311f-4387-9b9b-ef4c6ec8b5ce") },
                new GenreEntity { Name = "Fantasy", Id = Guid.Parse("8cd7baea-311f-4387-9b9b-ef4c6ec8b5ce") },
            };

            List<BookEntity> books = new List<BookEntity>
            {
                new BookEntity { Id = Guid.Parse("1cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"), Title = "Book 1" },
                new BookEntity { Id = Guid.Parse("2cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"), Title = "Book 2" },
                new BookEntity { Id = Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"), Title = "Book 3" },
            };

            List<GenresBooks> bookGenres = new List<GenresBooks>
            {
                new GenresBooks {
                    BookEntityId = Guid.Parse("1cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[0],
                    GenreEntityId = Guid.Parse("6cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    GenresEntity = genres[0]
                },
                new GenresBooks {
                    BookEntityId = Guid.Parse("2cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[1],
                    GenreEntityId = Guid.Parse("7cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    GenresEntity = genres[1]
                },
                new GenresBooks {
                    BookEntityId = Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[2],
                    GenreEntityId = Guid.Parse("8cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    GenresEntity = genres[2]
                },
                new GenresBooks {
                    BookEntityId = Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    BooksEntity = books[2],
                    GenreEntityId = Guid.Parse("6cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"),
                    GenresEntity = genres[0]
                },
            };

            inMemoryContext.Genres.AddRange(genres);
            inMemoryContext.Books.AddRange(books);
            inMemoryContext.GenresBooks.AddRange(bookGenres);
            inMemoryContext.SaveChanges();
        }

        [Test]
        public async Task Should_Delete_OneRecord_When_SearchingForExistingBookIdWithOneGenre()
        {
            var initialCount = inMemoryContext!.GenresBooks.Count();
            genresBooksRepository!.DeleteGenreEntriesForBook(Guid.Parse("1cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"));
            await genresBooksRepository.SaveAsync();

            var resultCount = inMemoryContext!.GenresBooks.Count();

            Assert.AreEqual(initialCount, resultCount + 1);
        }

        [Test]
        public async Task Should_Delete_TwoRecords_When_SearchingForExistingBookIdWithTwoGenres()
        {
            var initialCount = inMemoryContext!.GenresBooks.Count();
            genresBooksRepository!.DeleteGenreEntriesForBook(Guid.Parse("3cd7baea-311f-4387-9b9b-ef4c6ec8b5ce"));
            await genresBooksRepository.SaveAsync();

            var resultCount = inMemoryContext!.GenresBooks.Count();

            Assert.AreEqual(initialCount, resultCount + 2);
        }

        [Test]
        public async Task Should_NotDelete_When_SearchingForNonExistingBookId()
        {
            var initialCount = inMemoryContext!.GenresBooks.Count();
            genresBooksRepository!.DeleteGenreEntriesForBook(Guid.Parse("1243baea-311f-4387-9b9b-ef4c6ec8b5ce"));
            await genresBooksRepository.SaveAsync();

            var resultCount = inMemoryContext!.GenresBooks.Count();

            Assert.AreEqual(initialCount, resultCount);
        }
    }
}
