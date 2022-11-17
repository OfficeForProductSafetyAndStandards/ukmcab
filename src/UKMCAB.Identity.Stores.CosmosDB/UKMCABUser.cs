using Microsoft.AspNetCore.Identity;

namespace UKMCAB.Identity.Stores.CosmosDB
{
    public class UKMCABUser : IdentityUser
    {
        public List<string> Regulations { get; set; }
        public string RequestReason { get; set; }
        public bool RequestApproved { get; set; }
    }
}
