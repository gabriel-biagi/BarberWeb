


namespace BarberWeb.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; private set; }

        public string Email { get; private set; }

        public string PhoneNumber { get; private set; }

        public List<SchedulingHours> SchedulingHours { get; private set; } = new List<SchedulingHours>();


        public Customer(string name, string email, string phoneNumber)
        {
            if (CheckInvalidName(name))
            {
                throw new ArgumentException("Name must be between 3 and 60 characters.");
            }

            if (CheckInvalidEmail(email))
            {
                throw new ArgumentException("Email must be between 5 and 60 characters.");
            }

            if (CheckInvalidPhoneNumber(phoneNumber))
            {
                throw new ArgumentException("Phone Number must be between 10 and 15 characters.");
            }

            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        private bool CheckInvalidName(string field)
        {
            return (string.IsNullOrWhiteSpace(field) || field.Length < 3 || field.Length > 60) ;
        }

        private bool CheckInvalidEmail(string email)
        {
            return (string.IsNullOrWhiteSpace(email) || email.Length < 5 || email.Length > 60);
        }

        private bool CheckInvalidPhoneNumber(string phoneNumber)
        {
            return (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 10 || phoneNumber.Length > 15);
        }
    }
}
