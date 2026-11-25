namespace BarberWeb.Domain.Entities
{
    /// <summary>
    /// Motivo pelo qual um agendamento foi cancelado. Persistido junto ao agendamento
    /// para permitir, no futuro, avisar o cliente sobre o cancelamento e o motivo.
    /// </summary>
    public enum CancellationReason
    {
        ProfessionalDeactivated = 1
    }
}
