using UKMCAB.Common;
using UKMCAB.Core.Domain;
using UKMCAB.Core.Domain.CAB;
using UKMCAB.Data.Models;
using FileUpload = UKMCAB.Core.Domain.FileUpload;

namespace UKMCAB.Core.Mappers;
internal static class CabModelMapper
{
    public static CabModel MapToCabModel(this Document source)
    {
        var dest = new CabModel();
        var supportingDocuments = new List<FileUpload>();
        source.Documents?.ForEach(d => supportingDocuments.Add(new FileUpload(d.Label, d.LegislativeArea, d.Category, d.FileName, d.BlobName, d.UploadDateTime)));
        var schedules = new List<FileUpload>();
        source.Schedules?.ForEach(s =>
            schedules.Add(new FileUpload(s.Label, s.LegislativeArea, s.Category, s.FileName, s.BlobName,
                s.UploadDateTime)));
        
        dest.Id = source.id.ToGuid() ?? throw new Exception($"{nameof(source.id)} is not a guid (value:{source.id})");
        dest.Address = new GeoAddress(source.AddressLine1, source.AddressLine2, source.TownCity, source.County,
            source.Postcode, source.Country);
        dest.AppointmentDate = source.AppointmentDate;
        dest.BodyTypes = source.BodyTypes;
        dest.CABId = source.CABId.ToGuid() ?? throw new Exception($"{nameof(source.CABId)} is not a guid (value:{source.CABId})");
        dest.CabNumber = source.CABNumber;
        dest.CabNumberVisibility = source.CabNumberVisibility;
        dest.OrganisationContactDetails = new OrganisationContactDetails(source.Website, source.Email, source.Phone);
        dest.HiddenText = source.HiddenText;
        dest.LastUpdatedUtc = source.LastUpdatedDate;
        dest.LegislativeAreas = source.LegislativeAreas;
        dest.Name = source.Name;
        dest.PointOfContact = new PointOfContact(source.PointOfContactName, source.PointOfContactEmail, source.PointOfContactPhone, source.IsPointOfContactPublicDisplay);
        dest.RandomSort = source.RandomSort;
        dest.RegisteredOfficeLocation = source.RegisteredOfficeLocation;
        dest.RenewalDate = source.RenewalDate;
        dest.Schedules = schedules;
        dest.StatusValue = source.StatusValue;
        dest.SubStatus = source.SubStatus;
        dest.SupportingDocuments = supportingDocuments;
        dest.TestingLocations = source.TestingLocations;
        dest.UKASReference = source.UKASReference;
        return dest;
    }

    public static Document MapToDocument(this CabModel source)
    {
        var dest = new Document
        {
            id = (source.Id == Guid.Empty ? null : source.Id.ToString()) ?? throw new InvalidOperationException()
        };
        if (source.Address != null)
        {
            dest.AddressLine1 = source.Address.AddressLine1;
            dest.AddressLine2 = source.Address.AddressLine2;
            dest.TownCity = source.Address.TownCity;
            dest.County = source.Address.County;
            dest.Postcode = source.Address.PostCode;
            dest.Country = source.Address.Country;
        }

        dest.AppointmentDate = source.AppointmentDate;
        dest.BodyTypes = source.BodyTypes ;
        dest.CABId = (source.CABId == Guid.Empty ? null : source.CABId.ToString()) ?? throw new InvalidOperationException();
        dest.CABNumber = source.CabNumber;
        dest.CabNumberVisibility = source.CabNumberVisibility;
        if (source.OrganisationContactDetails != null)
        {
            dest.Website = source.OrganisationContactDetails.Website;
            dest.Phone = source.OrganisationContactDetails.Phone;
            dest.Email = source.OrganisationContactDetails.Email;
        }

        if (source.HiddenText != null) dest.HiddenText = source.HiddenText;
        dest.LegislativeAreas = source.LegislativeAreas;
        if (source.Name != null) dest.Name = source.Name;
        if (source.PointOfContact != null)
        {
            dest.PointOfContactName = source.PointOfContact.Name;
            dest.PointOfContactEmail = source.PointOfContact.Email;
            dest.PointOfContactPhone = source.PointOfContact.Phone;
            dest.IsPointOfContactPublicDisplay = source.PointOfContact.IsPublicDisplay;
        }

        if (source.RandomSort != null) dest.RandomSort = source.RandomSort;
        if (source.RegisteredOfficeLocation != null) dest.RegisteredOfficeLocation = source.RegisteredOfficeLocation;
        dest.RenewalDate = source.RenewalDate;
        if (source.Schedules.Any())
        {
            foreach (var fileUpload in source.Schedules)
            {
                dest.Schedules?.Add(new Data.Models.FileUpload
                {
                    Category = fileUpload.Category,
                    BlobName = fileUpload.BlobName,
                    UploadDateTime = fileUpload.UploadDateTime,
                    Label = fileUpload.Label,
                    FileName = fileUpload.FileName,
                    LegislativeArea = fileUpload.LegislativeArea
                });
                
            }
        }
        dest.StatusValue = source.StatusValue;
        dest.SubStatus = source.SubStatus;
        if (source.SupportingDocuments.Any())
        {
            foreach (var fileUpload in source.SupportingDocuments)
            {
                dest.Documents?.Add(new Data.Models.FileUpload
                {
                    Category = fileUpload.Category,
                    BlobName = fileUpload.BlobName,
                    UploadDateTime = fileUpload.UploadDateTime,
                    Label = fileUpload.Label,
                    FileName = fileUpload.FileName,
                    LegislativeArea = fileUpload.LegislativeArea
                });
            }
        }

        dest.TestingLocations = source.TestingLocations;
        if (source.UKASReference != null) dest.UKASReference = source.UKASReference;
        return dest;
    }

}