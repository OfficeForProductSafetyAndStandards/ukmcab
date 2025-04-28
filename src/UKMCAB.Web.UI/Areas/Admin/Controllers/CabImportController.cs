using System.Text.Json;
using System.Text.Json.Serialization;
using UKMCAB.Data.Interfaces.Services;
using UKMCAB.Data.Interfaces.Services.CAB;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers
{
    [Area("admin"), Route("admin/import")]
    public class CabImportController : Controller
    {
        private readonly ICABRepository cabRepo;
        private readonly IReadOnlyRepository<AreaOfCompetency> aocRepo;
        private readonly IReadOnlyRepository<Category> categoryRepo;
        private readonly IReadOnlyRepository<DesignatedStandard> designatedStandardRepo;
        private readonly IReadOnlyRepository<Data.Models.LegislativeAreas.LegislativeArea> legislativeAreaRepo;
        private readonly IReadOnlyRepository<PpeCategory> ppeCategoryRepo;
        private readonly IReadOnlyRepository<PpeProductType> ppeProductTypeRepo;
        private readonly IReadOnlyRepository<Procedure> procedureRepo;
        private readonly IReadOnlyRepository<Product> productRepo;
        private readonly IReadOnlyRepository<ProtectionAgainstRisk> protectionAgainstRiskRepo;
        private readonly IReadOnlyRepository<PurposeOfAppointment> purposeOfAppointmentRepo;
        private readonly IReadOnlyRepository<SubCategory> subCategoryRepo;
        private readonly IReadOnlyRepository<UserAccount> userAccountRepo;
        private readonly IReadOnlyRepository<UserAccountRequest> userAccountRequestRepo;

        public CabImportController(ICABRepository cabRepo,
            IReadOnlyRepository<AreaOfCompetency> aocRepo,
            IReadOnlyRepository<Category> categoryRepo,
            IReadOnlyRepository<DesignatedStandard> designatedStandardRepo,
            IReadOnlyRepository<UKMCAB.Data.Models.LegislativeAreas.LegislativeArea> legislativeAreaRepo,
            IReadOnlyRepository<PpeCategory> ppeCategoryRepo,
            IReadOnlyRepository<PpeProductType> ppeProductTypeRepo,
            IReadOnlyRepository<Procedure> procedureRepo,
            IReadOnlyRepository<Product> productRepo,
            IReadOnlyRepository<ProtectionAgainstRisk> protectionAgainstRiskRepo,
            IReadOnlyRepository<PurposeOfAppointment> purposeOfAppointmentRepo,
            IReadOnlyRepository<SubCategory> subCategoryRepo,
            IReadOnlyRepository<UserAccount> userAccountRepo,
            IReadOnlyRepository<UserAccountRequest> userAccountRequestRepo)
        {
            this.cabRepo = cabRepo;
            this.aocRepo = aocRepo;
            this.categoryRepo = categoryRepo;
            this.designatedStandardRepo = designatedStandardRepo;
            this.legislativeAreaRepo = legislativeAreaRepo;
            this.ppeCategoryRepo = ppeCategoryRepo;
            this.ppeProductTypeRepo = ppeProductTypeRepo;
            this.procedureRepo = procedureRepo;
            this.productRepo = productRepo;
            this.protectionAgainstRiskRepo = protectionAgainstRiskRepo;
            this.purposeOfAppointmentRepo = purposeOfAppointmentRepo;
            this.subCategoryRepo = subCategoryRepo;
            this.userAccountRepo = userAccountRepo;
            this.userAccountRequestRepo = userAccountRequestRepo;
        }

        [HttpGet("cabs")]
        public IActionResult Index()
        {
            var json = System.IO.File.ReadAllText("cab-data.json");
            List<Document> documents = JsonSerializer.Deserialize<List<Document>>(json);

            foreach (var doc in documents)
            {
                cabRepo.CreateAsync(doc, doc.LastUpdatedDate).Wait();
            }

            return View();
        }

        [HttpGet("user-accounts")]
        public IActionResult UserAccounts()
        {
            var userJson = System.IO.File.ReadAllText("StorageData/user-accounts.json");
            List<UserAccount> users = JsonSerializer.Deserialize<List<UserAccount>>(userJson, _serializationOptions);

            foreach (var user in users)
            {
                if (user.AuditLog is null) user.AuditLog = new List<Audit>();
                userAccountRepo.CreateAsync(user).Wait();
            }
            var userRequestJson = System.IO.File.ReadAllText("StorageData/user-account-requests.json");
            List<UserAccountRequest> userRequests = JsonSerializer.Deserialize<List<UserAccountRequest>>(userRequestJson, _serializationOptions);

            foreach (var user in userRequests)
            {
                if (user.AuditLog is null) user.AuditLog = new List<Audit>();
                userAccountRequestRepo.CreateAsync(user).Wait();
            }

            return View();
        }

        [HttpGet("supporting")]
        public IActionResult SupportingDocs()
        {
            var AreaOfCompetencyJson = System.IO.File.ReadAllText("StorageData/AreaOfCompetence.json");
            List<AreaOfCompetency> AreaOfCompetency = JsonSerializer.Deserialize<List<AreaOfCompetency>>(AreaOfCompetencyJson, _serializationOptions);
            foreach (var item in AreaOfCompetency)
            {
                if (item.ProtectionAgainstRiskIds is null)
                    item.ProtectionAgainstRiskIds = new List<Guid>();
            
                aocRepo.CreateAsync(item).Wait();
            }
            var CategoryJson = System.IO.File.ReadAllText("StorageData/Categories.json");
            List<Category> Category = JsonSerializer.Deserialize<List<Category>>(CategoryJson, _serializationOptions);
            foreach (var item in Category)
            {
                categoryRepo.CreateAsync(item).Wait();
            }
            var DesignatedStandardJson = System.IO.File.ReadAllText("StorageData/DesignatedStandards.json");
            List<DesignatedStandard> DesignatedStandard = JsonSerializer.Deserialize<List<DesignatedStandard>>(DesignatedStandardJson, _serializationOptions);
            foreach (var item in DesignatedStandard)
            {
                designatedStandardRepo.CreateAsync(item).Wait();
            }
            var LegislativeAreaJson = System.IO.File.ReadAllText("StorageData/LegislativeAreas.json");
            List<Data.Models.LegislativeAreas.LegislativeArea> LegislativeArea = JsonSerializer.Deserialize<List<Data.Models.LegislativeAreas.LegislativeArea>>(LegislativeAreaJson, _serializationOptions);
            foreach (var item in LegislativeArea)
            {
                legislativeAreaRepo.CreateAsync(item).Wait();
            }
            var PpeCategoryJson = System.IO.File.ReadAllText("StorageData/PpeCategories.json");
            List<PpeCategory> PpeCategory = JsonSerializer.Deserialize<List<PpeCategory>>(PpeCategoryJson, _serializationOptions);
            foreach (var item in PpeCategory)
            {
                ppeCategoryRepo.CreateAsync(item).Wait();
            }
            var PpeProductTypeJson = System.IO.File.ReadAllText("StorageData/PpeProductTypes.json");
            List<PpeProductType> PpeProductType = JsonSerializer.Deserialize<List<PpeProductType>>(PpeProductTypeJson, _serializationOptions);
            foreach (var item in PpeProductType)
            {
                ppeProductTypeRepo.CreateAsync(item).Wait();
            }
            var ProcedureJson = System.IO.File.ReadAllText("StorageData/Procedures.json");
            List<Procedure> Procedure = JsonSerializer.Deserialize<List<Procedure>>(ProcedureJson, _serializationOptions);
            foreach (var item in Procedure)
            {
                if (item.PurposeOfAppointmentIds is null)
                {
                    item.PurposeOfAppointmentIds = new List<Guid>();
                }
                if (item.CategoryIds is null)
                {
                    item.CategoryIds = new List<Guid>();
                }
                if (item.ProductIds is null)
                {
                    item.ProductIds = new List<Guid>();
                }
                if (item.PpeProductTypeIds is null)
                {
                    item.PpeProductTypeIds = new List<Guid>();
                }
                if (item.ProtectionAgainstRiskIds is null)
                {
                    item.ProtectionAgainstRiskIds = new List<Guid>();
                }
                if (item.AreaOfCompetencyIds is null)
                {
                    item.AreaOfCompetencyIds = new List<Guid>();
                }
                procedureRepo.CreateAsync(item).Wait();
            }
            var ProductJson = System.IO.File.ReadAllText("StorageData/Products.json");
            List<Product> Product = JsonSerializer.Deserialize<List<Product>>(ProductJson, _serializationOptions);
            foreach (var item in Product)
            {
                productRepo.CreateAsync(item).Wait();
            }
            var ProtectionAgainstRiskJson = System.IO.File.ReadAllText("StorageData/ProtectionAgainstRisks.json");
            List<ProtectionAgainstRisk> ProtectionAgainstRisk = JsonSerializer.Deserialize<List<ProtectionAgainstRisk>>(ProtectionAgainstRiskJson, _serializationOptions);
            foreach (var item in ProtectionAgainstRisk)
            {
                if (item.PpeProductTypeIds is null)
                    item.PpeProductTypeIds = new List<Guid>();
                protectionAgainstRiskRepo.CreateAsync(item).Wait();
            }
            var PurposeOfAppointmentJson = System.IO.File.ReadAllText("StorageData/PurposeOfAppointment.json");
            List<PurposeOfAppointment> PurposeOfAppointment = JsonSerializer.Deserialize<List<PurposeOfAppointment>>(PurposeOfAppointmentJson, _serializationOptions);
            foreach (var item in PurposeOfAppointment)
            {
                purposeOfAppointmentRepo.CreateAsync(item).Wait();
            }
            var SubCategoryJson = System.IO.File.ReadAllText("StorageData/SubCategories.json");
            List<SubCategory> SubCategory = JsonSerializer.Deserialize<List<SubCategory>>(SubCategoryJson, _serializationOptions);
            foreach (var item in SubCategory)
            {
                subCategoryRepo.CreateAsync(item).Wait();
            }







            return View();
        }

        public class GuidListOrEmptyStringConverter : JsonConverter<List<Guid>>
        {
            public override List<Guid> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    var str = reader.GetString();
                    return string.IsNullOrWhiteSpace(str) ? new List<Guid>() : throw new JsonException("Expected an empty string or an array.");
                }

                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    var guids = new List<Guid>();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndArray)
                            break;

                        if (reader.TokenType != JsonTokenType.String)
                            throw new JsonException("Expected string elements in the array.");

                        var str = reader.GetString();
                        if (Guid.TryParse(str, out var guid))
                            guids.Add(guid);
                        else
                            throw new JsonException($"Invalid GUID: {str}");
                    }

                    return guids;
                }

                throw new JsonException("Expected string or array.");
            }

            public override void Write(Utf8JsonWriter writer, List<Guid> value, JsonSerializerOptions options)
            {
                writer.WriteStartArray();
                foreach (var guid in value)
                    writer.WriteStringValue(guid.ToString());
                writer.WriteEndArray();
            }
        }
        public class FlexibleBoolConverter : JsonConverter<bool>
        {
            public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.True)
                    return true;
                if (reader.TokenType == JsonTokenType.False)
                    return false;
                if (reader.TokenType == JsonTokenType.String)
                {
                    var str = reader.GetString();
                    if (bool.TryParse(str, out var result))
                        return result;
                }

                throw new JsonException("Unable to convert value to bool.");
            }

            public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
                => writer.WriteBooleanValue(value);
        }
        public class NullableGuidEmptyStringConverter : JsonConverter<Guid?>
        {
            public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return null;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var str = reader.GetString();
                    if (string.IsNullOrWhiteSpace(str))
                        return null;

                    if (Guid.TryParse(str, out var guid))
                        return guid;

                    throw new JsonException($"Invalid GUID: {str}");
                }

                throw new JsonException("Expected string or null.");
            }

            public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                    writer.WriteStringValue(value.Value.ToString());
                else
                    writer.WriteStringValue(""); // or writer.WriteNullValue(); if preferred
            }
        }

        private static readonly JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters =
                    {
                        new JsonStringEnumConverter(),
                        new FlexibleBoolConverter(),
                        new GuidListOrEmptyStringConverter(),
                        new NullableGuidEmptyStringConverter()
                    }
        };
    }
}
