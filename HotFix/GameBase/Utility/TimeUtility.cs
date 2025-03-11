using System;
using System.Text;

/// <summary>
/// 时间工具类
/// </summary>
public static class TimeUtility
{
    /// <summary>
    /// 将秒数转换为 HH:mm:ss 格式
    /// </summary>
    public static string SecondToHMS(long seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    /// <summary>
    /// 将秒数转换为 mm:ss 格式
    /// </summary>
    public static string SecondToMS(long seconds)
    {
        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    /// <summary>
    /// 将秒数转换为中文描述格式（x天x小时x分x秒）
    /// </summary>
    public static string SecondToChinese(long seconds)
    {
        if (seconds <= 0) return "0秒";

        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        StringBuilder sb = new StringBuilder();
        
        if (ts.Days > 0)
            sb.Append($"{ts.Days}天");
        if (ts.Hours > 0)
            sb.Append($"{ts.Hours}小时");
        if (ts.Minutes > 0)
            sb.Append($"{ts.Minutes}分");
        if (ts.Seconds > 0)
            sb.Append($"{ts.Seconds}秒");
            
        return sb.ToString();
    }

    /// <summary>
    /// 将秒数转换为简短格式（1d2h3m4s）
    /// </summary>
    public static string SecondToShort(long seconds)
    {
        if (seconds <= 0) return "0s";

        TimeSpan ts = TimeSpan.FromSeconds(seconds);
        StringBuilder sb = new StringBuilder();
        
        if (ts.Days > 0)
            sb.Append($"{ts.Days}d");
        if (ts.Hours > 0)
            sb.Append($"{ts.Hours}h");
        if (ts.Minutes > 0)
            sb.Append($"{ts.Minutes}m");
        if (ts.Seconds > 0)
            sb.Append($"{ts.Seconds}s");
            
        return sb.ToString();
    }

    /// <summary>
    /// 获取当前时间戳（秒）
    /// </summary>
    public static long GetCurrentTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 获取当前时间戳（毫秒）
    /// </summary>
    public static long GetCurrentTimestampMS()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 时间戳转DateTime（秒）
    /// </summary>
    public static DateTime TimestampToDateTime(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
    }

    /// <summary>
    /// DateTime转时间戳（秒）
    /// </summary>
    public static long DateTimeToTimestamp(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }
}