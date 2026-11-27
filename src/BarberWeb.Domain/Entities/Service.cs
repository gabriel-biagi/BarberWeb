
using BarberWeb.Domain.Exception;

namespace BarberWeb.Domain.Entities
{
    public class Service
    {
        public const int NameMinLength = 3;
        public const int NameMaxLength = 60;
        public const int DescriptionMinLength = 5;
        public const int DescriptionMaxLength = 100;

        public int Id { get; set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public Service(string name, string description)
        {
            Validate(name, description);

            Name = name;
            Description = description;
        }

        /// <summary>
        /// Substitui nome e descrição, aplicando as mesmas regras da criação.
        /// Se algum valor for inválido, nada é alterado.
        /// </summary>
        public void Update(string name, string description)
        {
            Validate(name, description);

            Name = name;
            Description = description;
        }

        private static void Validate(string name, string description)
        {
            if (CheckInvalidName(name))
            {
                throw new InvalidInputException($"Name must be between {NameMinLength} and {NameMaxLength} characters.");
            }

            if (CheckInvalidDescription(description))
            {
                throw new InvalidInputException($"Description must be between {DescriptionMinLength} and {DescriptionMaxLength} characters.");
            }
        }

        private static bool CheckInvalidName(string field)
        {
            return (string.IsNullOrWhiteSpace(field) || field.Length < NameMinLength || field.Length > NameMaxLength);
        }

        private static bool CheckInvalidDescription(string desc)
        {
            return (string.IsNullOrWhiteSpace(desc) || desc.Length < DescriptionMinLength || desc.Length > DescriptionMaxLength);
        }
    }
}
