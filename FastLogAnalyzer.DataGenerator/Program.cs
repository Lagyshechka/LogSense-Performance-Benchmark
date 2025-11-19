using System.Diagnostics;
using System.Text;

class FastLogAnalyzer_DataGenerator
{
    static void Main(string[] args)
    {
        string path = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.Desktop), "test_log.csv");
        int lineCount = 5_000_000; // 5 mil lines
        
        Console.WriteLine($"Generating {lineCount} lines into {path}");
        
        var sw = Stopwatch.StartNew();
        using (var writer = new StreamWriter(path, false, Encoding.UTF8, 65536)) // 65536 - bufferSize of writer for speed 
        {
            var random = new Random();
            var date = DateTime.Now;
            var levels = new[] {"INFO", "WARN", "ERROR", "DEBUG"};
            var ips = new[] { "192.168.1.1", "10.0.0.5", "172.16.0.1", "172.0.0.1" };

            for (int i = 0; i < lineCount; i++)
            {
                writer.Write(date.AddSeconds(i).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                writer.Write(",");
                writer.Write(levels[random.Next(levels.Length)]);
                writer.Write(',');
                writer.Write("Database connection failed or timeout occured at service level");
                writer.Write(',');
                writer.Write(ips[random.Next(ips.Length)]);
                writer.Write(',');
                writer.WriteLine(random.Next(10, 5000));
            }
        }
        
        sw.Stop();
        Console.WriteLine($"Process finished. Final size: {new FileInfo(path).Length / 1024 / 1024} MB");
        Console.WriteLine($"Time elapsed: {sw.Elapsed.TotalSeconds:F2} sec");
    }
}