using UKMCAB.Core.Models.Account;

namespace UKMCAB.Core.Services.Account
{
    public interface IRegisterService
    {
        public string EncodeRegistrationDetails(RegistrationDTO registrationDto);
        public RegistrationDTO DecodeRegistrationDetails(string code);
    }
}
