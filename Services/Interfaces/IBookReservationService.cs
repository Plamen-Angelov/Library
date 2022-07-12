using Common.Models.InputDTOs;
using Common.Models.OutputDtos;
using DataAccess.Entities;

namespace Services.Interfaces
{
    public interface IBookReservationService
    {
        Task<BookReservationEntity> AddBookReservationAsync(BookReservationDto input);
        Task<(List<BookReservationOutput>, int)> GetBooksReservationsAsync(PaginatorInputDto input);
        Task<BookConfirmReservationOutput> GetBookReservationByIdAsync(Guid bookId);
        Task RejectBookReservationByIdAsync(BookReservationMessageDto input);
        Task ApproveBookReservation(BookReservationMessageDto input);
    }
}
