
using BarberWeb.Domain.Exception;
using System.ComponentModel.DataAnnotations;

namespace BarberWeb.Domain.Entities
{
    public class OpeningHours
    {
        public int Id { get; set; }

        [Required]
        public Professional Professional { get; private set; }

        [Required]
        public DayOfWeek DayOfWeek { get; private set; }

        [Required]
        public TimeOnly StartTime { get; private set; }

        [Required]
        public TimeOnly EndTime { get; private set; }

        public OpeningHours(Professional professional, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        {
            if (endTime <= startTime)
            {
                throw new BusinessException("End date cannot be less than start date");
            }

            Professional = professional;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }
    }

}
