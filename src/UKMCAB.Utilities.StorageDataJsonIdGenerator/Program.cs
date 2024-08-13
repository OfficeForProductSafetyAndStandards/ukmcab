using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using UKMCAB.Data.Models.LegislativeAreas;

var serializerSettings = new JsonSerializerSettings
{
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    },
    Formatting = Formatting.Indented
};

try
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "DesignatedStandards.json");
    var designatedStandardsJson = File.ReadAllText(path);

    var designatedStandards = JsonConvert.DeserializeObject<List<DesignatedStandard>>(designatedStandardsJson);
    if (designatedStandards == null)
    {
        Console.WriteLine("Error: file content is empty");
        return;
    }
    designatedStandards.ForEach(d => d.Id = Guid.NewGuid());

    File.WriteAllText(path, JsonConvert.SerializeObject(designatedStandards, serializerSettings));

}
catch (Exception exception)
{
    Console.WriteLine("Error: {0}", exception.Message);
}

Console.WriteLine("Done!");
