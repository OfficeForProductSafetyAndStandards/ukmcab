using AutoMapper;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.Search.Models;
using YamlDotNet.Core.Events;

namespace UKMCAB.Core.Mappers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile() 
    {
        //------------------------------------------------------------------------------------------------------------------------------
        // Note : For complex mappings, consider putting them into their own XyzProfile.cs classes to prevent this file getting too big.
        //------------------------------------------------------------------------------------------------------------------------------

        CreateMap<LegislativeArea, LegislativeAreaModel>();
        CreateMap<PurposeOfAppointment, PurposeOfAppointmentModel>();
        CreateMap<Category, CategoryModel>();
        CreateMap<SubCategory, SubCategoryModel>();
        CreateMap<Product, ProductModel>();
        CreateMap<Procedure, ProcedureModel>();
        CreateMap<SubCategory, SubCategoryModel>();
        CreateMap<DocumentLegislativeArea, DocumentLegislativeAreaIndexItem>()
            .ForMember(s => s.Status, opt => opt.MapFrom(d => (int)d.Status));
        CreateMap<DesignatedStandard, DesignatedStandardModel>();
    }
}
