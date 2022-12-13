using Microsoft.AspNetCore.DataProtection;
using UKMCAB.Common;
using UKMCAB.Common.Security;
using UKMCAB.Core.Models.Account;

namespace UKMCAB.Core.Services.Account
{
    public class RegisterService : IRegisterService
    {
        private ITimeLimitedDataProtector _dataProtector;

        // Test constructor
        public RegisterService(ITimeLimitedDataProtector dataProtector)
        {
            _dataProtector = dataProtector;
        }
        
        public RegisterService(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(RegisterService)).ToTimeLimitedDataProtector();
        }

        public  string EncodeRegistrationDetails(RegistrationDTO registrationDto)
        {
            return _dataProtector.Protect(JsonBase64UrlToken.Serialize(registrationDto), TimeSpan.FromDays(2));
        }

        public RegistrationDTO DecodeRegistrationDetails(string code)
        {
            Rule.IsFalse(string.IsNullOrWhiteSpace(code), "Code value is required");

            return JsonBase64UrlToken.Deserialize<RegistrationDTO>(_dataProtector.Unprotect(code));
        }
    }
}
