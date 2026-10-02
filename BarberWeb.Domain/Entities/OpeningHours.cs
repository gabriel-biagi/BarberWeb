
using BarberWeb.Domain.Exception;

namespace BarberWeb.Domain.Entities
{
    public class OpeningHours
    {
        public int Id { get; set; }

        public Professional Professional { get; private set; }

        public int ProfessionalId { get; private set; }
        public DayOfWeek DayOfWeek { get; private set; }

        public TimeOnly StartTime { get; private set; }

        public TimeOnly EndTime { get; private set; }

        public OpeningHours(Professional professional, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
        {
            if (endTime <= startTime)
            {
                throw new BusinessException("End date cannot be less than start date");
            }

            ArgumentNullException.ThrowIfNull(professional);

            Professional = professional;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }
    }

}
