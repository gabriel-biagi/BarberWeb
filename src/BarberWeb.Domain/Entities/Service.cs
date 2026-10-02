
namespace BarberWeb.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Service(string name, string description)
        {
            if (CheckInvalidName(name))
            {
                throw new ArgumentException("Name must be between 3 and 60 characters.");
            }

            if (CheckInvalidDescription(description))
            {
                throw new ArgumentException("Description must be between 5 and 200 characters.");
            }

            Name = name;
            Description = description;

        }

        private bool CheckInvalidName(string field)
        {
            return (string.IsNullOrWhiteSpace(field) || field.Length < 3 || field.Length > 60);
        }

        private bool CheckInvalidDescription(string desc)
        {
            return (string.IsNullOrWhiteSpace(desc) || desc.Length < 5 || desc.Length > 200);
        }
    }
}
