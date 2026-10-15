

using BarberWeb.Domain.Exception;

namespace BarberWeb.Domain.Entities
{
    public class Professional
    {
        public int Id { get; set; }

        public string Name { get; private set; }

        public string Email { get; private set; }

        public string PhoneNumber { get; private set; }

        public Professional(string name, string email, string phoneNumber)
        {
            if (CheckInvalidName(name))
            {
                throw new InvalidInputException("Name must be between 3 and 60 characters.");
            }

            if (CheckInvalidEmail(email))
            {
                throw new InvalidInputException("Email must be between 5 and 60 characters.");
            }

            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            if (CheckInvalidPhoneNumber(normalizedPhone))
            {
                throw new InvalidInputException("Phone Number must be between 10 and 15 digits.");
            }
            Name = name;
            Email = email;
            PhoneNumber = normalizedPhone;
        }

        
        private bool CheckInvalidName(string field)
        {
            return (string.IsNullOrWhiteSpace(field) || field.Length < 3 || field.Length > 60);
        }

        private bool CheckInvalidEmail(string email)
        {
            return (string.IsNullOrWhiteSpace(email) || email.Length < 5 || email.Length > 60);
        }

        private bool CheckInvalidPhoneNumber(string phoneNumber)
        {
            return (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10 || phoneNumber.Length > 15);
        }
        
        private static string NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return string.Empty;
            }

            return new string(phoneNumber.Where(char.IsAsciiDigit).ToArray());
        }
    }
}
