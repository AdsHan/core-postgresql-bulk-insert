using Microsoft.Extensions.Configuration;
using PerformanceBulkInsert.Strategy;
using System.Diagnostics;

class Program
{
    static IConfiguration configuration;

    static async Task Main(string[] args)
    {
        configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        Console.WriteLine("************");
        Console.WriteLine("Tomada de tempo para gravação de 1.000.000 registros");
        Console.WriteLine("************");

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var entityStrategy = new EntityStrategy();
        IEntityStrategy strategy = entityStrategy.SetEntityStrategy(EntityStrategyType.Product, "public", configuration.GetConnectionString("PostgreSQLCs"));

        await strategy.InsertTemporaryTableAsync();

        stopwatch.Stop();
        Console.WriteLine($"Tempo para inserir na tabela temporária: {FormatDate(stopwatch)}");
        Console.WriteLine("");

        stopwatch = new Stopwatch();
        stopwatch.Start();

        await strategy.MergeMainTableAsync();

        stopwatch.Stop();
        Console.WriteLine($"Tempo para executar o merge na tabela principal: {FormatDate(stopwatch)}");
        Console.WriteLine("");
        Console.WriteLine("************");
        Console.ReadLine();
    }

    private static string FormatDate(Stopwatch date)
    {
        TimeSpan ts = date.Elapsed;
        return String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
    }

}