using nClam;
using System.Text;

namespace TestClamAV.Services
{
    public interface IClamAVService
    {
        Task<string> ScanFile(string filePath);
        Task<string> ScanHtml(string htmlContent);
        Task<string> ScanFileAsync(Stream fileStream);
    }
    public class ClamAVService : IClamAVService
    {
        private readonly ClamClient _clamClient;

        public ClamAVService(IConfiguration configuration)
        {
            var clamAVHost = configuration["ClamAV:Host"] ?? throw new ArgumentNullException("ClamAV:Host is missing.");
            var clamAVPort = int.Parse(configuration["ClamAV:Port"] ?? throw new ArgumentNullException("ClamAV:Port is missing."));
            var maxStreamSize = configuration.GetValue<long>("ClamAV:MaxStreamSize", 50 * 1024 * 1024);

            _clamClient = new ClamClient(clamAVHost, clamAVPort)
            {
                MaxStreamSize = maxStreamSize
            };
        }

        public async Task<string> ScanFile(string filePath)
        {
            if (!File.Exists(filePath))
                return "File not found.";
            try
            {
                var result = await _clamClient.ScanFileOnServerAsync(filePath);
                return FormatScanResult(result, "File");
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public async Task<string> ScanHtml(string htmlContent)
        {
            try
            {
                using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
                var result = await _clamClient.SendAndScanFileAsync(memoryStream);
                return FormatScanResult(result, "HTML");
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public async Task<string> ScanFileAsync(Stream fileStream)
        {
            try
            {
                var result = await _clamClient.SendAndScanFileAsync(fileStream);
                return FormatScanResult(result, "File");
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        private static string FormatScanResult(ClamScanResult result, string type)
        {
            return result.Result switch
            {
                ClamScanResults.Clean => $"{type} is clean.",
                ClamScanResults.VirusDetected => $"Virus Found: {string.Join(", ", result.InfectedFiles.Select(i => i.VirusName))}",
                ClamScanResults.Error => $"Error: {result.RawResult}",
                _ => "Unknown result."
            };
        }
    }
}