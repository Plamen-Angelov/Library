using DataAccess.Models;
using System.ComponentModel.DataAnnotations;

using static Common.GlobalConstants;

namespace DataAccess.Entities
{
    public class GenreEntity : BaseModel<Guid>
    {
        [Required]
        [MaxLength(GENRE_NAME_MAX_LENGTH)]
        public string Name { get; set; } = string.Empty;

        // Many-to-many - one genre can have many books
        public ICollection<GenresBooks> GenresBooks { get; set; } = new HashSet<GenresBooks>();
    }
}
