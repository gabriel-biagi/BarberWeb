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
/// Os bugs de laço infinito (TimeOnly.Add dando a volta à meia-noite e duração de 24h) foram
/// corrigidos; os testes de regressão no fim do arquivo usam Timeout para não travar a suíte
/// caso o problema volte. A consulta por sobreposição de agendamentos que atravessam a
/// meia-noite está em EfSchedulingHoursRepository e não é coberta aqui (exige banco).
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

    // ---------- Regressões dos bugs corrigidos (laço infinito e virada da meia-noite) ----------
    // Timeout: se o laço voltar a não terminar, o teste falha em vez de travar a suíte.

    [Fact(Timeout = 5000)]
    public async Task WhenDurationWouldCrossMidnight_ShouldNotGenerateSlotOutsideTheBlock()
    {
        // Antes: 20:00 + 4h = 00:00 (TimeOnly dá a volta) e 00:00 <= 23:00, laço infinito.
        GivenOpeningHours(("20:00", "23:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromHours(4));

        var result = await WhenSearching();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact(Timeout = 5000)]
    public async Task WhenBlockEndsLateInTheDay_ShouldStopAtTheEndOfTheBlock()
    {
        // Antes: 23:00 + 1h = 00:00 <= 23:59, o laço não parava.
        GivenOpeningHours(("22:00", "23:59"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromHours(1));

        var result = await WhenSearching();

        var slot = Assert.Single(result.Value);
        Assert.Equal(new TimeOnly(22, 0), slot.Start);
        Assert.Equal(new TimeOnly(23, 0), slot.End);
    }

    [Fact(Timeout = 5000)]
    public async Task WhenSlotEndsExactlyAtTheEndOfTheBlock_ShouldIncludeIt()
    {
        GivenOpeningHours(("21:00", "23:30"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(50));

        var result = await WhenSearching();

        Assert.Equal(new[] { "21:00", "21:50", "22:40" }, Starts(result.Value));
        Assert.Equal(new TimeOnly(23, 30), result.Value.Last().End);
    }

    [Theory(Timeout = 5000)]
    [InlineData(24 * 60)]   // exatamente 24h
    [InlineData(25 * 60)]   // mais de 24h
    [InlineData(0)]         // zero (o domínio impede, mas o dado pode vir do banco)
    [InlineData(-30)]       // negativa
    public async Task WithInvalidServiceDuration_ShouldReturnBusinessRuleFailure(int minutes)
    {
        GivenOpeningHours(("09:00", "10:00"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromMinutes(minutes));

        var result = await WhenSearching();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
    }

    [Fact(Timeout = 5000)]
    public async Task WithDurationJustBelow24Hours_ShouldNotFailButFitsNoRegularBlock()
    {
        GivenOpeningHours(("00:00", "23:59"));
        GivenAppointments();
        GivenOffering(TimeSpan.FromHours(24) - TimeSpan.FromMinutes(1));

        var result = await WhenSearching();

        var slot = Assert.Single(result.Value);
        Assert.Equal(new TimeOnly(0, 0), slot.Start);
        Assert.Equal(new TimeOnly(23, 59), slot.End);
    }
}
