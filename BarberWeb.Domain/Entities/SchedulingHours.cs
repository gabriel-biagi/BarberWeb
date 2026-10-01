
using BarberWeb.Domain.Exception;

namespace BarberWeb.Domain.Entities
{
    public class SchedulingHours
    {
        public int Id { get; set; }

        public Customer Customer { get; private set; }
        public int CustomerId { get; private set; }

        public DateTimeOffset StartDate { get; private set; }

        public DateTimeOffset EndDate { get; private set; }

        public ProfessionalServiceOffering ProfessionalServiceOffering { get; private set; }
        public int ProfessionalServiceOfferingId { get; private set; }

        public int ProfessionalId { get; private set; } // EF Core não permite índice único com navegação; necessário para constraint em OnModelCreating

        public SchedulingHours(Customer customer, ProfessionalServiceOffering professionalServiceOffering, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (endDate <= startDate)
            {
                throw new BusinessException("End date cannot be less than start date");
            }

            ArgumentNullException.ThrowIfNull(customer);
            ArgumentNullException.ThrowIfNull(professionalServiceOffering);

            Customer = customer;
            ProfessionalServiceOffering = professionalServiceOffering;
            StartDate = startDate;
            EndDate = endDate;

            CustomerId = customer.Id;
            ProfessionalServiceOfferingId = professionalServiceOffering.Id;
            ProfessionalId = professionalServiceOffering.ProfessionalId;
        }
    }
}
