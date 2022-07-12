using Common;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
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
    public class AuthorServiceTests
    {
        Mock<IAuthorRepository> mockAuthorRepo = new Mock<IAuthorRepository>();
        Mock<IBookService> mockBookService = new Mock<IBookService>();
        IAuthorService authorService;
        AuthorDto input;
        private readonly ILogger<AuthorService> logger;

        [SetUp]
        public void SetUp()
        {
            authorService = new AuthorService(mockAuthorRepo.Object, mockBookService.Object, logger);

            input = new AuthorDto();
        }

        [Test]
        public async Task AddAuthorShouldAddTheInput()
        {
            mockAuthorRepo.Setup(x => x.InsertAsync(It.IsAny<AuthorEntity>())).ReturnsAsync(new AuthorEntity());

            AuthorOutput output = await authorService.AddAuthorAsync(input);

            Assert.IsNotNull(output);
            Assert.IsInstanceOf<AuthorOutput>(output);
        }

        [Test]
        public async Task AddAuthorThroesExceptionWhenInputAuthorExists()
        {
            mockAuthorRepo.Setup(x => x.FindAuthorByName(It.IsAny<string>())).Returns(new AuthorEntity());

            Assert.ThrowsAsync<ArgumentException>(async Task () => await authorService.AddAuthorAsync(input), "Author already exists");
        }

        [Test]
        public async Task GetAllWorksCorrectly()
        {
            mockAuthorRepo.Setup(x => x.GetAllAuthorsAsync()).ReturnsAsync(new List<AuthorOutput>() { new AuthorOutput()});

            List<AuthorOutput> output = await authorService.GetAllAuthorsAsync();

            Assert.IsNotNull(output);
            Assert.IsInstanceOf<List<AuthorOutput>>(output);
        }

        [Test]
        public async Task GetAllThrowsExceptionWhenNoAuthoursAreFound()
        {
            mockAuthorRepo.Setup(x => x.GetAllAuthorsAsync()).ReturnsAsync(new List<AuthorOutput>());

            Assert.ThrowsAsync<ArgumentException>(async Task () => await authorService.GetAllAuthorsAsync(), "Author not found");
        }

        [Test]
        public async Task GetAuthorsWorksCorrectly()
        {
            List<AuthorEntity> outputList = new List<AuthorEntity> { new AuthorEntity() };
            mockAuthorRepo.Setup(x => x.GetEntityPageAsync(It.IsAny<PaginatorInputDto>())).ReturnsAsync((outputList, 25));

            (List<AuthorOutput> output, int count) = await authorService.GetAuthorsAsync(new PaginatorInputDto());

            Assert.IsNotNull(output);
            Assert.IsNotNull(default(int));
        }

        [Test]
        public void GetAuthorsThrowsExceptionWhenNoAuthoursAreFound()
        {
            mockAuthorRepo.Setup(x => x.GetEntityPageAsync(It.IsAny<PaginatorInputDto>()))!.ReturnsAsync((default(List<AuthorEntity>), 25));

            Assert.ThrowsAsync<NullReferenceException>(async Task () => await authorService.GetAuthorsAsync(new PaginatorInputDto()), "No authors found.");
        }

        [Test]
        public async Task GetAuthorByIdWorksCorrectly()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new AuthorEntity());

            AuthorOutput output = await authorService.GetAuthorByIdAsync(new Guid());

            Assert.IsNotNull(output);
            Assert.IsInstanceOf<AuthorOutput>(output);
        }

        [Test]
        public async Task GetAuthorByIdThrowsExceptionWhenNoAuthoursAreFound()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(default(AuthorEntity));

            Assert.ThrowsAsync<ArgumentException>(async Task () => await authorService.GetAuthorByIdAsync(new Guid()), "Author not found");
        }

        [Test]
        public async Task UpdateAuthorWorksCorrectly()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new AuthorEntity());
            mockAuthorRepo.Setup(x => x.UpdateAsync(It.IsAny<AuthorEntity>())).ReturnsAsync(new AuthorEntity());

            AuthorOutput output = await authorService.UpdateAuthorAsync(input, new Guid());

            Assert.IsNotNull(output);
            Assert.IsInstanceOf<AuthorOutput>(output);
        }

        [Test]
        public void UpdateAuthorThrowsExceptionWhenAuthorIsNotFound()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(default(AuthorEntity));

            Assert.ThrowsAsync<ArgumentNullException>(async Task () => await authorService.UpdateAuthorAsync(input, new Guid()), "Author not found");
        }

        [Test]
        public void DeleteAuthorWorksCorrectly()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new AuthorEntity());
            mockBookService.Setup(x => x.GetBooksNumberForAuthorAsync(It.IsAny<Guid>())).ReturnsAsync(0);

            Assert.That(authorService.DeleteAuthorAsync(new Guid()).IsCompleted);
        }

        [Test]
        public void DeleteAuthorThrowsExceptionWhenAuthorIsNotFound()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(default(AuthorEntity));

            var result = Assert.ThrowsAsync<NullReferenceException>(async Task () => await authorService.DeleteAuthorAsync(new Guid()));
            Assert.AreEqual(ExceptionMessages.AUTHOR_NOT_FOUND, result!.Message);
        }

        [Test]
        public void DeleteAuthorThrowsExceptionWhenAuthorIsAssignedToBooks()
        {
            mockAuthorRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new AuthorEntity());
            mockBookService.Setup(x => x.GetBooksNumberForAuthorAsync(It.IsAny<Guid>())).ReturnsAsync(1);

            var result = Assert.ThrowsAsync<ArgumentException>(async Task () => await authorService.DeleteAuthorAsync(new Guid()));
            Assert.AreEqual(ExceptionMessages.AUTHOR_HAS_BOOKS, result!.Message);
        }

        [Test]
        public async Task Should_Return_AuthorsAndCount_When_SearchingAuthorsPaged()
        {
            var authorOutput = new AuthorOutput
            {
                AuthorName = "My author",
                Id = Guid.Parse("453629f8-71c9-457f-a460-433fe9ed14ee")
            };

            mockAuthorRepo.Setup(x => x.SearchForAuthorsAsync(It.IsAny<SearchAuthorDto>(), It.IsAny<PaginatorInputDto>())).ReturnsAsync((new List<AuthorOutput> { authorOutput }, 1));

            var (resultList, resultCount) = await authorService!.SearchForAuthorsAsync(new SearchAuthorDto(), new PaginatorInputDto());
            Assert.IsNotNull(resultList);
            Assert.IsNotNull(resultCount);
        }

        [Test]
        public void Should_ThrowNullReferenceException_When_SearchNoAuthorsFoundPaged()
        {
            var searchDto = new SearchAuthorDto
            {
                AuthorName = "Author name"
            };

            mockAuthorRepo.Setup(x => x.SearchForAuthorsAsync(It.IsAny<SearchAuthorDto>(), It.IsAny<PaginatorInputDto>()))!.ReturnsAsync((default(List<AuthorOutput>), default(int)));

            var result = Assert.ThrowsAsync<NullReferenceException>(async () => await authorService!.SearchForAuthorsAsync(searchDto, new PaginatorInputDto()));
            Assert.AreEqual(ExceptionMessages.NO_AUTHORS_FOUND, result!.Message);
        }
    }
}
