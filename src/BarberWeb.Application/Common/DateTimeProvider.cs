using BarberWeb.Application.Common.Interfaces;

namespace BarberWeb.Application.Common;

public class DateTimeProvider : IDateTimeProvider
{
    private readonly TimeProvider _timeProvider;
    private readonly TimeZoneInfo _barbershopTimeZone;

    public DateTimeProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        
        try
        {
            _barbershopTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Campo_Grande");
        }
        catch (TimeZoneNotFoundException)
        {
            // No Windows, usar o ID correspondente do sistema Windows
            _barbershopTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Brazilian Standard Time");
        }
    }

    public DateTimeOffset GetCurrentInstant()
    {
        return _timeProvider.GetUtcNow();
    }

    (DateTimeOffset Start, DateTimeOffset End) IDateTimeProvider.GetUtcRangeForLocalDay(DateOnly localDate)
    {

        var localStart = localDate.ToDateTime(TimeOnly.MinValue);
        
        var nextLocalDate = localDate.AddDays(1);
        var localEnd = nextLocalDate.ToDateTime(TimeOnly.MinValue);
        
        var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, _barbershopTimeZone);
        var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, _barbershopTimeZone);

        return (utcStart, utcEnd);
    }

    public DateTimeOffset ConvertLocalToUtc(DateOnly localDate, TimeOnly localTime)
    {
        var localDateTime = localDate.ToDateTime(localTime);
        
        var localDto = new DateTimeOffset(localDateTime, _barbershopTimeZone.GetUtcOffset(localDateTime));
        return localDto.ToUniversalTime();
    }

    public TimeOnly ConvertUtcToLocalTime(DateTimeOffset utcInstant)
    {
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcInstant.UtcDateTime, _barbershopTimeZone);
        return TimeOnly.FromDateTime(localDateTime);
    }
}