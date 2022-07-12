using Common;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;
using Moq;
using NUnit.Framework;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.ServicesTests
{
    [TestFixture]
    public class GenreServiceTests
    {
        private Mock<IGenreRepository> mockGenreRepository = new Mock<IGenreRepository>();
        private Mock<IBookService> mockBookService = new Mock<IBookService>();
        private GenreService? genreService;
        private GenreEntity? existingGenreEntity;
        private GenreEntity? modifiedGenreEntity;
        private Genre? inputGenre;

        [SetUp]
        public void Init()
        {
            genreService = new GenreService(mockGenreRepository.Object, mockBookService.Object);

            this.inputGenre = new Genre
            {
                Name = "Thriller"
            };

            this.modifiedGenreEntity = new GenreEntity
            {
                Name = "Thriller"
            };

            this.existingGenreEntity = new GenreEntity
            {
                Name = "Sci-Fi"
            };
        }

        [Test]
        public async Task Should_BeNotNull_When_AddingValidGenre()
        {
            mockGenreRepository.Setup(x => x.ContainsGenreName(It.IsAny<string>())).Returns(false);
            mockGenreRepository.Setup(x => x.InsertAsync(It.IsAny<GenreEntity>())).ReturnsAsync(new GenreEntity());

            var result = await genreService!.AddGenreAsync(new Genre());

            Assert.IsNotNull(result);
        }

        [Test]
        public void Should_ThrowArgumentException_When_AddingExistingGenre()
        {
            mockGenreRepository.Setup(x => x.ContainsGenreName(It.IsAny<string>())).Returns(true);
            mockGenreRepository.Setup(x => x.InsertAsync(It.IsAny<GenreEntity>())).ReturnsAsync(new GenreEntity());

            var result = Assert.ThrowsAsync<ArgumentException>(async () => await genreService!.AddGenreAsync(new Genre()));
            Assert.AreEqual(ExceptionMessages.GENRE_EXISTS, result!.Message);
        }

        [Test]
        public async Task Should_BeNotNull_When_UpdatingValidGenre()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(this.existingGenreEntity);
            mockGenreRepository.Setup(x => x.ContainsGenreName(It.IsAny<string>())).Returns(false);
            mockGenreRepository.Setup(x => x.UpdateAsync(this.existingGenreEntity!)).ReturnsAsync(this.modifiedGenreEntity!);

            var result = await genreService!.UpdateGenreAsync(new Guid(), this.inputGenre!);

            Assert.IsNotNull(result);
            Assert.AreEqual(this.inputGenre!.Name, result.Name);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_UpdatingInvalidGenre()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(default(GenreEntity));
            mockGenreRepository.Setup(x => x.ContainsGenreName(It.IsAny<string>())).Returns(false);
            mockGenreRepository.Setup(x => x.UpdateAsync(this.existingGenreEntity!)).ReturnsAsync(this.modifiedGenreEntity!);

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await genreService!.UpdateGenreAsync(new Guid(), this.inputGenre!));
            Assert.AreEqual(ExceptionMessages.GENRE_NOT_FOUND, result!.Message);
        }

        [Test]
        public void Should_ThrowArgumentException_When_UpdatingToExistingGenreName()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new GenreEntity());
            mockGenreRepository.Setup(x => x.ContainsGenreName(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(true);
            mockGenreRepository.Setup(x => x.UpdateAsync(It.IsAny<GenreEntity>())).ReturnsAsync(new GenreEntity());

            var result = Assert.ThrowsAsync<ArgumentException>(async () => await genreService!.UpdateGenreAsync(new Guid(), this.inputGenre!));
            Assert.AreEqual(ExceptionMessages.GENRE_EXISTS, result!.Message);
        }

        [Test]
        public void Should_Complete_When_DeletingExistingGenre()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new GenreEntity());
            mockBookService.Setup(x => x.GetBooksNumberForGenreAsync(It.IsAny<Guid>())).ReturnsAsync(0);

            var result = genreService!.DeleteGenreAsync(new Guid());

            Assert.That(result.IsCompleted, Is.True);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_DeletingNonExistingGenre()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(default(GenreEntity));

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await genreService!.DeleteGenreAsync(new Guid()));
            Assert.AreEqual(ExceptionMessages.GENRE_NOT_FOUND, result!.Message);
        }

        [Test]
        public void Should_ThrowArgumentException_When_DeletingAssignedGenre()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new GenreEntity());
            mockBookService.Setup(x => x.GetBooksNumberForGenreAsync(It.IsAny<Guid>())).ReturnsAsync(1);

            var result = Assert.ThrowsAsync<ArgumentException>(async () => await genreService!.DeleteGenreAsync(new Guid()));
            Assert.AreEqual(ExceptionMessages.GENRE_HAS_BOOKS, result!.Message);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_NoGenresFound()
        {
            mockGenreRepository.Setup(x => x.GetAllGenresAsync()).ReturnsAsync(new List<GenreOutput>());

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await genreService!.GetAllGenresAsync());
            Assert.AreEqual(ExceptionMessages.NO_GENRES_FOUND, result!.Message);
        }

        [Test]
        public async Task Should_Complete_When_FoundGenres()
        {
            mockGenreRepository.Setup(x => x.GetAllGenresAsync()).ReturnsAsync(new List<GenreOutput> { new GenreOutput() });

            var result = await genreService!.GetAllGenresAsync();

            Assert.IsNotNull(result);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_GenreNotFound()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(default(GenreEntity));

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await genreService!.GetGenreByIdAsync(new Guid()));
            Assert.AreEqual(ExceptionMessages.GENRE_NOT_FOUND, result!.Message);
        }

        [Test]
        public async Task Should_Complete_When_FoundBook()
        {
            mockGenreRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new GenreEntity());

            var result = await genreService!.GetGenreByIdAsync(new Guid());
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task Should_Return_GenresAndCount_When_GettingGenresPaged()
        {
            mockGenreRepository.Setup(x => x.GetEntityPageAsync(It.IsAny<PaginatorInputDto>())).ReturnsAsync((new List<GenreEntity> { this.existingGenreEntity! }, 1));

            var (resultList, resultCount) = await genreService!.GetGenresAsync(new PaginatorInputDto());
            Assert.IsNotNull(resultList);
            Assert.IsNotNull(resultCount);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_NoGenresFoundPaged()
        {
            mockGenreRepository.Setup(x => x.GetEntityPageAsync(It.IsAny<PaginatorInputDto>()))!.ReturnsAsync((default(List<GenreEntity>), default(int)));

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await genreService!.GetGenresAsync(new PaginatorInputDto()));
            Assert.AreEqual(ExceptionMessages.NO_GENRES_FOUND, result!.Message);
        }

        [Test]
        public async Task GetCountOfAllGenresAsync_ShouldReturnCorrectNumberOfAllGenres()
        {
            // Arrange
            int count = 3;
            mockGenreRepository.Setup(x => x.GetCountOfAllGenresAsync()).ReturnsAsync(count);

            // Act
            var result = await genreService!.GetCountOfAllGenresAsync();

            // Assert
            Assert.AreEqual(count, result);
        }

        [Test]
        public async Task Should_Return_GenresAndCount_When_SearchingGenresPaged()
        {
            var genreOutput = new GenreOutput
            {
                Name = "My genre",
                Id = Guid.Parse("453629f8-71c9-457f-a460-433fe9ed14ee")
            };

            mockGenreRepository.Setup(x => x.SearchForGenresAsync(It.IsAny<SearchGenreDto>(), It.IsAny<PaginatorInputDto>())).ReturnsAsync((new List<GenreOutput> { genreOutput }, 1));

            var (resultList, resultCount) = await genreService!.SearchForGenresAsync(new SearchGenreDto(), new PaginatorInputDto());
            Assert.IsNotNull(resultList);
            Assert.IsNotNull(resultCount);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_SearchNoAuthorsFoundPaged()
        {
            var searchDto = new SearchGenreDto
            {
                Name = "Genre name"
            };

            mockGenreRepository.Setup(x => x.SearchForGenresAsync(It.IsAny<SearchGenreDto>(), It.IsAny<PaginatorInputDto>()))!.ReturnsAsync((default(List<GenreOutput>), default(int)));

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await genreService!.SearchForGenresAsync(searchDto, new PaginatorInputDto()));
            Assert.AreEqual(ExceptionMessages.NO_GENRES_FOUND, result!.Message);
        }
    }
}
