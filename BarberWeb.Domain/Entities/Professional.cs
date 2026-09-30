
using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Domain.Entities
{
    internal class Professional
    {
        public int Id { get; set; }

        [StringLength(maximumLength: 60, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 60 characters.")]
        public required string Name { get; set; }
        [StringLength(maximumLength: 60, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 100 characters.")]
        public required string Email { get; set; }
        [StringLength(maximumLength: 15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 characters.")]
        public required string PhoneNumber { get; set; }

    }
}
