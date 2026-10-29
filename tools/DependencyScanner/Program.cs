using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Usage: DependencyScanner <binary folder> <report.json>");
var assemblies = new List<object>();
foreach (var path in Directory.EnumerateFiles(args[0]).Order()) {
    if (!new[] { ".dll", ".gha" }.Contains(Path.GetExtension(path).ToLowerInvariant())) continue;
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    if (!pe.HasMetadata) continue;
    var metadata = pe.GetMetadataReader();
    if (!metadata.IsAssembly) continue;
    var definition = metadata.GetAssemblyDefinition();
    var references = metadata.AssemblyReferences.Select(handle => {
        var reference = metadata.GetAssemblyReference(handle);
        return metadata.GetString(reference.Name);
    }).ToArray();
    var resourceReferences = new List<string>();
    var resourceDirectory = pe.PEHeaders.CorHeader.ResourcesDirectory;
    if (resourceDirectory.Size > 0) {
        var bytes = pe.GetSectionData(resourceDirectory.RelativeVirtualAddress).GetContent(0, resourceDirectory.Size);
        // Preserialized .resources name their reader in their header rather than
        // necessarily using an AssemblyRef. Keep the resource reader dependency.
        if (Encoding.UTF8.GetString(bytes.AsSpan()).Contains("System.Resources.Extensions"))
            resourceReferences.Add("System.Resources.Extensions");
    }
    assemblies.Add(new { file = Path.GetFileName(path), name = metadata.GetString(definition.Name),
        version = definition.Version.ToString(), references, resourceReferences });
}
File.WriteAllText(args[1], JsonSerializer.Serialize(assemblies, new JsonSerializerOptions { WriteIndented = true }));
