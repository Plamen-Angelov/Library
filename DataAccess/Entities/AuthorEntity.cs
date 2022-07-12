﻿using DataAccess.Models;
using System.ComponentModel.DataAnnotations;

using static Common.GlobalConstants;

namespace DataAccess.Entities
{
    public class AuthorEntity : BaseDeletableModel<Guid>
    {
        [Required]
        [MaxLength(AUTHORNAME_MAX_LENGTH)]
        public string AuthorName { get; set; } = string.Empty;

        // Many-to-many - one author can have/write many books
        public ICollection<AuthorsBooks> AuthorsBooks { get; set; } = new HashSet<AuthorsBooks>();
    }
}
