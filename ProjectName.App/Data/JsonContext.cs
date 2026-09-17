using System.Text.Json.Serialization;
using ProjectName.App.Models;

// Catalog and seed files are authored camelCase; matching leniently keeps the
// JSON readable without annotating every property.
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(ProjectTask))]
[JsonSerializable(typeof(ProjectsJson))]
[JsonSerializable(typeof(Category))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(PmcrCycle))]
[JsonSerializable(typeof(PmcrRuntime))]
[JsonSerializable(typeof(Dso))]
[JsonSerializable(typeof(DsoCatalogFile))]
[JsonSerializable(typeof(MarketplaceItem))]
[JsonSerializable(typeof(MarketplaceCatalogFile))]
public partial class JsonContext : JsonSerializerContext
{
}