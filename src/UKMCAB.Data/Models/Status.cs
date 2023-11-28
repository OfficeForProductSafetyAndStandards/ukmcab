namespace UKMCAB.Data.Models
{
    public enum Status
    {
        Unknown = 0,
        Draft = 20,
        Published = 30,
        Archived = 40,
        Historical = 50,
    }

    public enum SubStatus
    {
        None,
        PendingApproval
    }
}
