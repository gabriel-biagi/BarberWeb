
namespace BarberWeb.Domain.Entities
{
    internal class OpeningHours
    {
        public int Id { get; set; }
        public required Professional Professional { get; set; }
        public required DayOfWeek DayOfWeek { get; set; }
        public required TimeOnly StartTime { get; set; }
        public required TimeOnly EndTime { get; set; }
    }
}
