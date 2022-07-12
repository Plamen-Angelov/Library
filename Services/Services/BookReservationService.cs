using Microsoft.AspNetCore.Identity;
using log4net;
using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;
using Repositories.Interfaces;
using Repositories.Mappers;
using Services.EmailSender;
using Services.Interfaces;
using static Common.ExceptionMessages;

namespace Services.Services
{
    public class BookReservationService : IBookReservationService
    {
        private readonly IBookReservationRepository bookReservationRepository;
        private readonly IBookRepository bookRepository;
        private readonly IUserRepository userRepository;
        private readonly IMailSender mailSender;
        private readonly UserManager<UserEntity> userManager;
        private readonly ILog log = LogManager.GetLogger(typeof(BookReservationService));

        public BookReservationService(
            IBookReservationRepository bookReservationRepository, 
            IBookRepository bookRepository,
            IUserRepository userRepository,
            UserManager<UserEntity> userManager,
            IMailSender mailSender
            )
        {
            this.bookReservationRepository = bookReservationRepository;
            this.bookRepository = bookRepository;
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.mailSender = mailSender;
        }

        public async Task<BookReservationEntity> AddBookReservationAsync(BookReservationDto input)
        {
            var userIdParsedToString = input.UserId.ToString();

            var existingUserEntity = await userRepository.GetByIdAsync(userIdParsedToString);

            if (existingUserEntity == null)
            {
                log.Error($"Add method throws exception {USER_NOT_FOUND}");
                throw new ArgumentException(USER_NOT_FOUND);
            }

            var existingBookEntity = await bookRepository.GetByIdAsync(input.BookId);

            if (existingBookEntity == null)
            {
                log.Error($"Add method throws exception {BOOK_NOT_FOUND}");
                throw new ArgumentException(BOOK_NOT_FOUND);
            }

            if (!existingBookEntity.IsAvailable)
            {
                log.Error($"Add method throws exception {BOOK_IS_NOT_AVAILABLE}");
                throw new ArgumentException(BOOK_IS_NOT_AVAILABLE);
            }

            var currentQuantity = existingBookEntity.CurrentQuantity;

            if (currentQuantity <= 0)
            {
                log.Error($"Add method throws exception {BOOK_QUANTITY_IS_NULL}");
                throw new ArgumentException(BOOK_QUANTITY_IS_NULL);
            }

            var bookReservationEntity = new BookReservationEntity
            {
                UserEntityId = input.UserId,
                BookEntityId = input.BookId,
            };

            await bookReservationRepository.InsertAsync(bookReservationEntity);
            await bookReservationRepository.SaveAsync();

            return bookReservationEntity;
        }

        public async Task<(List<BookReservationOutput>, int)> GetBooksReservationsAsync(PaginatorInputDto input)
        {
            var result = new List<BookReservationOutput>();
            var (entities, totalCount) = await bookReservationRepository.GetBookReservationPageAsync(input);

            if (totalCount == 0 || entities.All(x=>x.IsReviewed))
            {
                log.Error($"GetBooksReservations method throws exception {ALL_BOOK_RESERVATIONS_ARE_REVIEWED}");
                throw new NullReferenceException(ALL_BOOK_RESERVATIONS_ARE_REVIEWED);
            }

            TimeZoneInfo sofiaTime = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");

            foreach (var entity in entities)
            {
                if (!entity.IsReviewed)
                {
                    var mappedEntity = Mapper.ToBookReservationResult(entity);
                    var currentBook = bookRepository.GetByIdAsync(mappedEntity.BookId).Result;
                    var currentUser = userRepository.GetByIdAsync((mappedEntity.UserId).ToString()).Result;

                    var createdOnLocal = TimeZoneInfo.ConvertTimeFromUtc(entity.CreatedOn, sofiaTime);

                    result.Add(new BookReservationOutput
                    {
                        Id = mappedEntity.Id,
                        BookTitle = currentBook!.Title,
                        UserName = String.Concat(currentUser!.FirstName," ",currentUser.LastName),
                        Email = currentUser.Email,
                        IsApproved = entity.IsApproved,
                        CreatedOn = createdOnLocal.ToString("dd/MM/yyyy HH:mm:ss")
                    });
                }
            }
            return (result, totalCount);
        }

        public async Task<BookConfirmReservationOutput> GetBookReservationByIdAsync(Guid bookReservationId)
        {
            var existingRequest = await this.bookReservationRepository.GetByIdAsync(bookReservationId);
            TimeZoneInfo sofiaTime = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");

            if (existingRequest == null)
            {
                log.Error($"GetBookReservationById method throws exception {BOOKRESERVATION_NOT_FOUND}");
                throw new NullReferenceException(BOOKRESERVATION_NOT_FOUND);
            }

            if (existingRequest.IsReviewed)
            {
                log.Error($"GetBookReservationById method throws exception {BOOKRESERVATION_WAS_REVIEWED}");
                throw new NullReferenceException(BOOKRESERVATION_WAS_REVIEWED);
            }

            var result = Mapper.ToBookReservationResult(existingRequest);
            var currentBook = bookRepository.GetByIdAsync(result.BookId).Result;
            var currentUser = userRepository.GetByIdAsync((result.UserId).ToString()).Result;

            var createdOnLocal = TimeZoneInfo.ConvertTimeFromUtc(existingRequest.CreatedOn, sofiaTime);

            var output = new BookConfirmReservationOutput
            {
                BookTitle = currentBook!.Title,
                UserName = String.Concat(currentUser!.FirstName," ",currentUser.LastName),
                Quantity = currentBook.CurrentQuantity,
                IsAvailable = currentBook.IsAvailable,
                CreatedRequestDate = createdOnLocal.ToString("dd/MM/yyyy HH:mm:ss"),
                Message = String.Empty
            };

            return output;
        }

        public async Task RejectBookReservationByIdAsync(BookReservationMessageDto input)
        {
            var existingRequest = await this.bookReservationRepository.GetByIdAsync(input.bookReservationId);

            if (existingRequest == null)
            {
                log.Error($"RejectBookReservation method throws exception {BOOKRESERVATION_NOT_FOUND}");
                throw new NullReferenceException(BOOKRESERVATION_NOT_FOUND);
            }

            if (existingRequest.IsReviewed)
            {
                log.Error($"RejectBookReservation method throws exception {BOOKRESERVATION_WAS_REVIEWED}");
                throw new NullReferenceException(BOOKRESERVATION_WAS_REVIEWED);
            }

            var existingLibrarian = await userManager.FindByIdAsync(input.librarianId.ToString());

            if (existingLibrarian == null)
            {
                log.Error($"RejectBookReservation method throws exception {LIBRARIAN_NOT_FOUND}");
                throw new NullReferenceException(LIBRARIAN_NOT_FOUND);
            }

            if (existingRequest.UserEntityId == input.librarianId)
            {
                log.Error($"RejectBookReservation method throws exception {LIBRARIAN_SELFCONFIRMATION_REJECTIONERROR}");
                throw new InvalidOperationException(LIBRARIAN_SELFCONFIRMATION_REJECTIONERROR);
            }

            var userToString = existingRequest!.UserEntityId.ToString();
            var user = await userRepository.GetByIdAsync(userToString);
            var email = user!.Email;

            existingRequest.LibrarianId = input.librarianId;
            existingRequest.IsReviewed = true;
            await bookReservationRepository.UpdateAsync(existingRequest);
            await bookReservationRepository.SaveAsync();

            await mailSender.SendEmailAsync(email, "Rejected book reservation request","", $"<p>{ input.Message }</p>");

            log.Info("Email is sent successfully.");
        }

        public async Task ApproveBookReservation(BookReservationMessageDto input)
        {
            var existingBookReservation = await bookReservationRepository.GetByIdAsync(input.bookReservationId);

            if (existingBookReservation == null)
            {
                log.Error($"RejectBookReservation method throws exception {BOOK_RESERVATION_NOT_EXISTS}");
                throw new NullReferenceException(BOOK_RESERVATION_NOT_EXISTS);
            }

            var book = await bookRepository.GetByIdAsync(existingBookReservation.BookEntityId);

            if (book == null)
            {
                log.Error($"RejectBookReservation method throws exception {BOOK_NOT_FOUND}");
                throw new NullReferenceException(BOOK_NOT_FOUND);
            }

            if (book.CurrentQuantity == 0 || book.IsAvailable == false)
            {
                log.Error($"RejectBookReservation method throws exception {BOOK_IS_NOT_AVAILABLE}");
                throw new ArgumentNullException(BOOK_IS_NOT_AVAILABLE);
            }

            var user = await userRepository.GetByIdAsync(existingBookReservation.UserEntityId.ToString());

            if (user == null)
            {
                log.Error($"RejectBookReservation method throws exception {USER_NOT_FOUND}");
                throw new ArgumentNullException(USER_NOT_FOUND);
            }

            var librarian = await userManager.FindByIdAsync(input.librarianId.ToString());

            if (librarian == null)
            {
                log.Error($"RejectBookReservation method throws exception {LIBRARIAN_NOT_FOUND}");
                throw new ArgumentNullException(LIBRARIAN_NOT_FOUND);
            }

            if (user!.Id == input.librarianId.ToString())
            {
                log.Error($"RejectBookReservation method throws exception {LIBRARIAN_SELFCONFIRMATION_ERROR}");
                throw new InvalidOperationException(LIBRARIAN_SELFCONFIRMATION_ERROR);
            }

            existingBookReservation.IsApproved = true;
            existingBookReservation.IsReviewed = true;
            existingBookReservation.LibrarianId = input.librarianId;

            book!.CurrentQuantity -= 1;

            if (book.CurrentQuantity == 0)
            {
                book.IsAvailable = false;
            }

            await bookRepository.UpdateAsync(book);

            await bookReservationRepository.SaveAsync();

            await mailSender.SendEmailAsync(user!.Email, "Book reservation approved", input.Message!, $"<p> {input.Message} <p>");

            log.Info("Email is sent successfully.");
        }
    }
}
