using BarberWeb.Application.Common;
using BarberWeb.Application.Common.Interfaces;
using Microsoft.Extensions.Time.Testing;

namespace BarberWeb.Application.Tests.Common;

/// <summary>
/// Testes do <see cref="DateTimeProvider"/> com <see cref="FakeTimeProvider"/>.
/// O fuso da barbearia é fixo no construtor (America/Campo_Grande, UTC-4 sem horário de verão
/// nas datas usadas aqui). Os testes dependem desse fuso existir no sistema operacional.
/// A instância é tipada como <see cref="IDateTimeProvider"/> porque
/// GetUtcRangeForLocalDay é implementado de forma explícita.
/// </summary>
public class DateTimeProviderTests
{
    private static readonly DateOnly LocalDate = new(2026, 11, 30);

    private readonly FakeTimeProvider _fakeTime = new(new DateTimeOffset(2026, 11, 30, 12, 0, 0, TimeSpan.Zero));
    private readonly IDateTimeProvider _provider;

    public DateTimeProviderTests()
    {
        _provider = new DateTimeProvider(_fakeTime);
    }

    [Fact]
    public void GetCurrentInstant_ShouldReturnTimeFromTimeProvider()
    {
        var instant = _provider.GetCurrentInstant();

        Assert.Equal(new DateTimeOffset(2026, 11, 30, 12, 0, 0, TimeSpan.Zero), instant);
    }

    [Fact]
    public void GetCurrentInstant_AfterAdvancingTime_ShouldFollowTimeProvider()
    {
        _fakeTime.Advance(TimeSpan.FromHours(1));

        Assert.Equal(new DateTimeOffset(2026, 11, 30, 13, 0, 0, TimeSpan.Zero), _provider.GetCurrentInstant());
    }

    [Fact]
    public void GetUtcRangeForLocalDay_ShouldReturnUtcBoundariesOfTheLocalDay()
    {
        var (start, end) = _provider.GetUtcRangeForLocalDay(LocalDate);

        // 00:00 local (UTC-4) = 04:00 UTC
        Assert.Equal(new DateTimeOffset(2026, 11, 30, 4, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(new DateTimeOffset(2026, 12, 1, 4, 0, 0, TimeSpan.Zero), end);
        Assert.Equal(TimeSpan.FromHours(24), end - start);
    }

    [Fact]
    public void ConvertLocalToUtc_ShouldApplyBarbershopOffset()
    {
        var utc = _provider.ConvertLocalToUtc(LocalDate, new TimeOnly(8, 0));

        Assert.Equal(new DateTimeOffset(2026, 11, 30, 12, 0, 0, TimeSpan.Zero), utc);
    }

    [Fact]
    public void ConvertLocalToUtc_WhenLocalTimeIsLate_ShouldRollOverToNextUtcDay()
    {
        var utc = _provider.ConvertLocalToUtc(LocalDate, new TimeOnly(21, 0));

        Assert.Equal(new DateTimeOffset(2026, 12, 1, 1, 0, 0, TimeSpan.Zero), utc);
    }

    [Fact]
    public void ConvertUtcToLocalTime_ShouldApplyBarbershopOffset()
    {
        var local = _provider.ConvertUtcToLocalTime(new DateTimeOffset(2026, 11, 30, 12, 0, 0, TimeSpan.Zero));

        Assert.Equal(new TimeOnly(8, 0), local);
    }

    [Fact]
    public void ConvertUtcToLocalTime_WhenUtcIsAlreadyNextDay_ShouldReturnPreviousLocalDayTime()
    {
        // 02:00 UTC de 01/12 ainda é 22:00 do dia 30/11 no horário local.
        var local = _provider.ConvertUtcToLocalTime(new DateTimeOffset(2026, 12, 1, 2, 0, 0, TimeSpan.Zero));

        Assert.Equal(new TimeOnly(22, 0), local);
    }

    [Fact]
    public void UtcInstantAfterUtcMidnight_ShouldStillBelongToPreviousLocalDayRange()
    {
        var instant = new DateTimeOffset(2026, 12, 1, 2, 0, 0, TimeSpan.Zero);

        var (start, end) = _provider.GetUtcRangeForLocalDay(LocalDate);

        Assert.True(instant >= start && instant < end);
    }

    [Fact]
    public void ConvertLocalToUtcAndBack_ShouldReturnOriginalLocalTime()
    {
        var original = new TimeOnly(14, 30);

        var utc = _provider.ConvertLocalToUtc(LocalDate, original);
        var roundTrip = _provider.ConvertUtcToLocalTime(utc);

        Assert.Equal(original, roundTrip);
    }
}
