namespace BarberWeb.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTimeOffset GetCurrentInstant();
    (DateTimeOffset Start, DateTimeOffset End) GetUtcRangeForLocalDay(DateOnly localDate);
    DateTimeOffset ConvertLocalToUtc(DateOnly localDate, TimeOnly localTime);
    TimeOnly ConvertUtcToLocalTime(DateTimeOffset utcInstant);
}