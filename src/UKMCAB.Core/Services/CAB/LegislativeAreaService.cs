using AutoMapper;
using UKMCAB.Common;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.Models;
using LinqKit;
using UKMCAB.Data.Interfaces.Services;

namespace UKMCAB.Core.Services.CAB;

public class LegislativeAreaService : ILegislativeAreaService
{
    private readonly IReadOnlyRepository<LegislativeArea> _legislativeAreaRepository;
    private readonly IReadOnlyRepository<PurposeOfAppointment> _purposeOfAppointmentRepository;
    private readonly IReadOnlyRepository<Category> _categoryRepository;
    private readonly IReadOnlyRepository<SubCategory> _subCategoryRepository;
    private readonly IReadOnlyRepository<Product> _productRepository;
    private readonly IReadOnlyRepository<Procedure> _procedureRepository;
    private readonly IReadOnlyRepository<DesignatedStandard> _designatedStandardRepository;
    private readonly IReadOnlyRepository<PpeCategory> _ppeCategoryRepository;
    private readonly IReadOnlyRepository<PpeProductType> _ppeProductTypeRepository;
    private readonly IReadOnlyRepository<ProtectionAgainstRisk> _protectionAgainstRiskRepository;
    private readonly IReadOnlyRepository<AreaOfCompetency> _areaOfCompetencyRepository;
    private readonly IMapper _mapper;

    public LegislativeAreaService(
        IReadOnlyRepository<LegislativeArea> legislativeAreaRepository,
        IReadOnlyRepository<PurposeOfAppointment> purposeOfAppointmentRepository,
        IReadOnlyRepository<Category> categoryAreRepository,
        IReadOnlyRepository<Product> productRepository,
        IReadOnlyRepository<Procedure> procedureRepository,
        IReadOnlyRepository<SubCategory> subCategoryRepository,
        IReadOnlyRepository<DesignatedStandard> designatedStandardRepository,
        IMapper mapper)
    {
        _legislativeAreaRepository = legislativeAreaRepository;
        _purposeOfAppointmentRepository = purposeOfAppointmentRepository;
        _categoryRepository = categoryAreRepository;
        _productRepository = productRepository;
        _procedureRepository = procedureRepository;
        _subCategoryRepository = subCategoryRepository;
        _designatedStandardRepository = designatedStandardRepository;
        _mapper = mapper;
    }
    public LegislativeAreaService(
        IReadOnlyRepository<LegislativeArea> legislativeAreaRepository,
        IReadOnlyRepository<PurposeOfAppointment> purposeOfAppointmentRepository,
        IReadOnlyRepository<Category> categoryAreRepository,
        IReadOnlyRepository<Product> productRepository,
        IReadOnlyRepository<Procedure> procedureRepository,
        IReadOnlyRepository<SubCategory> subCategoryRepository,
        IReadOnlyRepository<DesignatedStandard> designatedStandardRepository,
        IReadOnlyRepository<PpeCategory> ppeCategoryRepository,
        IReadOnlyRepository<PpeProductType> ppeProductTypeRepository,
        IReadOnlyRepository<ProtectionAgainstRisk> protectionAgainstRiskRepository,
        IReadOnlyRepository<AreaOfCompetency> areaOfCompetencyRepository,

    IMapper mapper)
    {
        _legislativeAreaRepository = legislativeAreaRepository;
        _purposeOfAppointmentRepository = purposeOfAppointmentRepository;
        _categoryRepository = categoryAreRepository;
        _productRepository = productRepository;
        _procedureRepository = procedureRepository;
        _subCategoryRepository = subCategoryRepository;
        _designatedStandardRepository = designatedStandardRepository;
        _ppeCategoryRepository = ppeCategoryRepository;
        _ppeProductTypeRepository = ppeProductTypeRepository;
        _protectionAgainstRiskRepository = protectionAgainstRiskRepository;
        _areaOfCompetencyRepository = areaOfCompetencyRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreasAsync()
    {
        var result = await _legislativeAreaRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<LegislativeAreaModel>>(result);
    }

    public async Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasAsync(List<Guid> excludeLegislativeAreaIds)
    {
        var allLegislativeAreas = await this.GetAllLegislativeAreasAsync();
        return allLegislativeAreas.Where(n => !excludeLegislativeAreaIds.Contains(n.Id));
    }

    public async Task<IEnumerable<LegislativeAreaModel>> GetLegislativeAreasByRoleId(string roleId)
    {
        Guard.IsFalse<ArgumentNullException>(string.IsNullOrWhiteSpace(roleId));
        var result = await _legislativeAreaRepository.QueryAsync(l => l.RoleId == roleId);
        return _mapper.Map<IEnumerable<LegislativeAreaModel>>(result);
    }

    public async Task<LegislativeAreaModel> GetLegislativeAreaByIdAsync(Guid legislativeAreaId)
    {
        Guard.IsTrue(legislativeAreaId != Guid.Empty, "Guid cannot be empty");
        var la = await _legislativeAreaRepository.QueryAsync(l => l.Id == legislativeAreaId);
        return _mapper.Map<LegislativeAreaModel>(la.First());
    }
    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid legislativeAreaId, int? pageNumber = null, string? searchTerm = null, int pageSize = 20, List<Guid>? designatedStandardIds = null, bool isShowAllSelectedDesignatedRequest = false, bool showAllSelectedIsOn = false)
    {
        var purposeOfAppointments = await _purposeOfAppointmentRepository.QueryAsync(x => x.LegislativeAreaId == legislativeAreaId);
        if (purposeOfAppointments.Any())
        {
            return new ScopeOfAppointmentOptionsModel 
            {
                PurposeOfAppointments = _mapper.Map<IEnumerable<PurposeOfAppointmentModel>>(purposeOfAppointments)
            };
        }

        var categories = await _categoryRepository.QueryAsync(x => x.LegislativeAreaId == legislativeAreaId);
        if (categories.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Categories = _mapper.Map<IEnumerable<CategoryModel>>(categories)
            };
        }

        var products = await _productRepository.QueryAsync(x => x.LegislativeAreaId == legislativeAreaId);
        if (products.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Products = _mapper.Map<IEnumerable<ProductModel>>(products)
            };
        }

        var procedures = await _procedureRepository.QueryAsync(x => x.LegislativeAreaId == legislativeAreaId);
        if (procedures.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Procedures = _mapper.Map<IEnumerable<ProcedureModel>>(procedures)
            };
        }

        var designatedStandardPredicate = PredicateBuilder.New<DesignatedStandard>(x => x.LegislativeAreaId == legislativeAreaId);

        if ((isShowAllSelectedDesignatedRequest || showAllSelectedIsOn) && designatedStandardIds is not null && designatedStandardIds?.Count > 0)
        {
            designatedStandardPredicate = designatedStandardPredicate.And(x => designatedStandardIds.Contains(x.Id));
        }

        (var designatedStandards, var paginationInfo) = pageNumber is not null
            ? await _designatedStandardRepository.PaginatedQueryAsync<DesignatedStandard>(designatedStandardPredicate, (int)pageNumber, searchTerm, pageSize)
            : (await _designatedStandardRepository.QueryAsync(x => x.LegislativeAreaId == legislativeAreaId), null);

        if (designatedStandards.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                DesignatedStandards = _mapper.Map<IEnumerable<DesignatedStandardModel>>(designatedStandards),
                PaginationInfo = paginationInfo
            };
        }

        var ppeCategory = await _ppeCategoryRepository.QueryAsync(x => x.LegislativeAreaId == legislativeAreaId);
        if (ppeCategory.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                PpeCategories = _mapper.Map<IEnumerable<PpeCategoryModel>>(ppeCategory)
            };
        }       

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPurposeOfAppointmentAsync(Guid purposeOfAppointmentId)
    {
        var categories = await _categoryRepository.QueryAsync(x => x.PurposeOfAppointmentId == purposeOfAppointmentId);
        if (categories.Any())
        {
            // Remove duplicate categories (those with subcategories will appear multiple times in the list).
            var distinctCategories = categories.GroupBy(x => x.Name).Select(x => x.First());
            
            return new ScopeOfAppointmentOptionsModel
            {
                Categories = _mapper.Map<IEnumerable<CategoryModel>>(distinctCategories).Distinct()
            };
        }

        var products = await _productRepository.QueryAsync(x => x.PurposeOfAppointmentId == purposeOfAppointmentId);
        if (products.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Products = _mapper.Map<IEnumerable<ProductModel>>(products)
            };
        }

        var procedures = await _procedureRepository.QueryAsync(x => x.PurposeOfAppointmentIds.Contains(purposeOfAppointmentId));
        if (procedures.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Procedures = _mapper.Map<IEnumerable<ProcedureModel>>(procedures)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForCategoryAsync(Guid categoryId)
    {        
        var subcategories = await _subCategoryRepository.QueryAsync(x => x.CategoryId == categoryId);

        if (subcategories.Count() > 1)
        {
            return new ScopeOfAppointmentOptionsModel
            {   
                Subcategories = _mapper.Map<IEnumerable<SubCategoryModel>>(subcategories)
            };
        }

        var products = await _productRepository.QueryAsync(x => x.CategoryId == categoryId);
        if (products.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Products = _mapper.Map<IEnumerable<ProductModel>>(products)
            };
        }

        var procedures = await _procedureRepository.QueryAsync(x => x.CategoryIds.Contains(categoryId));
        if (procedures.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Procedures = _mapper.Map<IEnumerable<ProcedureModel>>(procedures)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForSubCategoryAsync(Guid subCategoryId)
    {
        var products = await _productRepository.QueryAsync(x => x.SubCategoryId == subCategoryId);
        if (products.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Products = _mapper.Map<IEnumerable<ProductModel>>(products)
            };
        }
        return new ScopeOfAppointmentOptionsModel();
    }
    
    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForProductAsync(Guid productId)
    {
        var procedures = await _procedureRepository.QueryAsync(x => x.ProductIds.Contains(productId));
        if (procedures.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Procedures = _mapper.Map<IEnumerable<ProcedureModel>>(procedures)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPpeCategoryAsync(Guid ppeCategoryId)
    {
        var ppeProductTypes = await _ppeProductTypeRepository.QueryAsync(x => x.PpeCategoryId == ppeCategoryId);
        if (ppeProductTypes.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                PpeProductTypes = _mapper.Map<IEnumerable<PpeProductTypeModel>>(ppeProductTypes)
            };
        }

        var protectionAgainstRisks = await _protectionAgainstRiskRepository.QueryAsync(x => x.Id == ppeCategoryId);
        if (protectionAgainstRisks.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                ProtectionAgainstRisks = _mapper.Map<IEnumerable<ProtectionAgainstRiskModel>>(protectionAgainstRisks)
            };
        }

        var areaOfCompetencies = await _areaOfCompetencyRepository.QueryAsync(x => x.Id == ppeCategoryId);
        if (areaOfCompetencies.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                AreaOfCompetencies = _mapper.Map<IEnumerable<AreaOfCompetencyModel>>(areaOfCompetencies)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForPpeProductTypeAsync()
    {
        var protectionAgainstRisks = await _protectionAgainstRiskRepository.GetAllAsync();
        if (protectionAgainstRisks.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                ProtectionAgainstRisks = _mapper.Map<IEnumerable<ProtectionAgainstRiskModel>>(protectionAgainstRisks)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForProtectionAgainstRiskAsync()
    {
        var areaOfCompetencies = await _areaOfCompetencyRepository.GetAllAsync();
        if (areaOfCompetencies.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                AreaOfCompetencies = _mapper.Map<IEnumerable<AreaOfCompetencyModel>>(areaOfCompetencies)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForAreaOfCompetencyAsync(Guid areaOfCompetencyId)
    {
        var procedures = await _procedureRepository.QueryAsync(x => x.AreaOfCompetencyIds.Contains(areaOfCompetencyId));
        if (procedures.Any())
        {
            return new ScopeOfAppointmentOptionsModel
            {
                Procedures = _mapper.Map<IEnumerable<ProcedureModel>>(procedures)
            };
        }

        return new ScopeOfAppointmentOptionsModel();
    }
    public async Task<PurposeOfAppointmentModel?> GetPurposeOfAppointmentByIdAsync(Guid purposeOfAppointmentId)
    {
        Guard.IsTrue(purposeOfAppointmentId != Guid.Empty, "Guid cannot be empty");
        var pa = await _purposeOfAppointmentRepository.QueryAsync(l => l.Id == purposeOfAppointmentId);
        return _mapper.Map<PurposeOfAppointmentModel>(pa.FirstOrDefault());
    }

    public async Task<CategoryModel?> GetCategoryByIdAsync(Guid categoryId)
    {
        Guard.IsTrue(categoryId != Guid.Empty, "Guid cannot be empty");
        var cat = await _categoryRepository.QueryAsync(l => l.Id == categoryId);
        return _mapper.Map<CategoryModel>(cat.FirstOrDefault());
    }

    public async Task<ProductModel?> GetProductByIdAsync(Guid productId)
    {
        Guard.IsTrue(productId != Guid.Empty, "Guid cannot be empty");
        var prod = await _productRepository.QueryAsync(p => p.Id == productId);
        return _mapper.Map<ProductModel>(prod.FirstOrDefault());
    }

    public async Task<ProcedureModel?> GetProcedureByIdAsync(Guid procedureId)
    {
        Guard.IsTrue(procedureId != Guid.Empty, "Guid cannot be empty");
        var procedure = await _procedureRepository.QueryAsync(p => p.Id == procedureId);
        return _mapper.Map<ProcedureModel>(procedure.FirstOrDefault());
    }

    public async Task<SubCategoryModel?> GetSubCategoryByIdAsync(Guid subCategoryId)
    {
        Guard.IsTrue(subCategoryId != Guid.Empty, "Guid cannot be empty");
        var subCat = await _subCategoryRepository.QueryAsync(p => p.Id == subCategoryId);
        return _mapper.Map<SubCategoryModel>(subCat.FirstOrDefault());
    }

    public async Task<PpeCategoryModel?> GetPpeCategoryByIdAsync(Guid ppeCategoryId)
    {
        Guard.IsTrue(ppeCategoryId != Guid.Empty, "Guid cannot be empty");
        var ppeCategries = await _ppeCategoryRepository.QueryAsync(p => p.Id == ppeCategoryId);
        return _mapper.Map<PpeCategoryModel>(ppeCategries.FirstOrDefault());
    }
    
    public async Task<PpeProductTypeModel?> GetPpeProductTypeByIdAsync(Guid ppeProductTypeId)
    {
        Guard.IsTrue(ppeProductTypeId != Guid.Empty, "Guid cannot be empty");
        var ppeTypes = await _ppeProductTypeRepository.QueryAsync(p => p.Id == ppeProductTypeId);
        return _mapper.Map<PpeProductTypeModel>(ppeTypes.FirstOrDefault());
    }

    public async Task<ProtectionAgainstRiskModel?> GetProtectionAgainstRiskByIdAsync(Guid protectionAgainstRiskId)
    {
        Guard.IsTrue(protectionAgainstRiskId != Guid.Empty, "Guid cannot be empty");
        var parTypes = await _protectionAgainstRiskRepository.QueryAsync(p => p.Id == protectionAgainstRiskId);
        return _mapper.Map<ProtectionAgainstRiskModel>(parTypes.FirstOrDefault());
    }

    public async Task<AreaOfCompetencyModel?> GetAreaOfCompetencyByIdAsync(Guid areaOfCompetencyId)
    {
        Guard.IsTrue(areaOfCompetencyId != Guid.Empty, "Guid cannot be empty");
        var aOfCompetencies = await _areaOfCompetencyRepository.QueryAsync(p => p.Id == areaOfCompetencyId);
        return _mapper.Map<AreaOfCompetencyModel>(aOfCompetencies.FirstOrDefault());
    }
    public async Task<DesignatedStandardModel?> GetDesignatedStandardByIdAsync(Guid designatedStandardId)
    {
        Guard.IsTrue(designatedStandardId != Guid.Empty, "Guid cannot be empty");
        var designatedStandard = await _designatedStandardRepository.QueryAsync(p => p.Id == designatedStandardId);
        return _mapper.Map<DesignatedStandardModel>(designatedStandard.FirstOrDefault());
    }

    public async Task<List<DesignatedStandardModel>> GetDesignatedStandardsForDocumentAsync(Document document)
    {
        var designatedStandardIds = document.ScopeOfAppointments.SelectMany(soa => soa.DesignatedStandardIds).Distinct();
        foreach (var id in designatedStandardIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var designatedStandards = (await _designatedStandardRepository.QueryAsync(d => designatedStandardIds.Contains(d.Id)))
            .OrderBy(d => d.Name)
            .ToList();

        return _mapper.Map<List<DesignatedStandardModel>>(designatedStandards);
    }

    public async Task<List<LegislativeAreaModel>> GetLegislativeAreasForDocumentAsync(Document document)
    {
        var legislativeAreaIds = document.DocumentLegislativeAreas.Select(docLa => docLa.LegislativeAreaId);
        foreach (var id in legislativeAreaIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var la = (await _legislativeAreaRepository.QueryAsync(l => legislativeAreaIds.Contains(l.Id))).ToList();
        return _mapper.Map<List<LegislativeAreaModel>>(la);
    }

    public async Task<List<PurposeOfAppointmentModel>> GetPurposeOfAppointmentsForDocumentAsync(Document document)
    {
        var purposeOfAppointmentIds = document.ScopeOfAppointments.Where(soa => soa.PurposeOfAppointmentId.HasValue).Select(soa => soa.PurposeOfAppointmentId!.Value);
        foreach (var id in purposeOfAppointmentIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var purposeOfAppointments = (await _purposeOfAppointmentRepository.QueryAsync(l => purposeOfAppointmentIds.Contains(l.Id))).ToList();
        return _mapper.Map<List<PurposeOfAppointmentModel>>(purposeOfAppointments);
    }

    public async Task<List<CategoryModel>> GetCategoriesForDocumentAsync(Document document)
    {
        var categoryIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.CategoryIdAndProcedureIds
                .Where(categoryAndProcedures => categoryAndProcedures.CategoryId.HasValue)
                .Select(c => c.CategoryId))
            .Distinct();

        foreach (var id in categoryIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var cat = (await _categoryRepository.QueryAsync(l => categoryIds.Contains(l.Id))).ToList();
        return _mapper.Map<List<CategoryModel>>(cat);
    }

    public async Task<List<SubCategoryModel>> GetSubCategoriesForDocumentAsync(Document document)
    {
        var subCategoryIds = document.ScopeOfAppointments.Where(soa => soa.SubCategoryId.HasValue).Select(soa => soa.SubCategoryId!.Value);
        foreach (var id in subCategoryIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var subCategories = (await _subCategoryRepository.QueryAsync(p => subCategoryIds.Contains(p.Id))).ToList();
        return _mapper.Map<List<SubCategoryModel>>(subCategories);
    }

    public async Task<List<ProductModel>> GetProductsForDocumentAsync(Document document)
    {
        var productIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.ProductIdAndProcedureIds
                .Where(productAndProcedures => productAndProcedures.ProductId.HasValue)
                .Select(productIdAndProcedureIds => productIdAndProcedureIds.ProductId!.Value))
            .Distinct();

        foreach (var id in productIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var products = (await _productRepository.QueryAsync(p => productIds.Contains(p.Id))).ToList();
        return _mapper.Map<List<ProductModel>>(products);
    }

    public async Task<List<ProcedureModel>> GetProceduresForDocumentAsync(Document document)
    {
        var productProcedureIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.ProductIdAndProcedureIds
                .SelectMany(productIdAndProcedureIds => productIdAndProcedureIds.ProcedureIds));

        var categoryProcedureIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.CategoryIdAndProcedureIds
                .SelectMany(categoryIdAndProcedureIds => categoryIdAndProcedureIds.ProcedureIds));

        var ppeProductTypeProcedureIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.PpeProductTypeIdAndProcedureIds
                .SelectMany(ppeProductTypeIdAndProcedureIds => ppeProductTypeIdAndProcedureIds.ProcedureIds));

        var protectionAgainstRiskProcedureIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.ProtectionAgainstRiskIdAndProcedureIds
                .SelectMany(protectionAgainstRiskIdAndProcedureIds => protectionAgainstRiskIdAndProcedureIds.ProcedureIds));

        var areaOfCompetencyProcedureIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.AreaOfCompetencyIdAndProcedureIds
                .SelectMany(areaOfCompetencyIdAndProcedureIds => areaOfCompetencyIdAndProcedureIds.ProcedureIds));

        var distictProcedureIds = productProcedureIds.Concat(categoryProcedureIds)
            .Concat(ppeProductTypeProcedureIds)
            .Concat(protectionAgainstRiskProcedureIds)
            .Concat(areaOfCompetencyProcedureIds).Distinct();

        foreach (var id in distictProcedureIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var procedures = (await _procedureRepository.QueryAsync(p => distictProcedureIds.Contains(p.Id))).ToList();
        return _mapper.Map<List<ProcedureModel>>(procedures);
    }

    public async Task<List<PpeProductTypeModel>> GetPpeProductTypesForDocumentAsync(Document document)
    {
        return await GetEntitiesForDocumentAsync<PpeProductType, PpeProductTypeModel>(
            document,
            soa => soa.PpeProductTypeIdAndProcedureIds,
            x => x.PpeProductTypeId
        );
    }

    public async Task<List<ProtectionAgainstRiskModel>> GetProtectionAgainstRisksForDocumentAsync(Document document)
    {
        return await GetEntitiesForDocumentAsync<ProtectionAgainstRisk, ProtectionAgainstRiskModel>(
            document,
            soa => soa.ProtectionAgainstRiskIdAndProcedureIds,
            x => x.ProtectionAgainstRiskId
        );
    }
    public async Task<List<AreaOfCompetencyModel>> GetAreaOfCompetenciesForDocumentAsync(Document document)
    {
        var areaOfCompetencyIds = document.ScopeOfAppointments
            .SelectMany(soa => soa.AreaOfCompetencyIdAndProcedureIds
                .Where(areaOfCompetencyAndProcedures => areaOfCompetencyAndProcedures.AreaOfCompetencyId.HasValue)
                .Select(areaOfCompetencyAndProcedures => areaOfCompetencyAndProcedures.AreaOfCompetencyId!.Value))
            .Distinct();

        foreach (var id in areaOfCompetencyIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }
        var areaOfCompetencies = (await _areaOfCompetencyRepository.QueryAsync(p => areaOfCompetencyIds.Contains(p.Id))).ToList();
        return _mapper.Map<List<AreaOfCompetencyModel>>(areaOfCompetencies);       

    }

    public async Task<ScopeOfAppointmentOptionsModel> GetPpeProductTypeScopeOfAppointmentOptionsAsync()
    {
        var ppeProductTypes = await _ppeProductTypeRepository.GetAllAsync();
        return new ScopeOfAppointmentOptionsModel
        {
            PpeProductTypes = _mapper.Map<IEnumerable<PpeProductTypeModel>>(ppeProductTypes)
        };
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetProtectionAgainstRiskScopeOfAppointmentOptionsAsync()
    {
        var protectionAgainstRisks = await _protectionAgainstRiskRepository.GetAllAsync();
        return new ScopeOfAppointmentOptionsModel
        {
            ProtectionAgainstRisks = _mapper.Map<IEnumerable<ProtectionAgainstRiskModel>>(protectionAgainstRisks)
        };
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetAreaOfCompetencyScopeOfAppointmentOptionsAsync()
    {
        var areaOfCompetencies = await _areaOfCompetencyRepository.GetAllAsync();
        return new ScopeOfAppointmentOptionsModel
        {
            AreaOfCompetencies = _mapper.Map<IEnumerable<AreaOfCompetencyModel>>(areaOfCompetencies)
        };
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetPpeProcedureScopeOfAppointmentOptionsAsync()
    {
        var procedures = await _procedureRepository.QueryAsync(x => 
            x.PpeProductTypeIds != null);

        return new ScopeOfAppointmentOptionsModel
        {
            Procedures = _mapper.Map<IEnumerable<ProcedureModel>>(procedures)
        };
    }

    public async Task<List<TModel>> GetEntitiesForDocumentAsync<TEntity, TModel>(
        Document document,
        Func<DocumentScopeOfAppointment, IEnumerable<dynamic>> selector,
        Func<dynamic, Guid?> idSelector
    ) where TEntity : class, IEntity
    {
        IReadOnlyRepository<TEntity> repository = ResolveRepository<TEntity>();

        var entityIds = document.ScopeOfAppointments
            .SelectMany(selector)
            .Select(idSelector)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct();

        foreach (var id in entityIds)
        {
            Guard.IsTrue(id != Guid.Empty, "Guid cannot be empty");
        }

        var entities = (await repository.QueryAsync(e => entityIds.Contains(e.Id))).ToList();
        return _mapper.Map<List<TModel>>(entities);
    }

    private IReadOnlyRepository<TEntity> ResolveRepository<TEntity>() where TEntity : class
    {
        if (typeof(TEntity) == typeof(PpeProductType))
            return (IReadOnlyRepository<TEntity>)_ppeProductTypeRepository;
        if (typeof(TEntity) == typeof(ProtectionAgainstRisk))
            return (IReadOnlyRepository<TEntity>)_protectionAgainstRiskRepository;
        if (typeof(TEntity) == typeof(AreaOfCompetency))
            return (IReadOnlyRepository<TEntity>)_areaOfCompetencyRepository;

        throw new NotSupportedException($"Repository for type {typeof(TEntity).Name} is currently not supported.");
    }
}