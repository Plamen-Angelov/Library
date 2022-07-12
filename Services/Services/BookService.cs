using log4net;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;
using Repositories.Interfaces;
using Repositories.Mappers;
using Services.Interfaces;
using static Common.ExceptionMessages;

namespace Services.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository bookRepository;
        private readonly IAuthorRepository authorRepository;
        private readonly IGenreRepository genreRepository;
        private readonly IAuthorsBooksRepository authorsBooksRepository;
        private readonly IGenresBooksRepository genresBooksRepository;
        private readonly IBlobService blobService;
        private readonly ILog log = LogManager.GetLogger(typeof(BookService));
        
        public BookService(IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IGenreRepository genreRepository,
            IAuthorsBooksRepository authorsBooksRepository,
            IGenresBooksRepository genresBooksRepository,
            IBlobService blobService
            )
        {
            this.bookRepository = bookRepository;
            this.authorRepository = authorRepository;
            this.genreRepository = genreRepository;
            this.authorsBooksRepository = authorsBooksRepository;
            this.genresBooksRepository = genresBooksRepository;
            this.blobService = blobService;
        }

        public async Task<BookEntity> AddBookAsync(AddBookDto book)
        {
            book.BookTitle = book.BookTitle.Trim()!;
            book.Description = book.Description?.Trim();
            book.Availability = true;

            var authorsEntities = new List<AuthorEntity>();
            var genresEntitties = new List<GenreEntity>();

            var existBook = this.bookRepository.FindBookByTitle(book);

            if (existBook != null)
            {
                log.Error($"Add method throws exception {BOOKTITLE_FOUND}");
                throw new ArgumentException(BOOKTITLE_FOUND);
            }

            if (book.BookAuthors == null)
            {
                log.Error($"Add method throws exception {NO_AUTHORS_FOUND}");
                throw new ArgumentException(NO_AUTHORS_FOUND);
            }

            if (book.Genres == null)
            {
                log.Error($"Add method throws exception {NO_GENRES_FOUND}");
                throw new ArgumentException(NO_GENRES_FOUND);
            }

            if (book.TotalQuantity <= 0)
            {
                log.Error($"Add method throws exception {BOOK_QUANTITY_IS_INVALID}");
                throw new ArgumentException(BOOK_QUANTITY_IS_INVALID);
            }

            foreach (var author in book.BookAuthors)
            {
                var existAuthor = this.authorRepository.FindAuthorByName(author);
                authorsEntities.Add(existAuthor);
            }

            foreach (var genre in book.Genres)
            {
                var existGenre = this.genreRepository.FindGenreByName(genre);
                genresEntitties.Add(existGenre);
            }

            string? url = default(string);

            if (book.BookCover?.Length > 0)
            {
                url = await blobService.UploadBlobFileAsync(book.BookCover!, book.BookTitle);
            }

            var bookEntity = Mapper.ToBookEntity(book, authorsEntities, genresEntitties, url);

            await this.bookRepository.InsertAsync(bookEntity);
            await this.bookRepository.SaveAsync();

            return bookEntity;
        }

        public async Task<BookOutput> UpdateBookAsync(Guid bookId, AddBookDto book)
        {
            var existingEntity = await this.bookRepository.GetByIdAsync(bookId);

            if (existingEntity == null)
            {
                log.Error($"UpdateBook method throws exception {BOOK_NOT_FOUND}");
                throw new NullReferenceException(BOOK_NOT_FOUND);
            }

            book.BookTitle = book.BookTitle.Trim()!;
            book.Description = book.Description?.Trim();

            var authorsEntities = new List<AuthorEntity>();
            var genresEntitties = new List<GenreEntity>();

            if (await this.bookRepository.ContainsBookName(bookId, book.BookTitle))
            {
                log.Error($"UpdateBook method throws exception {BOOK_EXISTS}");
                throw new ArgumentException(BOOK_EXISTS);
            }

            if (book.BookAuthors == null)
            {
                log.Error($"UpdateBook method throws exception {NO_AUTHORS_FOUND}");
                throw new ArgumentException(NO_AUTHORS_FOUND);
            }

            if (book.Genres!.Count == 0)
            {
                log.Error($"UpdateBook method throws exception {NO_GENRES_FOUND}");
                throw new ArgumentException(NO_GENRES_FOUND);
            }

            this.authorsBooksRepository.DeleteAuthorEntriesForBook(bookId);
            this.genresBooksRepository.DeleteGenreEntriesForBook(bookId);

            foreach (var author in book.BookAuthors)
            {
                var existAuthor = this.authorRepository.FindAuthorByName(author);
                authorsEntities.Add(existAuthor);
            }

            foreach (var genre in book.Genres)
            {
                var existGenre = this.genreRepository.FindGenreByName(genre);
                genresEntitties.Add(existGenre);
            }

            string? url = default(string);

            if (book.DeleteCover == true && existingEntity.ImageAddress != null)
            {
                await blobService.RemoveBlobFileAsync(existingEntity.ImageAddress!);
            }
            else if (book.BookCover?.Length > 0 && existingEntity.ImageAddress != null)
            {
                url = await blobService.UpdateBlobFileAsync(book.BookCover!, existingEntity.ImageAddress!, book.BookTitle);
            }
            else if (book.BookCover?.Length > 0 && existingEntity.ImageAddress == null)
            {
                url = await blobService.UploadBlobFileAsync(book.BookCover!, book.BookTitle);
            }
            else if ((book.BookTitle != existingEntity.Title) && existingEntity.ImageAddress?.Length > 0)
            {
                url = await blobService.RenameBlobFileAsync(existingEntity.ImageAddress!, book.BookTitle);
            }
            else
            {
                url = existingEntity.ImageAddress;
            }

            var inputBookEntity = Mapper.ToBookEntity(book, authorsEntities, genresEntitties, url);

            existingEntity.Title = inputBookEntity.Title;
            existingEntity.Description = inputBookEntity.Description;
            existingEntity.AuthorsBooks = inputBookEntity.AuthorsBooks;
            existingEntity.GenresBooks = inputBookEntity.GenresBooks;
            existingEntity.IsAvailable = inputBookEntity.IsAvailable;
            existingEntity.ImageAddress = inputBookEntity.ImageAddress;

            var diffTotalQuantityAfterUpdate = inputBookEntity.TotalQuantity - existingEntity.TotalQuantity;
            var numberBorrowedBooks = existingEntity.TotalQuantity - existingEntity.CurrentQuantity;

            if (inputBookEntity.TotalQuantity < 0)
            {
                log.Error($"UpdateBook method throws exception {BOOK_QUANTITY_IS_LESS_THAN_ZERO}");
                throw new ArgumentException(BOOK_QUANTITY_IS_LESS_THAN_ZERO);
            }
            else if (inputBookEntity.TotalQuantity < numberBorrowedBooks)
            {
                log.Error($"UpdateBook method throws exception {String.Format(BOOK_QUANTITY_IS_LESS_THAN_BORROWEDBOOKS, numberBorrowedBooks)}");
                throw new ArgumentException(String.Format(BOOK_QUANTITY_IS_LESS_THAN_BORROWEDBOOKS, numberBorrowedBooks));
            }
            else
            {
                existingEntity.TotalQuantity += diffTotalQuantityAfterUpdate;
                existingEntity.CurrentQuantity += diffTotalQuantityAfterUpdate;
            }

            await this.bookRepository.UpdateAsync(existingEntity);
            await this.bookRepository.SaveAsync();

            var result = Mapper.ToBookOutput(existingEntity);

            var authors = this.authorRepository.FindAuthorsByBookId(result.Id);
            result.AllAuthors = String.Join(", ", authors);

            var genres = this.genreRepository.FindGenresByBookId(result.Id);
            result.AllGenres = String.Join(", ", genres);

            return result;
        }

        public async Task DeleteBookAsync(Guid bookId)
        {
            var existingEntity = await this.bookRepository.GetByIdAsync(bookId);

            if (existingEntity == null)
            {
                log.Error($"DeleteBook method throws exception {BOOK_NOT_FOUND}");
                throw new NullReferenceException(BOOK_NOT_FOUND);
            }

            if (existingEntity.ImageAddress != null)
            {
                await blobService.RemoveBlobFileAsync(existingEntity.ImageAddress!);
            }

            await this.bookRepository.DeleteAsync(bookId);
            await this.bookRepository.SaveAsync();
        }

        public async Task<(List<BookOutput>, int)> GetBooksAsync(PaginatorInputDto input)
        {
            var (entities, totalCount) = await this.bookRepository.GetEntityPageAsync(input);

            if (totalCount == 0)
            {
                log.Error($"DeleteBook method throws exception {NO_BOOKS_FOUND}");
                throw new NullReferenceException(NO_BOOKS_FOUND);
            }

            var result = new List<BookOutput>();

            entities.ForEach(entity => result.Add(Mapper.ToBookOutput(entity)));

            return (result, totalCount);
        }

        public async Task<List<BookOutput>> GetAllBooksAsync()
        {
            var result = await this.bookRepository.GetAllBooksAsync();

            if (result.Count == 0)
            {
                log.Error($"GetAllBooks method throws exception {NO_BOOKS_FOUND}");
                throw new NullReferenceException(NO_BOOKS_FOUND);
            }

            foreach (var book in result)
            {
                var authors = this.authorRepository.FindAuthorsByBookId(book.Id);
                book.AllAuthors = String.Join(", ", authors);

                var genres = this.genreRepository.FindGenresByBookId(book.Id);
                book.AllGenres = String.Join(", ", genres);
            }

            return result;
        }
        public async Task<BookOutput> GetBookByIdAsync(Guid bookId)
        {
            var existingEntity = await this.bookRepository.GetByIdAsync(bookId);

            if (existingEntity == null)
            {
                log.Error($"GetBookById method throws exception {BOOK_NOT_FOUND}");
                throw new NullReferenceException(BOOK_NOT_FOUND);
            }

            var result = Mapper.ToBookOutput(existingEntity);

            var authors = this.authorRepository.FindAuthorsByBookId(result.Id);
            result.AllAuthors = String.Join(", ", authors);

            var genres = this.genreRepository.FindGenresByBookId(result.Id);
            result.AllGenres = String.Join(", ", genres);

            return result;
        }

        public async Task<int> GetBooksNumberForGenreAsync(Guid genreId)
        {
            var result = await this.bookRepository.GetBooksNumberForGenre(genreId);

            return result;
        }

        public async Task<int> GetBooksNumberForAuthorAsync(Guid authorId)
        {
            var result = await this.bookRepository.GetBooksNumberForAuthor(authorId);

            return result;
        }

        public async Task<int> GetCountOfAllBooksAsync()
        {
            var allBooks = await this.bookRepository.GetCountOfAllBooksAsync();

            return allBooks;
        }

        public async Task<(List<LastBooksOutput>, int)> GetBooksForLastTwoWeeksAsync(PaginatorInputDto pagination)
        {
            var (lastBooksOutput, lastBooksCount) = await this.bookRepository.GetBooksForLastTwoWeeksAsync(pagination);

            foreach (var book in lastBooksOutput)
            {
                var authors = this.authorRepository.FindAuthorsByBookId(book.Id);
                book.AllAuthors = String.Join(", ", authors);
            }

            return (lastBooksOutput, lastBooksCount);
        }

        public async Task<(List<BookOutput>, int)> SearchForBooksAsync(SearchBookDto input, PaginatorInputDto pagination)
        {
            var authors = new List<Guid>();
            var genres = new List<Guid>();

            if (input.Author != null)
            {
                var authorsResult = await this.authorRepository.FindMultipleAuthorsByNameAsync(input.Author);
                authors.AddRange(authorsResult);
            }

            if (input.Genre != null)
            {
                var genresResult = await this.genreRepository.FindMultipleGenresByNameAsync(input.Genre);
                genres.AddRange(genresResult);
            }

            var (result, totalCount) = await this.bookRepository.SearchForBooksAsync(input, authors, genres, pagination);

            if (totalCount == 0)
            {
                throw new NullReferenceException(NO_BOOKS_FOUND);
            }

            foreach (var book in result)
            {
                var allAuthors = this.authorRepository.FindAuthorsByBookId(book.Id);
                book.AllAuthors = String.Join(", ", allAuthors);

                var allGenres = this.genreRepository.FindGenresByBookId(book.Id);
                book.AllGenres = String.Join(", ", allGenres);
            }

            return (result, totalCount);
        }
    }
}
