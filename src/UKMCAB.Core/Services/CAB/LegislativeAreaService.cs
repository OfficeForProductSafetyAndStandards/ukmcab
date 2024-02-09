using AutoMapper;
using UKMCAB.Common;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.CosmosDb.Services;
using UKMCAB.Data.Models.LegislativeAreas;
using System.Linq;

namespace UKMCAB.Core.Services.CAB;

public class LegislativeAreaService : ILegislativeAreaService
{
    private readonly IReadOnlyRepository<LegislativeArea> _legislativeAreaRepository;
    private readonly IReadOnlyRepository<PurposeOfAppointment> _purposeOfAppointmentRepository;
    private readonly IReadOnlyRepository<Category> _categoryRepository;
    private readonly IReadOnlyRepository<SubCategory> _subCategoryRepository;
    private readonly IReadOnlyRepository<Product> _productRepository;
    private readonly IReadOnlyRepository<Procedure> _procedureRepository;
    private readonly IMapper _mapper;

    public LegislativeAreaService(
        IReadOnlyRepository<LegislativeArea> legislativeAreaRepository,
        IReadOnlyRepository<PurposeOfAppointment> purposeOfAppointmentRepository,
        IReadOnlyRepository<Category> categoryAreRepository,
        IReadOnlyRepository<Product> productRepository,
        IReadOnlyRepository<Procedure> procedureRepository,
        IReadOnlyRepository<SubCategory> subCategoryRepository,
    IMapper mapper)
    {
        _legislativeAreaRepository = legislativeAreaRepository;
        _purposeOfAppointmentRepository = purposeOfAppointmentRepository;
        _categoryRepository = categoryAreRepository;
        _productRepository = productRepository;
        _procedureRepository = procedureRepository;
        _subCategoryRepository = subCategoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LegislativeAreaModel>> GetAllLegislativeAreasAsync()
    {
        var result = await _legislativeAreaRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<LegislativeAreaModel>>(result);
    }

    public async Task<LegislativeAreaModel?> GetLegislativeAreaByIdAsync(Guid legislativeAreaId)
    {
        Guard.IsTrue(legislativeAreaId != Guid.Empty, "Guid cannot be empty");
        var la = await _legislativeAreaRepository.QueryAsync(l => l.Id == legislativeAreaId);
        return _mapper.Map<LegislativeAreaModel>(la.FirstOrDefault());
    }

    public async Task<ScopeOfAppointmentOptionsModel> GetNextScopeOfAppointmentOptionsForLegislativeAreaAsync(Guid legislativeAreaId)
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

    public async Task<ScopeOfAppointmentOptionsModel?> GetNextScopeOfAppointmentOptionsForProductAsync(Guid productId)
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

    public async Task<SubCategoryModel?> GetSubCategoryByIdAsync(Guid subCategoryId)
    {
        Guard.IsTrue(subCategoryId != Guid.Empty, "Guid cannot be empty");
        var cat = await _subCategoryRepository.QueryAsync(l => l.Id == subCategoryId);
        return _mapper.Map<SubCategoryModel>(cat.FirstOrDefault());
    }

    public async Task<IEnumerable<LegislativeAreaModel>> GetAvailableCabLegislativeAreas(List<Guid?> selectedLegislativeAreaIds)
    {
        var allLegislativeAreas = await this.GetAllLegislativeAreasAsync();
        return allLegislativeAreas.Where(n => !selectedLegislativeAreaIds.Contains(n.Id));
    }
}
