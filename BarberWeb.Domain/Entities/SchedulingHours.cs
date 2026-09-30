    
namespace BarberWeb.Domain.Entities
{
    internal class SchedulingHours
    {
        public int Id { get; set; }
        public required Customer Customer { get; set; }
        public required Professional Professional { get; set; }
        public required Service Service { get; set; }
    }
}
