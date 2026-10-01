
namespace BarberWeb.Domain.Entities
{
    public class ProfessionalServiceOffering
    {
        public int Id { get; private set; }
        public Professional Professional { get; private set; }
        public int ProfessionalId { get; private set; }
        public Service Service { get; private set; }
        public int ServiceId { get; private set; }

        public decimal Price { get; private set; }
        public TimeSpan Time { get; private set; }

        public ProfessionalServiceOffering(Professional professional, Service service, decimal price, TimeSpan time)
        {
            ArgumentNullException.ThrowIfNull(professional);
            ArgumentNullException.ThrowIfNull(service);
            if (CheckInvalidPrice(price))
            {
                throw new ArgumentException("Price must be between 0.01 and 9999.99");
            }

            Professional = professional;
            ProfessionalId = professional.Id;
            Service = service;
            ServiceId = service.Id;
            Price = price;
            Time = time;
        }

        private bool CheckInvalidPrice(decimal price)
        {
            return (price < 0.01m || price > 9999.99m);
        }
    }
}
