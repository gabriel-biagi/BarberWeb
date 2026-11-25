

using BarberWeb.Domain.Exception;

namespace BarberWeb.Domain.Entities
{
    public class Professional
    {
        public int Id { get; set; }

        public string Name { get; private set; }

        public string Email { get; private set; }

        public string PhoneNumber { get; private set; }

        /// <summary>
        /// Instante da desativação (exclusão lógica). Nulo enquanto o profissional está ativo.
        /// A desativação é permanente: não existe reativação.
        /// </summary>
        public DateTimeOffset? DeletedAt { get; private set; }

        public bool IsDeleted => DeletedAt.HasValue;

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


        /// <summary>
        /// Desativa o profissional de forma permanente (exclusão lógica) e anonimiza email e telefone,
        /// liberando esses valores, protegidos por índice único, para um novo cadastro.
        /// O nome é mantido para preservar o histórico de agendamentos.
        /// </summary>
        public void Deactivate(DateTimeOffset now)
        {
            if (IsDeleted)
            {
                throw new BusinessException("Professional is already deactivated.");
            }

            if (Id <= 0)
            {
                throw new InvalidOperationException("Only a persisted professional can be deactivated.");
            }

            DeletedAt = now;
            Email = $"deleted-{Id}@removed.invalid"; // TLD .invalid é reservado e nunca resolve
            PhoneNumber = $"000{Id:D10}";            // 13 dígitos; nenhum DDD real começa com 000
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
