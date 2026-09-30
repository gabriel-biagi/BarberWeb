
using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Domain.Entities
{
    internal class Service
    {
        public int Id { get; set; }

        [StringLength(maximumLength: 60, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 60 characters.")]
        public required string Name { get; set; }

        [StringLength(maximumLength: 200, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 200 characters.")]
        public required string Description { get; set; }

        [Range(0.01, 9999.99, ErrorMessage = "Price must be greater than zero.")]
        public required decimal Price { get; set; }
    }
}
