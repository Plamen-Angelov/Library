using API.Controllers;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.ControllersTests
{
    [TestFixture]
    public class BooksControllerTests
    {
        private readonly Mock<IBookService> mockBooksService = new Mock<IBookService>();
        private AddBookDto bookDto = new AddBookDto();
        private BookEntity bookEntity = new BookEntity();
        BooksController? booksController;

        [SetUp]
        public void Init()
        {
            booksController = new BooksController(mockBooksService.Object);
            this.bookDto = new AddBookDto
            {
                BookTitle = "Alice"
            };
            this.bookEntity = new BookEntity
            {
                Title = "Alice"
            };
        }

        [Test]
        public async Task Should_Return_Ok_When_Add_Valid_Book()
        {
            mockBooksService.Setup(x => x.AddBookAsync(It.IsAny<AddBookDto>())).ReturnsAsync(this.bookEntity);

            var result = await booksController!.AddBook(this.bookDto);
            var okResult = result as OkObjectResult;

            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_Return_Exception_When_Add_NotValid_Book()
        {
            mockBooksService.Setup(x => x.AddBookAsync(It.IsAny<AddBookDto>())).ThrowsAsync(new ArgumentException());

            var result = await booksController!.AddBook(default(AddBookDto)!);

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result);
        }

        [Test]
        public async Task Should_ReturnOk_When_UpdatingValidBook()
        {
            mockBooksService.Setup(x => x.UpdateBookAsync(It.IsAny<Guid>(), It.IsAny<AddBookDto>())).ReturnsAsync(new BookOutput());

            var result = await booksController!.UpdateBook(new Guid(), new AddBookDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnBadRequest_When_UpdatingToAlreadyExistingBook()
        {
            mockBooksService.Setup(x => x.UpdateBookAsync(It.IsAny<Guid>(), It.IsAny<AddBookDto>())).ThrowsAsync(new ArgumentException());

            var result = await booksController!.UpdateBook(new Guid(), new AddBookDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), result);
            Assert.AreEqual(400, ((BadRequestObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNotFound_When_UpdatingInvalidBook()
        {
            mockBooksService.Setup(x => x.UpdateBookAsync(It.IsAny<Guid>(), It.IsAny<AddBookDto>())).ThrowsAsync(new NullReferenceException());

            var result = await booksController!.UpdateBook(new Guid(), new AddBookDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            Assert.AreEqual(404, ((NotFoundObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_DeletingExistingBook()
        {
            mockBooksService.Setup(x => x.DeleteBookAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new Guid()));

            var result = await booksController!.DeleteBook(new Guid());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkResult), result);
            Assert.AreEqual(200, ((OkResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNotFound_When_DeletingNonExistingBook()
        {
            mockBooksService.Setup(x => x.DeleteBookAsync(It.IsAny<Guid>())).ThrowsAsync(new NullReferenceException());

            var result = await booksController!.DeleteBook(new Guid());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            Assert.AreEqual(404, ((NotFoundObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_GettingValidBook()
        {
            mockBooksService.Setup(x => x.GetBookByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new BookOutput());

            var result = await booksController!.GetBook(new Guid());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNotFound_When_GettingNonExistingBook()
        {
            mockBooksService.Setup(x => x.GetBookByIdAsync(It.IsAny<Guid>())).ThrowsAsync(new NullReferenceException());

            var result = await booksController!.GetBook(new Guid());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            Assert.AreEqual(404, ((NotFoundObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_GettingValidBooksPaged()
        {
            mockBooksService.Setup(x => x.GetBooksAsync(It.IsAny<PaginatorInputDto>())).ReturnsAsync((new List<BookOutput>(), new int()));

            var result = await booksController!.GetBooks(new PaginatorInputDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNotFound_When_GettingNoBooksPaged()
        {
            mockBooksService.Setup(x => x.GetBooksAsync(It.IsAny<PaginatorInputDto>())).ThrowsAsync(new NullReferenceException());

            var result = await booksController!.GetBooks(new PaginatorInputDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            Assert.AreEqual(404, ((NotFoundObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_GettingValidAllBooks()
        {
            mockBooksService.Setup(x => x.GetAllBooksAsync()).ReturnsAsync(new List<BookOutput>());

            var result = await booksController!.GetAllBooks();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNotFound_When_GettingNoBooks()
        {
            mockBooksService.Setup(x => x.GetAllBooksAsync()).ThrowsAsync(new NullReferenceException());

            var result = await booksController!.GetAllBooks();

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            Assert.AreEqual(404, ((NotFoundObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNumber_When_GettingBooksNumberForGenre()
        {
            mockBooksService.Setup(x => x.GetBooksNumberForGenreAsync(It.IsAny<Guid>())).ReturnsAsync(new int());

            var result = await booksController!.GetGenreBooksNumber(It.IsAny<Guid>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNumber_When_GettingBooksNumberForAuthor()
        {
            mockBooksService.Setup(x => x.GetBooksNumberForAuthorAsync(It.IsAny<Guid>())).ReturnsAsync(new int());

            var result = await booksController!.GetAuthorBooksNumber(It.IsAny<Guid>());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnOk_When_SearchingValidBooksPaged()
        {
            mockBooksService.Setup(x => x.SearchForBooksAsync(It.IsAny<SearchBookDto>(), It.IsAny<PaginatorInputDto>())).ReturnsAsync((new List<BookOutput>(), new int()));

            var result = await booksController!.SearchBooks(new SearchBookDto(), new PaginatorInputDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(OkObjectResult), result);
            Assert.AreEqual(200, ((OkObjectResult)result).StatusCode);
        }

        [Test]
        public async Task Should_ReturnNotFound_When_SearchingNoBooksPaged()
        {
            mockBooksService.Setup(x => x.SearchForBooksAsync(It.IsAny<SearchBookDto>(), It.IsAny<PaginatorInputDto>())).ThrowsAsync(new NullReferenceException());

            var result = await booksController!.SearchBooks(new SearchBookDto(), new PaginatorInputDto());

            Assert.IsNotNull(result);
            Assert.IsInstanceOf(typeof(NotFoundObjectResult), result);
            Assert.AreEqual(404, ((NotFoundObjectResult)result).StatusCode);
        }
    }
}
