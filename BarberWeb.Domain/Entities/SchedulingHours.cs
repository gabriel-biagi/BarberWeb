
using BarberWeb.Domain.Exception;
using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Domain.Entities
{
    public class SchedulingHours
    {
        public int Id { get; set; }

        [Required]
        public Customer Customer { get; private set; }

        [Required]
        public Professional Professional { get; private set; }

        [Required]
        public DateTimeOffset StartDate { get; private set; }

        [Required]
        public DateTimeOffset EndDate { get; private set; }

        [Required]
        public Service Service { get; private set; }

        public SchedulingHours(Customer customer, Professional professional, Service service, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (endDate <= startDate)
            {
                throw new BusinessException("End date cannot be less than start date");
            }

            Customer = customer;
            Professional = professional;
            Service = service;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
