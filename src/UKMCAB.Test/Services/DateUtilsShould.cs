using Microsoft.AspNetCore.Mvc.ModelBinding;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKMCAB.Web.UI.Services;

namespace UKMCAB.Test.Services;

public class DateUtilsShould
{


    [Category("Date Validation Happy Path")]
    [Test]
    public void ReturnsCorrectAppointmentDate_When_AValidDateIsEntered()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "29";
        var month = "02";
        var year = "2020";
        var modelKey = "AppointmentDate";
        var errorMessagePart = "appointment";
        var date = $"{year}/{month}/{day}";
        DateTime.TryParse(date, out DateTime dateTime);

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.EqualTo(dateTime));
        Assert.That(modelState.ErrorCount, Is.Zero);
    }

    [Category("Date Validation Happy Path")]
    [Test]
    public void ReturnsCorrectReviewDate_When_AValidDateIsEntered()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "12";
        var month = "12";
        var year = "2024";
        var modelKey = "ReviewDate";
        var errorMessagePart = "review";
        var date = $"{year}/{month}/{day}";
        var aptDate = "2019/12/12";
        DateTime.TryParse(aptDate, out DateTime appointmentDate);
        DateTime.TryParse(date, out DateTime dateTime);

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart,appointmentDate);

        //Assert
        Assert.That(result, Is.EqualTo(dateTime));
        Assert.That(modelState.ErrorCount, Is.Zero);
    }

    [Category("Date Validation Happy Path")]
    [Test]
    public void ReturnsCorrectReviewtDate_When_AValidReviewDateIsEntered_And_No_AppointmentDateEntered()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "11";
        var month = "07";
        var year = "2028";
        var modelKey = "ReviewDate";
        var errorMessagePart = "review";
        var date = $"{year}/{month}/{day}";
        DateTime.TryParse(date, out DateTime dateTime);

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.EqualTo(dateTime));
        Assert.That(modelState.ErrorCount, Is.Zero);
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustBeInThePast_When_AFutureDateIsEntered()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "13";
        var month = "07";
        var year = "2024";
        var modelKey = "AppointmentDate";
        var errorMessagePart = "appointment";
        var errorMessage = "The appointment date must be in the past.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustBeInTheFuture_When_APastDateIsEntered()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "01";
        var month = "01";
        var year = "2023";
        var modelKey = "ReviewDate";
        var errorMessagePart = "review";
        var errorMessage = "The review date must be in the future.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustBeWithin5Years_When_GivenAnInvalidDate()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "01";
        var month = "01";
        var year = "2023";
        var modelKey = "ReviewDate";
        var errorMessagePart = "review";
        var errorMessage = "The review date must be in the future.";
        var aptDate = DateTime.Now.AddDays(-1);

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart,aptDate);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    
    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustBeARealDateError_When_AppointmentDateDayIsInvalid()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "33";
        var month = "05";
        var year = "2023";
        var modelKey = "AppointmentDate";
        var modelKeyDay = "AppointmentDateDay";        
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must be a real date.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey,errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage)); 
    }

    [Category("Date Validation")]
    [Test]
    public void Return2ErrorMessages_AppointmentDateMustBeARealDate()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "33";
        var month = "05";
        var year = "2023";
        var modelKey = "AppointmentDate";
        var modelKeyDay = "AppointmentDateDay";
        var errorMessagePart = "appointment";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(modelState.ErrorCount, Is.EqualTo(2)); // Error 1 for the day field and Error 2 for the AppointmentDate field
        Assert.That(modelState[modelKey].Errors.Count, Is.EqualTo(1));
        Assert.That(modelState[modelKeyDay].Errors.Count, Is.EqualTo(1));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustBeARealDateError_When_ReviewDateDayIsInvalid()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "03";
        var month = "13";
        var year = "2023";
        var modelKey = "ReviewDate";
        var modelKeyDay = "ReviewDateDay";
        var errorMessagePart = "review";
        var errorMessage = "Review date must be a real date.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void Returns2Errors_When_ReviewDateDayIsInvalid()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "03";
        var month = "13";
        var year = "2023";
        var modelKey = "ReviewDate";
        var modelKeyDay = "ReviewDateDay";
        var modelKeyMonth = "ReviewDateMonth";
        var errorMessagePart = "review";
        var errorMessage = "Review date must be a real date.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState.ErrorCount, Is.EqualTo(2)); // 2 Errors for the day & month fields and 1 Error for the AppointmentDate field
    }


    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustBeARealDateError_When_29FebIsEnteredInANonLeapYear()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "29";
        var month = "02";
        var year = "2025";
        var modelKey = "ReviewDate";
        var modelKeyDay = "ReviewDateDay";
        var errorMessagePart = "review";
        var errorMessage = "Review date must be a real date.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustIncludeMonthAndYear_When_Both_Are_Empty_Strings()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "29";
        var month = string.Empty;
        var year = string.Empty;
        var modelKey = "AppointmentDate";
        var modelKeyMonth = "AppointmentDateMonth";
        var modelKeyYear = "AppointmentDateYear";
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must include a month and year.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState.ContainsKey(modelKeyYear));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustIncludeMonthAndYear_When_Both_Are_Empty_Strings()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "29";
        var month = string.Empty;
        var year = string.Empty;
        var modelKey = "ReviewDate";
        var modelKeyMonth = "ReviewDateMonth";
        var modelKeyYear = "ReviewDateYear";
        var errorMessagePart = "review";
        var errorMessage = "Review date must include a month and year.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState.ContainsKey(modelKeyYear));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustIncludeDayAndYear_When_Both_Are_Empty_Strings()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = string.Empty;
        var month = "2";
        var year = string.Empty;
        var modelKey = "AppointmentDate";
        var modelKeyDay = "AppointmentDateDay";
        var modelKeyYear = "AppointmentDateYear";
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must include a day and year.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));
        Assert.That(modelState.ContainsKey(modelKeyYear));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustIncludeDayAndYear_When_Both_Are_Empty_Strings()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = string.Empty;
        var month = "12";
        var year = string.Empty;
        var modelKey = "ReviewDate";
        var modelKeyDay = "ReviewDateDay";
        var modelKeyYear = "ReviewDateYear";
        var errorMessagePart = "review";
        var errorMessage = "Review date must include a day and year.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));
        Assert.That(modelState.ContainsKey(modelKeyYear));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustIncludeDayAndMonth_When_Both_Are_Empty_Strings()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = string.Empty;
        var month = string.Empty;
        var year = "2021";
        var modelKey = "AppointmentDate";
        var modelKeyDay = "AppointmentDateDay";
        var modelKeyMonth = "AppointmentDateMonth";
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must include a day and month.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustIncludeDayAndMonth_When_Both_Are_Empty_Strings()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = string.Empty;
        var month = string.Empty;
        var year = "2025";
        var modelKey = "ReviewDate";
        var modelKeyDay = "ReviewDateDay";
        var modelKeyMonth = "ReviewDateMonth";
        var errorMessagePart = "review";
        var errorMessage = "Review date must include a day and month.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustIncludeDay_When_Day_Is_An_Empty_String()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = string.Empty;
        var month = "07";
        var year = "2021";
        var modelKey = "AppointmentDate";
        var modelKeyDay = "AppointmentDateDay";
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must include a day.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustIncludeDay_When_Day_Is_An_Empty_String()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = string.Empty;
        var month = "5";
        var year = "2025";
        var modelKey = "ReviewDate";
        var modelKeyDay = "ReviewDateDay";
        var errorMessagePart = "review";
        var errorMessage = "Review date must include a day.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyDay));    
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustIncludeMonth_When_Month_Is_An_Empty_String()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "15";
        var month = string.Empty;
        var year = "2021";
        var modelKey = "AppointmentDate";
        var modelKeyMonth = "AppointmentDateMonth";
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must include a month.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustIncludedMonth_When_Month_Is_An_Empty_String()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "20";
        var month = string.Empty;
        var year = "2025";
        var modelKey = "ReviewDate";
        var modelKeyMonth = "ReviewDateMonth";
        var errorMessagePart = "review";
        var errorMessage = "Review date must include a month.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyMonth));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }


    [Category("Date Validation")]
    [Test]
    public void ReturnsAppointmentDateMustIncludeYear_When_Year_Is_An_Empty_String()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "15";
        var month = "05";
        var year = string.Empty; ;
        var modelKey = "AppointmentDate";
        var modelKeyYear = "AppointmentDateYear";
        var errorMessagePart = "appointment";
        var errorMessage = "Appointment date must include a year.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyYear));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

    [Category("Date Validation")]
    [Test]
    public void ReturnsReviewDateMustIncludedYear_When_Year_Is_An_Empty_String()
    {
        // Arrange
        var modelState = new ModelStateDictionary();
        var day = "20";
        var month = "11";
        var year = string.Empty;
        var modelKey = "ReviewDate";
        var modelKeyYear = "ReviewDateYear";
        var errorMessagePart = "review";
        var errorMessage = "Review date must include a year.";

        //Act
        var result = DateUtils.CheckDate(modelState, day, month, year, modelKey, errorMessagePart);

        //Assert
        Assert.That(result, Is.Null);
        Assert.That(modelState.ContainsKey(modelKey));
        Assert.That(modelState.ContainsKey(modelKeyYear));
        Assert.That(modelState[modelKey].Errors[0].ErrorMessage, Is.EqualTo(errorMessage));
    }

}
