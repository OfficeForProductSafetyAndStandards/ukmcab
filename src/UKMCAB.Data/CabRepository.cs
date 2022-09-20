using System.Text.Json.Nodes;
using System.Text.Json;
using MoreLinq;
using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Configuration;

namespace UKMCAB.Data;

/// <summary>
/// NOTE: THIS IS FOR ALPHA ONLY.
/// </summary>
public static class CabRepository
{
    private static CabData[] _cabs = Array.Empty<CabData>();
    private static new Dictionary<string, string> _pdfIdTextMap = new();

    public static IConfiguration Config { get; set; }

    public static async Task LoadAsync()
    {
        var cabsJson = await LoadTransformJsonAsync();
        _cabs = JsonSerializer.Deserialize<CabData[]>(cabsJson);
        _cabs = _cabs.Where(x => !x.Name.Contains("demo")).ToArray();

        _cabs.ForEach(x => x.TestingLocations = x.TestingLocations?.Where(x => x != "null").ToList() ?? new List<string>());

        await LoadPdfTextAsync();

        foreach (var cab in _cabs)
        {
            cab.RawJsonData = JsonSerializer.Serialize(cab);
            var pdfIds = (cab.Pdfs ?? "").Split("#");
            foreach (var pdfId in pdfIds)
            {
                cab.RawAllPdfText += " " + (_pdfIdTextMap.ContainsKey(pdfId) ? _pdfIdTextMap[pdfId] : string.Empty);
            }
            cab.RawAllText = cab.RawJsonData + " " + cab.RawAllPdfText;
            cab.SearchFields = GetSearchFieldsString(cab);

            cab.BodyType ??= new List<string>();
            cab.TestingLocations ??= new List<string>();
            cab.RegisteredOfficeLocation ??= new List<string>();
        }
    }

    private static string GetSearchFieldsString(CabData cab)
    {
        var searchFields = new List<string>();
        // CAB name
        searchFields.Add(cab.Name.Trim());
        // body number
        if (!string.IsNullOrWhiteSpace(cab.BodyNumber))
        {
            searchFields.Add(cab.BodyNumber.Trim());
        }
        // external id: included for dev testing
        if (!string.IsNullOrWhiteSpace(cab.ExternalID))
        {
            searchFields.Add(cab.ExternalID.Trim());
        }

        if (cab.LegislativeAreas != null && cab.LegislativeAreas.Any())
        {
            searchFields.AddRange(cab.LegislativeAreas);
        }
        if (cab.Regulations != null && cab.Regulations.Any())
        {
            foreach (var regulation in cab.Regulations)
            {
                // regulation name
                searchFields.Add(regulation.Name.Trim());
                if (regulation.ProductGroups != null && regulation.ProductGroups.Any())
                {
                    foreach (var productGroup in regulation.ProductGroups)
                    {
                        if (productGroup.Lines != null && productGroup.Lines.Any())
                        {
                            // product name
                            searchFields.AddRange(productGroup.Lines.Select(l => l.Name.Trim()));
                        }

                        if (productGroup.StandardsSpecificationsList != null && productGroup.StandardsSpecificationsList.Any())
                        {
                            // standards number
                            searchFields.AddRange(productGroup.StandardsSpecificationsList.Select(s => s.Value.Trim()));
                        }

                        if (productGroup.Schedules != null && productGroup.Schedules.Any())
                        {
                            foreach (var schedule in productGroup.Schedules)
                            {
                                // schedule name
                                searchFields.Add(schedule.Name.Trim());
                                if (schedule.PartsModuleList != null && schedule.PartsModuleList.Any())
                                {
                                    // module name
                                    // part name
                                    searchFields.AddRange(schedule.PartsModuleList.Select(p => p.Label.Trim()));
                                }
                            }
                        }
                    }
                }
            }
        }

        return string.Join('|', searchFields);
    }



    private static async Task LoadPdfTextAsync()
    {
        if (_pdfIdTextMap.Count == 0)
        {
            const string indexName = "azureblob-index";

            var endpoint = new Uri(Config["AzureSearchEndPoint"]);
            var key = Config["AzureSearchKey"];

            var credential = new AzureKeyCredential(key);
            var client = new SearchClient(endpoint, indexName, credential);

            var response = await client.SearchAsync<SearchDocument>("", new SearchOptions { Size = 200 });
            var results = response.Value.GetResults().ToArray();


            foreach (var data in results)
            {
                var id = data.Document.GetString("metadata_storage_name");
                var content = data.Document.GetString("content");
                _pdfIdTextMap[id] = content;
            }
        }
    }


    public static string[] GetBodyTypeFacets() => _cabs.SelectMany(x => x.BodyType ?? new List<string>()).Distinct().OrderBy(x => x).ToArray();

    public static string[] GetRegisteredOfficeLocationFacets() => _cabs.SelectMany(x => x.RegisteredOfficeLocation ?? new List<string>()).Distinct().OrderBy(x => x).ToArray();

    public static string[] GetTestingLocationFacets() => _cabs.SelectMany(x => x.TestingLocations ?? new List<string>()).Distinct().OrderBy(x => x).ToArray();

    public static string[] GetLegislativeAreaFacets() => _cabs.SelectMany(x => x.LegislativeAreas ?? new List<string>()).Distinct().OrderBy(x => x).ToArray();


    public static CabData Get(string id)
    {
        return _cabs.Single(c => c.ExternalID == id);
    }
    
    public static CabData[] Search(string text, string[] bodyTypes = null!, string[] registeredOfficeLocation = null!, string[] testingLocations = null!,  string[] legislativeAreas = null!)
    {
        bodyTypes ??= Array.Empty<string>();
        registeredOfficeLocation ??= Array.Empty<string>();
        testingLocations ??= Array.Empty<string>();
        legislativeAreas ??= Array.Empty<string>();

        var results = _cabs;

        if ((text ?? "").Trim().Length > 0)
        {
            results = results.Where(x => x.SearchFields.Contains(text, StringComparison.InvariantCultureIgnoreCase)).ToArray();
        }

        if (bodyTypes.Length > 0)
        {
            results = results.Where(x => x.BodyType.Intersect(bodyTypes, StringComparer.CurrentCultureIgnoreCase).Count() > 0).ToArray();
        }

        if (registeredOfficeLocation.Length > 0)
        {
            results = results.Where(x => x.RegisteredOfficeLocation.Intersect(registeredOfficeLocation, StringComparer.CurrentCultureIgnoreCase).Count() > 0).ToArray();
        }

        if (testingLocations.Length > 0)
        {
            results = results.Where(x => x.TestingLocations.Intersect(testingLocations, StringComparer.CurrentCultureIgnoreCase).Count() > 0).ToArray();
        }

        if (legislativeAreas.Length > 0)
        {
            results = results.Where(x => x.LegislativeAreas.Intersect(legislativeAreas, StringComparer.CurrentCultureIgnoreCase).Count() > 0).ToArray();
        }

        return results;
    }




    static async Task<string> LoadTransformJsonAsync()
    {
        var json = await new HttpClient().GetStringAsync("https://app-umb-ukmcab.azurewebsites.net/umbraco/surface/cabdata/json");
        JsonNode node = JsonNode.Parse(json)!;

        Recurse(node);
        Recurse2(node);

        var cabs = node.Root.AsObject().Single().Value.AsArray();


        var options = new JsonSerializerOptions { WriteIndented = true };
        var cabsJson = cabs.ToJsonString(options);

        return cabsJson;


        void Recurse(JsonNode node, string name = null)
        {
            if (node is JsonObject jObject)
            {
                var subs = jObject.ToArray();
                foreach (var sub in subs)
                {
                    if (sub.Value is JsonValue)
                    {
                        if (sub.Key.Equals("@nodeName"))
                        {
                            jObject.Remove(sub.Key);
                            jObject.Add("name", sub.Value);
                        }
                        else if (sub.Key.StartsWith("@"))
                        {
                            jObject.Remove(sub.Key);
                        }
                        else if (sub.Key == "#cdata-section")
                        {
                            var data = sub.Value.ToString();
                            jObject.Remove(sub.Key);
                            var parent = jObject.Parent.AsObject();
                            parent.Remove(name);

                            if (data.StartsWith(@"[{""") || data.StartsWith(@"[")) // double encoded json
                            {
                                parent.Add(name, JsonNode.Parse(data));
                            }
                            else
                            {
                                parent.Add(name, JsonValue.Create(data));
                            }
                        }
                    }
                    else
                    {
                        Recurse(sub.Value, sub.Key);
                    }
                }
            }
            else if (node is JsonArray jArray)
            {
                foreach (var sub in jArray)
                {
                    Recurse(sub);
                }
            }
        }

        void Recurse2(JsonNode node, string name = null)
        {
            if (new[] { "productGroup", "regulation", "schedule", "testingLocations" }.Any(x => x == name) && node.Parent is JsonObject && (node is JsonObject || node is JsonValue))
            {
                var parent = node.Parent.AsObject();
                parent.Remove(name);
                var arr = new JsonArray(node);
                parent.Add(name, arr);
                Recurse2(arr);
            }
            else if (node is JsonObject jObject)
            {
                var subs = jObject.ToArray();
                foreach (var sub in subs)
                {
                    Recurse2(sub.Value, sub.Key);
                }
            }
            else if (node is JsonArray jArray)
            {
                foreach (var sub in jArray)
                {
                    Recurse2(sub);
                }
            }
        }
    }



}
