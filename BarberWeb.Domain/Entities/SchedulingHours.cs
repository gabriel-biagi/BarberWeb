
using BarberWeb.Domain.Exception;

namespace BarberWeb.Domain.Entities
{
    public class SchedulingHours
    {
        public int Id { get; set; }

        public Customer Customer { get; private set; }

        public Professional Professional { get; private set; }

        public DateTimeOffset StartDate { get; private set; }

        public DateTimeOffset EndDate { get; private set; }

        public Service Service { get; private set; }

        public SchedulingHours(Customer customer, Professional professional, Service service, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (endDate <= startDate)
            {
                throw new BusinessException("End date cannot be less than start date");
            }

            ArgumentNullException.ThrowIfNull(customer);
            ArgumentNullException.ThrowIfNull(professional);
            ArgumentNullException.ThrowIfNull(service);

            Customer = customer;
            Professional = professional;
            Service = service;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
