using BarberWeb.Domain.Entities;

namespace BarberWeb.Application.Tests.Builders;

/// <summary>
/// Cria entidades de domínio já ligadas entre si.
/// Os construtores do domínio exigem objetos relacionados (por exemplo, um agendamento
/// precisa de cliente e oferta, e a oferta precisa de profissional e serviço), então
/// centralizar a montagem evita repetição nos testes.
/// </summary>
public static class TestEntities
{
    public static Professional Professional(int id = 1, string name = "Carlos Silva")
        => new(name, $"prof{id}@email.com", $"1197777{id:D4}") { Id = id };

    public static Service Service(int id = 1, string name = "Corte")
        => new(name, "Corte de cabelo masculino") { Id = id };

    public static Customer Customer(int id = 1, string name = "Maria Souza")
        => new(name, $"cliente{id}@email.com", $"1196666{id:D4}") { Id = id };

    public static ProfessionalServiceOffering Offering(
        Professional professional,
        Service service,
        TimeSpan duration,
        decimal price = 50m)
        => new(professional, service, price, duration);

    public static OpeningHours OpeningHours(
        Professional professional,
        DayOfWeek dayOfWeek,
        string start,
        string end)
        => new(professional, dayOfWeek, TimeOnly.Parse(start), TimeOnly.Parse(end));

    public static SchedulingHours Scheduling(
        Customer customer,
        ProfessionalServiceOffering offering,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc)
        => new(customer, offering, startUtc, endUtc);
}
