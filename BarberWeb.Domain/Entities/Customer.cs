
using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 60, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 60 characters.")]
        public string Name { get; private set; }

        [Required]
        [StringLength(maximumLength: 60, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 60 characters.")]
        public string Email { get; private set; }

        [Required]
        [StringLength(maximumLength: 15, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 15 characters.")]
        public string PhoneNumber { get; private set; }


        public Customer(string name, string email, string phoneNumber)
        {
            if (CheckValidName(name))
            {
                throw new ArgumentException("Name must be between 3 and 60 characters.");
            }

            if (CheckValidEmail(email))
            {
                throw new ArgumentException("Email must be between 5 and 60 characters.");
            }

            if (CheckValidPhoneNumber(phoneNumber))
            {
                throw new ArgumentException("Phone Number must be between 10 and 15 characters.");
            }

            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        private bool CheckValidName(string field)
        {
            return (string.IsNullOrWhiteSpace(field) || field.Length < 3 || field.Length > 60) ;
        }

        private bool CheckValidEmail(string email)
        {
            return (string.IsNullOrWhiteSpace(email) || email.Length < 5 || email.Length > 60);
        }

        private bool CheckValidPhoneNumber(string phoneNumber)
        {
            return (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10 || phoneNumber.Length > 15);
        }
    }
}
