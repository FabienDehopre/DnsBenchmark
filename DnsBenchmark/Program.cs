using System.Text.Json;

var domains = await LoadDomains();
var results = domains.Select(domain => new BenchmarkResultRow { Domain = domain }).ToDictionary(k => k.Domain, v => v);
await AnsiConsole.Progress()
    .AutoClear(true)
    .AutoRefresh(true)
    .HideCompleted(false)
    .Columns(
        new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new SpinnerColumn(),
        new RemainingTimeColumn()
    )
    .StartAsync(async ctx =>
    {
        var cloudflare = ctx.AddTask(DnsServer.CloudflareDns.Name, autoStart: false, maxValue: domains.Length);
        var quad9Malware = ctx.AddTask(DnsServer.Quad9MalwareDns.Name, autoStart: false, maxValue: domains.Length);
        var controlDMalware = ctx.AddTask(DnsServer.ControlDMalwareDns.Name, autoStart: false, maxValue: domains.Length);
        var controlDAdsTracking = ctx.AddTask(DnsServer.ControlDAdsTrackingDns.Name, autoStart: false, maxValue: domains.Length);
        var controlDFamily = ctx.AddTask(DnsServer.ControlDFamilyDns.Name, autoStart: false, maxValue: domains.Length);
        var dns0Zero = ctx.AddTask(DnsServer.Dns0ZeroDns.Name, autoStart: false, maxValue: domains.Length);
        var dns0Kids = ctx.AddTask(DnsServer.Dns0KidsDns.Name, autoStart: false, maxValue: domains.Length);

        Task[] benchmarks =
        [
            Benchmark(cloudflare, DnsServer.CloudflareDns, domains, results),
            Benchmark(quad9Malware, DnsServer.Quad9MalwareDns, domains, results),
            Benchmark(controlDMalware, DnsServer.ControlDMalwareDns, domains, results),
            Benchmark(controlDAdsTracking, DnsServer.ControlDAdsTrackingDns, domains, results),
            Benchmark(controlDFamily, DnsServer.ControlDFamilyDns, domains, results),
            Benchmark(dns0Zero, DnsServer.Dns0ZeroDns, domains, results),
            Benchmark(dns0Kids, DnsServer.Dns0KidsDns, domains, results),
        ];
        
        await Task.WhenAll(benchmarks);
    });

if (DnsServer.HasErrors)
{
    var viewErrors = AnsiConsole.Prompt(
        new TextPrompt<bool>("[yellow]Resolving phase completed with errors.[/] View errors?")
            .AddChoice(true)
            .AddChoice(false)
            .DefaultValue(true)
            .WithConverter(choice => choice ? "y" : "n"));
    if (viewErrors)
    {
        DnsServer.PrintErrors();
        AnsiConsole.MarkupLine("Press [bold][[Enter]][/] key to continue...");
        Console.ReadLine();
    }
}

SelectionPromptChoice? choice = null;
while (choice is not { Command: "exit" })
{
    choice = AnsiConsole.Prompt(
        new SelectionPrompt<SelectionPromptChoice>()
            .Title("[lime]Resolving phase completeted.[/] What would you like to do?")
            .UseConverter(c => c.Text)
            .AddChoices(
                new SelectionPromptChoice("print_table_Udp", "Show the UDP resolve times"),
                new SelectionPromptChoice("print_table_DoH", "Show the DoH resolve times"),
                new SelectionPromptChoice("print_table_DoT", "Show the DoT resolve times"),
                new SelectionPromptChoice("save_as_csv", "Save results as CSV"),
                // new SelectionPromptChoice("save_as_xlsx", "Save results as Excel Sheets"),
                new SelectionPromptChoice("save_as_json", "Save results as JSON"),
                new SelectionPromptChoice("exit", "Exit")));

    if (choice.Command.StartsWith("print_table_"))
    {
        var resultValue = Enum.Parse<ResultValue>(choice.Command.Split('_').Last());
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<SelectionPromptChoice>()
                .Title("Which data would you like to see?")
                .UseConverter(c => c.Text)
                .AddChoices(
                    new SelectionPromptChoice("Minimum", "Minimum time"),
                    new SelectionPromptChoice("Maximum", "Maximum time"),
                    new SelectionPromptChoice("Average", "Average time"),
                    new SelectionPromptChoice("Median", "Median time")));
        var resultType = Enum.Parse<ResultType>(type.Command);
        OutputResult(results.Values, resultValue, resultType);
    } 
    else if (choice.Command.StartsWith("save_as_csv"))
    {
        var fileName = AnsiConsole.Prompt(
            new TextPrompt<string>("Save As [grey](results.csv)[/]")
                .Validate(v => v.EndsWith(".csv") ? ValidationResult.Success() : ValidationResult.Error("The file name must have the .csv extension."))
                .DefaultValue("results.csv"));
        await SaveAsCsv(fileName, results.Values);
    } 
    else if (choice.Command.StartsWith("save_as_json"))
    {
        var fileName = AnsiConsole.Prompt(
            new TextPrompt<string>("Save As [grey](results.json)[/]")
                .Validate(v => v.EndsWith(".json") ? ValidationResult.Success() : ValidationResult.Error("The file name must have the .json extension."))
                .DefaultValue("results.json"));
        await SaveAsJson(fileName, results.Values);
    }
    
    AnsiConsole.Clear();
}


async Task<string[]> LoadDomains()
{
    var data = await File.ReadAllLinesAsync("cloudflare-radar_top-1000-domains.csv");
    if (data.First().Equals("domain", StringComparison.InvariantCultureIgnoreCase))
    {
        data = data.Skip(1).ToArray();    
    }
    
    return [
#if DEBUG
        ..data.OrderBy(x => Guid.NewGuid()).Take(10),
#else
        ..data,
#endif
        "dehopre.com", "dehopre.dev", "dehop.re"
    ];
}

async Task SaveAsCsv(string fileName, IEnumerable<BenchmarkResultRow> data)
{
    await using (var writer = new StreamWriter(fileName))
    {
        writer.NewLine = "\r\n";
        await writer.WriteLineAsync(string.Join(',', BenchmarkResultRow.GetRowHeaders()));
        foreach (var row in data)
        {
            var csvRows = row.GetCsvRow();
            foreach (var csvRow in csvRows)
            {
                await writer.WriteLineAsync(string.Join(',', csvRow));
            }
        }

        await writer.FlushAsync();
    }
    
    AnsiConsole.MarkupLineInterpolated($"[lime]Results successfully saved to [dim]{fileName}[/].[/]");
    AnsiConsole.MarkupLine("Press [bold][[Enter]][/] key to continue...");
    Console.ReadLine();
}

async Task SaveAsJson(string fileName, IEnumerable<BenchmarkResultRow> data)
{
    await using (var writer = File.Create(fileName))
    {
        await JsonSerializer.SerializeAsync(writer, data);   
        await writer.FlushAsync();
    }
    
    AnsiConsole.MarkupLineInterpolated($"[lime]Results successfully saved to [dim]{fileName}[/].[/]");
    AnsiConsole.MarkupLine("Press [bold][[Enter]][/] key to continue...");
    Console.ReadLine();
}

void OutputResult(IEnumerable<BenchmarkResultRow> data, ResultValue resultValue, ResultType resultType)
{
    var table = new Table().Border(TableBorder.Rounded);
    table.AddColumns(
        new TableColumn("Domain"),
        new TableColumn("Cloudflare"),
        new TableColumn("Quad9 Malware"),
        new TableColumn("ControlD Malware"),
        new TableColumn("ControlD Ads & Tracking"),
        new TableColumn("ControlD Family"),
        new TableColumn("Dns0 Zero"),
        new TableColumn("Dns0 Kids")
        );
    
    foreach (var row in data)
    {
        table.AddRow(row.ToRow(resultValue, resultType));
    }
    
    AnsiConsole.Clear();
    AnsiConsole.Write(table);
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("Press [bold][[Enter]][/] key to continue...");
    Console.ReadLine();
}

async Task Benchmark(ProgressTask task, DnsServer dns, string[] domains, Dictionary<string, BenchmarkResultRow> results)
{
    var rnd = new Random();
    task.StartTask();

    foreach (var domain in domains)
    {
        var resolveResults = new List<DnsResolveResult>();
        for (int i = 0; i < 10; i++)
        {
            var resolveResult = await dns.Resolve(domain);
            resolveResults.Add(resolveResult);
            await Task.Delay(rnd.Next(200, 1000));
        }
        
        results[domain].SetResult(dns, resolveResults);
        task.Increment(1d);
    }

    task.StopTask();
}