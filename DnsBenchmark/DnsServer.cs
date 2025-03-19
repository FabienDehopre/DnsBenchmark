namespace DnsBenchmark;

public class DnsServer
{
    public static readonly DnsServer CloudflareDns = new("Cloudflare", "1.1.1.1", "https://dns.cloudflare.com/dns-query", "one.one.one.one");
    public static readonly DnsServer Quad9MalwareDns = new("Quad9Malware", "9.9.9.9", "https://dns.quad9.net/dns-query", "dns.quad9.net");
    public static readonly DnsServer ControlDMalwareDns = new("ControlDMalware", "76.76.2.1", "https://freedns.controld.com/p1", "p1.freedns.controld.com");
    public static readonly DnsServer ControlDAdsTrackingDns = new("ControlDAdsTracking", "76.76.2.2", "https://freedns.controld.com/p2", "p2.freedns.controld.com");
    public static readonly DnsServer ControlDFamilyDns = new("ControlDFamily", "76.76.2.4", "https://freedns.controld.com/p4", "p4.freedns.controld.com");
    public static readonly DnsServer Dns0ZeroDns = new("Dns0Zero", "193.110.81.9", "https://zero.dns0.eu/", "zero.dns0.eu");
    public static readonly DnsServer Dns0KidsDns = new("Dns0Kids", "193.110.81.1", "https://kids.dns0.eu/", "kids.dns0.eu");
    
    private readonly ClientX _udpClient;
    private readonly ClientX _httpsClient;
    private readonly ClientX _tlsClient;

    public DnsServer(string name, string ip, string https, string tls)
    {
        Name = name;
        _udpClient = new(ip, DnsRequestFormat.DnsOverUDP);
        _httpsClient = new(new Uri(https), DnsRequestFormat.DnsOverHttps);
        _tlsClient = new(tls, DnsRequestFormat.DnsOverTLS);
    }
    
    public string Name { get; }
    private List<Markup> Errors { get; set; } = new();
    private bool ContainsError => Errors.Count > 0;
    public static bool HasErrors => 
        CloudflareDns.ContainsError ||
        Quad9MalwareDns.ContainsError ||
        ControlDMalwareDns.ContainsError ||
        ControlDAdsTrackingDns.ContainsError ||
        ControlDFamilyDns.ContainsError ||
        Dns0ZeroDns.ContainsError ||
        Dns0KidsDns.ContainsError;

    public async Task<DnsResolveResult> Resolve(string domain)
    {
        Task<(double? result, Markup? error)>[] resolveTasks = [
            Resolve(_udpClient, domain),
            Resolve(_httpsClient, domain),
            Resolve(_tlsClient, domain),
        ];
        await Task.WhenAll(resolveTasks);
        var (udp, udpError) = await resolveTasks[0];
        var (doh, dohError) = await resolveTasks[1];
        var (dot, dotError) = await resolveTasks[2];
        Errors.AddIfNotNull(udpError);
        Errors.AddIfNotNull(dohError);
        Errors.AddIfNotNull(dotError);
        return new DnsResolveResult(udp, doh, dot);
    }

    private void PrintErrorsToConsole()
    {
        foreach (var error in Errors)
        {
            AnsiConsole.Write(error);
            AnsiConsole.WriteLine();
        }
    }

    public static void PrintErrors()
    {
        if (CloudflareDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]Cloudflare Errors[/]").Centered());
            CloudflareDns.PrintErrorsToConsole();
        }
        
        if (Quad9MalwareDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]Quad9 Malware Errors[/]").Centered());
            Quad9MalwareDns.PrintErrorsToConsole();
        }
        
        if (ControlDMalwareDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]ControlD Malware Errors[/]").Centered());
            ControlDMalwareDns.PrintErrorsToConsole();
        }
        
        if (ControlDAdsTrackingDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]ControlD Ads & Tracking Errors[/]").Centered());
            ControlDAdsTrackingDns.PrintErrorsToConsole();
        }
        
        if (ControlDFamilyDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]ControlD Family Errors[/]").Centered());
            ControlDFamilyDns.PrintErrorsToConsole();
        }
        
        if (Dns0ZeroDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]Dns0 Zero Errors[/]").Centered());
            Dns0ZeroDns.PrintErrorsToConsole();
        }
        
        if (Dns0KidsDns.ContainsError)
        {
            AnsiConsole.Write(new Rule("[red]Dns0 Kids Errors[/]").Centered());
            Dns0KidsDns.PrintErrorsToConsole();
        }
    }

    private static async Task<(double? result, Markup? error)> Resolve(ClientX client, string domain)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var data = await client.Resolve(domain);
            stopwatch.Stop();
            if (!string.IsNullOrWhiteSpace(data.Error))
            {
                return (
                    null, 
                    new Markup($"Error while resolving [deepskyblue1 italic]{domain}[/] using DNS [chartreuse3]{client.GetDnsServer()} [dim]({client.GetDnsRequestFormat()})[/][/]: [red bold]{data.Error}[/]"));
            }
            
            return (stopwatch.Elapsed.TotalMilliseconds, null);
        }
        catch (Exception ex)
        {
            return (
                null,
                new Markup($"Exception while resolving [deepskyblue1 italic]{domain}[/] using DNS [chartreuse3]{client.GetDnsServer()} [dim]({client.GetDnsRequestFormat()})[/][/]: [red bold]{ex.Message}[/]"));
        }
    }
}