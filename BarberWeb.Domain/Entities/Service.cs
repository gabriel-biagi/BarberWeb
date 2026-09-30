
using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 60, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 60 characters.")]
        public string Name { get; private set; }

        [Required]
        [StringLength(maximumLength: 200, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 200 characters.")]
        public string Description { get; private set; }

        [Required]
        [Range(0.01, 9999.99, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; private set; }

        public Service(string name, string description, decimal price)
        {
            if (CheckValidName(name))
            {
                throw new ArgumentException("Name must be between 3 and 60 characters.");
            }

            if (CheckValidDescription(description))
            {
                throw new ArgumentException("Description must be between 5 and 200 characters.");
            }

            if (CheckValidPrice(price))
            {
                throw new ArgumentException("Price must be between 0.01 and 9999.99");
            }

            Name = name;
            Description = description;
            Price = price;
        }

        private bool CheckValidName(string field)
        {
            return (string.IsNullOrWhiteSpace(field) || field.Length < 3 || field.Length > 60);
        }

        private bool CheckValidDescription(string desc)
        {
            return (string.IsNullOrWhiteSpace(desc) || desc.Length < 5 || desc.Length > 200);
        }

        private bool CheckValidPrice(decimal price)
        {
            return (price < 0.01m || price > 9999.99m);
        }
    }
}
