using System.Globalization;

namespace DnsResolverTiming;

public static class Extensions
{
    public static string GetResolveTimeAsString(this List<DnsResolveResult> results, ResultType resultType, ResultValue resultValue)
    {
        var value = results.GetResolveTime(resultType, resultValue);
        return value.HasValue ? $"{Math.Round(value.Value, 3)}ms" : "";
    }

    public static void AddIfNotNull(this List<Markup> list, Markup? value)
    {
        if (value is not null)
        {
            list.Add(value);
        }
    }

    public static string GetDnsServer(this ClientX client)
    {
        switch (client.EndpointConfiguration.RequestFormat)
        {
            case DnsRequestFormat.DnsOverUDP:
                return client.EndpointConfiguration.Hostname;
            case DnsRequestFormat.DnsOverHttps:
                return client.EndpointConfiguration.BaseUri.ToString();
            case DnsRequestFormat.DnsOverTLS:
                return client.EndpointConfiguration.Hostname;
            default:
                return "(unknown)";
        }
    }

    public static string GetDnsRequestFormat(this ClientX client)
    {
        switch (client.EndpointConfiguration.RequestFormat)
        {
            case DnsRequestFormat.DnsOverUDP:
                return "UDP";
            case DnsRequestFormat.DnsOverTCP:
                return "TCP";
            case DnsRequestFormat.DnsOverHttps:
            case DnsRequestFormat.DnsOverHttpsJSON:
            case DnsRequestFormat.DnsOverHttpsPOST:
                return "DoH";
            case DnsRequestFormat.DnsOverTLS:
                return "DoT";
            default:
                return "(unknown)";
        }
    }

    public static string CsvFormat(this double? value)
    {
        if (!value.HasValue)
        {
            return "";
        }
        
        return Math.Round(value.Value, 3).ToString(CultureInfo.InvariantCulture);
    }
    
    private static double? GetResolveTime(this List<DnsResolveResult> results, ResultType resultType, ResultValue resultValue)
    {
        switch (resultType)
        {
            case ResultType.Minimum:
                return results.Min(r => r.GetResolveTime(resultValue));
            case ResultType.Maximum:
                return results.Max(r => r.GetResolveTime(resultValue));
            case ResultType.Average:
                return results.Average(r => r.GetResolveTime(resultValue));
            case ResultType.Median:
                return results.Select(r => r.GetResolveTime(resultValue)).Median();
            default:
                throw new InvalidEnumArgumentException(nameof(resultType), (int)resultType, typeof(ResultType));
        }
    }

    private static double? Median(this IEnumerable<double?> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        
        var data = source.Where(n => n.HasValue).Select(n => n!.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
        {
            return null;
        }
        
        if (data.Length % 2 == 0)
        {
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        }
        
        return data[data.Length / 2];
    }
}