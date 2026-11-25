using BarberWeb.Application.Common;
using BarberWeb.Application.Common.Interfaces;
using BarberWeb.Application.Services;
using BarberWeb.Application.Services.Interfaces;
using BarberWeb.Application.Tests.Builders;
using BarberWeb.Domain.Entities;
using Moq;

namespace BarberWeb.Application.Tests.Services;

/// <summary>
/// Testes de <see cref="AvailableHoursService"/> com todas as dependências mockadas,
/// inclusive <see cref="IDateTimeProvider"/>. O mock simula o fuso UTC-4 da barbearia,
/// então estes testes validam a lógica de slots, e não a conversão de fuso
/// (isso é coberto em DateTimeProviderTests).
///
/// Bugs conhecidos documentados abaixo (testes com Skip, sem correção por decisão do projeto):
///  1. TimeOnly.Add dá a volta à meia-noite: gera slot fora do bloco e, em casos como bloco 20:00-23:00
///     com serviço de 4h, o laço nunca termina (confirmado: o teste travou ao remover o Skip).
///  2. Duração de 24 h causa laço infinito (por isso o teste NUNCA deve ser executado sem a correção).
///  3. EfSchedulingHoursRepository filtra apenas por StartDate dentro do dia, então agendamentos
///     que começam no dia anterior e atravessam a meia-noite não são considerados.
///     Não há teste porque exige banco de dados (não existe projeto de testes de infraestrutura).
/// </summary>
public class AvailableHoursServiceTests
{
    private static readonly DateOnly Date = new(2026, 11, 30); // segunda-feira
    private static readonly TimeSpan BarbershopOffset = TimeSpan.FromHours(-4);
    private static readonly DateTimeOffset UtcStart = new(2026, 11, 30, 4, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UtcEnd = new(2026, 12, 1, 4, 0, 0, TimeSpan.Zero);

    private readonly Mock<IOpeningHoursService> _openingHours = new();
    private readonly Mock<ISchedulingHoursService> _schedulingHours = new();
    private readonly Mock<IProfessionalServiceOfferingService> _offerings = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();

    private readonly Professional _professional = TestEntities.Professional();
    private readonly Service _service = TestEntities.Service();
    private readonly AvailableHoursService _sut;

    public AvailableHoursServiceTests()
    {
        _dateTimeProvider
            .Setup(p => p.GetUtcRangeForLocalDay(Date))
            .Returns((UtcStart, UtcEnd));

        _dateTimeProvider
            .Setup(p => p.ConvertLocalToUtc(It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
            .Returns((DateOnly d, TimeOnly t) => ToUtc(d, t));

        _sut = new AvailableHoursService(
            _openingHours.Object,
            _schedulingHours.Object,
            _offerings.Object,
            _dateTimeProvider.Object);
    }

    private static DateTimeOffset ToUtc(DateOnly date, TimeOnly time)
        => new DateTimeOffset(date.ToDateTime(time), BarbershopOffset).ToUniversalTime();

    private static DateTimeOffset ToUtc(string time) => ToUtc(Date, TimeOnly.Parse(time));

    private void GivenOpeningHours(params (string Start, string End)[] blocks)
    {
        var hours = blocks
            .Select(b => TestEntities.OpeningHours(_professional, DayOfWeek.Monday, b.Start, b.End))
            .ToList();

        _openingHours
            .Setup(s => s.GetOpeningHoursOfTheProfessionalByDay(_professional.Id, DayOfWeek.Monday))
            .ReturnsAsync(hours);
    }

    private void GivenOffering(TimeSpan? duration)
    {
        _offerings
            .Setup(s => s.GetTimeSpanByServiceIdAndProfessionalId(_service.Id, _professional.Id))
            .ReturnsAsync(duration);
    }

    private void GivenAppointments(params (string Start, string End)[] appointments)
    {
        var customer = TestEntities.Customer();
        var offering = TestEntities.Offering(_professional, _service, TimeSpan.FromMinutes(30));

        var scheduled = appointments
            .Select(a => TestEntities.Scheduling(customer, offering, ToUtc(a.Start), ToUtc(a.End)))
            .ToList();

        _schedulingHours
            .Setup(s => s.GetSchedulingHoursOfTheProfessionalByDay(_professional.Id, UtcStart, UtcEnd))
            .ReturnsAsync(scheduled);
    }

    private Task<Result<IEnumerable<BarberWeb.Application.DTOs.AvailableSlotDto>>> WhenSearching()
        => _sut.GetAvailableSlotsByProfessionalAndDateAsync(_professional.Id, Date, _service.Id);

    private static string[] Starts(IEnumerable<BarberWeb.Application.DTOs.AvailableSlotDto> slots)
        => slots.Select(s => s.Start.ToString("HH:mm")).ToArray();

    // ---------- Fluxo básico ----------

    [Fact]
    public async Task WithoutOpeningHours_ShouldReturnEmptyList()
    {
        GivenOpeningHours();
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(30));

        var result = await WhenSearching();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task WithoutOfferingDuration_ShouldReturnBusinessRuleFailure()
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(null);

        var result = await WhenSearching();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
    }

    [Fact]
    public async Task ShouldQueryDependenciesWithDayOfWeekAndUtcRangeOfTheLocalDay()
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(30));

        await WhenSearching();

        _dateTimeProvider.Verify(p => p.GetUtcRangeForLocalDay(Date), Times.Once);
        _openingHours.Verify(s => s.GetOpeningHoursOfTheProfessionalByDay(_professional.Id, DayOfWeek.Monday), Times.Once);
        _schedulingHours.Verify(s => s.GetSchedulingHoursOfTheProfessionalByDay(_professional.Id, UtcStart, UtcEnd), Times.Once);
    }

    [Fact]
    public async Task WhenDurationFitsExactly_ShouldGenerateConsecutiveSlotsWithProfessionalName()
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(30));

        var result = await WhenSearching();

        var slots = result.Value.ToList();
        Assert.Equal(new[] { "09:00", "09:30" }, Starts(slots));
        Assert.Equal(new TimeOnly(9, 30), slots[0].End);
        Assert.Equal(new TimeOnly(10, 0), slots[1].End);
        Assert.All(slots, s => Assert.Equal(_professional.Name, s.ProfessionalName));
    }

    [Fact]
    public async Task WhenRemainderIsShorterThanDuration_ShouldNotGeneratePartialSlot()
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(40));

        var result = await WhenSearching();

        var slot = Assert.Single(result.Value);
        Assert.Equal(new TimeOnly(9, 0), slot.Start);
        Assert.Equal(new TimeOnly(9, 40), slot.End);
    }

    [Fact]
    public async Task WhenDurationIsLongerThanBlock_ShouldReturnEmptyList()
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromHours(2));

        var result = await WhenSearching();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task WithMultipleOpeningBlocks_ShouldGenerateSlotsForEachBlock()
    {
        GivenOpeningHours(("09:00", "10:00"), ("14:00", "15:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(30));

        var result = await WhenSearching();

        Assert.Equal(new[] { "09:00", "09:30", "14:00", "14:30" }, Starts(result.Value));
    }

    // ---------- Conflitos com agendamentos existentes (bloco 09:00-10:00, slots de 30 min) ----------

    [Theory]
    [InlineData("09:15", "09:45", new string[0])]                     // sobreposição parcial nos dois slots
    [InlineData("09:00", "09:15", new[] { "09:30" })]                 // contido no primeiro slot
    [InlineData("08:00", "11:00", new string[0])]                     // contém todos os slots
    [InlineData("09:00", "09:30", new[] { "09:30" })]                 // adjacente: termina quando o próximo slot começa
    [InlineData("09:30", "10:00", new[] { "09:00" })]                 // adjacente: começa quando o slot anterior termina
    [InlineData("10:00", "10:30", new[] { "09:00", "09:30" })]        // depois do bloco, sem conflito
    [InlineData("08:00", "09:00", new[] { "09:00", "09:30" })]        // antes do bloco, sem conflito
    public async Task ShouldExcludeOnlySlotsOverlappingExistingAppointments(
        string appointmentStart,
        string appointmentEnd,
        string[] expectedStarts)
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments((appointmentStart, appointmentEnd));
        GivenOffering(TimeSpan.FromMinutes(30));

        var result = await WhenSearching();

        Assert.Equal(expectedStarts, Starts(result.Value));
    }

    // ---------- Bugs conhecidos (documentados, não corrigidos) ----------

    [Fact(Skip = "KnownBug #1: TimeOnly.Add dá a volta à meia-noite. Em um bloco 20:00-23:00 com serviço de 4h, " +
                 "20:00 + 4h = 00:00 e 00:00 <= 23:00, gerando um slot fora do bloco; como 00:00, 04:00, ..., 20:00 " +
                 "também passam na condição, o laço NUNCA termina (confirmado: a execução travou). " +
                 "NÃO remova o Skip antes de corrigir (comparar em TimeSpan, dentro do bloco).")]
    [Trait("Category", "KnownBug")]
    public async Task KnownBug_DurationCrossingMidnight_ShouldNotGenerateSlotOutsideTheBlock()
    {
        GivenOpeningHours(("20:00", "23:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromHours(4));

        var result = await WhenSearching();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact(Skip = "KnownBug #2: duração de 24h causa laço infinito (currentStart.Add(24h) volta ao mesmo horário). " +
                 "NÃO remova o Skip antes de implementar a validação 'duração < 24h' no serviço, " +
                 "ou a execução travará e consumirá memória.")]
    [Trait("Category", "KnownBug")]
    public async Task KnownBug_DurationOf24Hours_ShouldReturnBusinessRuleFailure()
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromHours(24));

        var result = await WhenSearching();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
    }
}
