using AutoMapper;
using UKMCAB.Core.Domain.LegislativeAreas;
using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Core.Mappers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile() 
    {
        //------------------------------------------------------------------------------------------------------------------------------
        // Note : For complex mappings, consider putting them into their own XyzProfile.cs classes to prevent this file getting too big.
        //------------------------------------------------------------------------------------------------------------------------------

        this.CreateMap<LegislativeArea, LegislativeAreaModel>();
        this.CreateMap<PurposeOfAppointment, PurposeOfAppointmentModel>();
        this.CreateMap<Category, CategoryModel>();
        this.CreateMap<Product, ProductModel>();
        this.CreateMap<Procedure, ProcedureModel>();
        this.CreateMap<SubCategory, SubCategoryModel>();
    }
}
