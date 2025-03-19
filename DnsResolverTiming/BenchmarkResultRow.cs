namespace DnsResolverTiming;

public class BenchmarkResultRow
{
    public string Domain { get; init; } = string.Empty;

    public List<DnsResolveResult> Cloudflare { get; private set; } = new();

    public List<DnsResolveResult> Quad9Malware { get; private set; } = new();
    
    public List<DnsResolveResult> ControlDMalware { get; private set; } = new();
    
    public List<DnsResolveResult> ControlDAdsTracking { get; private set; } = new();
    
    public List<DnsResolveResult> ControlDFamily { get; private set; } = new();
    
    public List<DnsResolveResult> Dns0Zero { get; private set; } = new();

    public List<DnsResolveResult> Dns0Kids { get; private set; } = new();

    public void SetResult(DnsServer dns, IEnumerable<DnsResolveResult> results)
    {
        if (dns.Name == DnsServer.CloudflareDns.Name)
            Cloudflare = [..results];
        else if (dns.Name == DnsServer.Quad9MalwareDns.Name)
            Quad9Malware = [..results];
        else if (dns.Name == DnsServer.ControlDMalwareDns.Name)
            ControlDMalware = [..results];
        else if (dns.Name == DnsServer.ControlDAdsTrackingDns.Name)
            ControlDAdsTracking = [..results];
        else if (dns.Name == DnsServer.ControlDFamilyDns.Name)
            ControlDFamily = [..results];
        else if (dns.Name == DnsServer.Dns0ZeroDns.Name)
            Dns0Zero = [..results];
        else if (dns.Name == DnsServer.Dns0KidsDns.Name)
            Dns0Kids = [..results];
        else
            throw new ArgumentException($"Invalid dns '{dns.Name}'", nameof(dns));
    }

    public IRenderable[] ToRow(ResultValue resultValue, ResultType resultType)
    {
        return [
            new Text(Domain),
            new Text(Cloudflare?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
            new Text(Quad9Malware?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
            new Text(ControlDMalware?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
            new Text(ControlDAdsTracking?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
            new Text(ControlDFamily?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
            new Text(Dns0Zero?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
            new Text(Dns0Kids?.GetResolveTimeAsString(resultType, resultValue) ?? ""),
        ];
    }

    public string[][] GetCsvRow()
    {
        return Enumerable.Repeat(Domain, 10)
            .Select((domain, index) => new string[]
            {
                domain,
                Cloudflare.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                Cloudflare.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                Cloudflare.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
                Quad9Malware.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                Quad9Malware.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                Quad9Malware.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
                ControlDMalware.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                ControlDMalware.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                ControlDMalware.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
                ControlDAdsTracking.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                ControlDAdsTracking.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                ControlDAdsTracking.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
                ControlDFamily.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                ControlDFamily.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                ControlDFamily.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
                Dns0Zero.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                Dns0Zero.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                Dns0Zero.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
                Dns0Kids.ElementAtOrDefault(index).UdpResolveTime.CsvFormat(),
                Dns0Kids.ElementAtOrDefault(index).DoHResolveTime.CsvFormat(),
                Dns0Kids.ElementAtOrDefault(index).DoTResolveTime.CsvFormat(),
            })
            .ToArray();
    }

    public static string[] GetRowHeaders()
    {
        return
        [
            "Domain",
            "Cloudflare (UDP)",
            "Cloudflare (DoH)",
            "Cloudflare (DoT)",
            "Quad9 Malware (UDP)",
            "Quad9 Malware (DoH)",
            "Quad9 Malware (DoT)",
            "ControlD Malware (UDP)",
            "ControlD Malware (DoH)",
            "ControlD Malware (DoT)",
            "ControlD Ads & Tracking (UDP)",
            "ControlD Ads & Tracking (DoH)",
            "ControlD Ads & Tracking (DoT)",
            "ControlD Family (UDP)",
            "ControlD Family (DoH)",
            "ControlD Family (DoT)",
            "Dns0 Zero (UDP)",
            "Dns0 Zero (DoH)",
            "Dns0 Zero (DoT)",
            "Dns0 Kids (UDP)",
            "Dns0 Kids (DoH)",
            "Dns0 Kids (DoT)",
        ];
    }
}