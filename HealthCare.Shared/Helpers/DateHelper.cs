namespace HealthCare.Shared.Helpers;

public static class DateHelper
{
    public static DateTime Now()     => DateTime.UtcNow;
    public static DateTime Today()   => DateTime.UtcNow.Date;
    public static DateTime? NullableNow() => DateTime.UtcNow;
}