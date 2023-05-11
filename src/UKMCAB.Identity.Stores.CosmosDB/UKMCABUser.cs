using Microsoft.AspNetCore.Identity;

namespace UKMCAB.Identity.Stores.CosmosDB
{
    public class UKMCABUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Regulations { get; set; } = new List<string>();
        public string? RequestReason { get; set; }
        public bool RequestApproved { get; set; }
    }
}
