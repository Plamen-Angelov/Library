using DataAccess.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static Common.GlobalConstants;

namespace DataAccess.Entities
{
    public class NotificationEntity : BaseModel<Guid>
    {
        [Required]
        [MaxLength(NOTIFICATION_MESSAGE_MAX_LENGTH)]
        public string Message { get; set; } = string.Empty;

        [ForeignKey(nameof(UserEntity))]
        public Guid UserEntityId { get; set; }

        [ForeignKey(nameof(UserEntity))]
        public Guid LibrarianId { get; set; }
    }
}
