using System.Text.Json;

namespace PMCR.Core.Trails;

public sealed class TrailService
{
    public async Task<string> OpenFrameAsync(string role, string intent, string sourceType = "pmcro")
    {
        var uuid = Guid.NewGuid();
        var dir = Path.Combine(".pmcro", "trails", sourceType, uuid.ToString());
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"00-{role}.jsonl");
        var payload = new { phase = "open", role, intent, sourceType, timestamp = DateTime.UtcNow, law = "EC-SYS-003" };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(payload) + "\n");
        return Path.Combine(dir, $"{uuid}");
    }

    public async Task WriteFrameAsync(string role, string trailPath, object data, string evidence = "")
    {
        var framePath = Path.Combine(Path.GetDirectoryName(trailPath)!, $"{role}.jsonl");
        var payload = new { role, timestamp = DateTime.UtcNow, data, evidence };
        await File.AppendAllTextAsync(framePath, JsonSerializer.Serialize(payload) + "\n");
    }
}
