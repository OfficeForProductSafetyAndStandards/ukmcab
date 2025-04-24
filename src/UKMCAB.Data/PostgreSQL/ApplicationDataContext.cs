using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UKMCAB.Data.Models;
using UKMCAB.Data.Models.LegislativeAreas;
using UKMCAB.Data.Models.Users;

namespace UKMCAB.Data.PostgreSQL;

public class ApplicationDataContext : DbContext
{
    public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options)
    : base(options) { }

    public DbSet<LegislativeArea> LegislativeAreas => Set<LegislativeArea>();
    public DbSet<PurposeOfAppointment> PurposeOfAppointments => Set<PurposeOfAppointment>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Procedure> Procedures => Set<Procedure>();
    public DbSet<SubCategory> SubCategories => Set<SubCategory>();
    public DbSet<DesignatedStandard> DesignatedStandards => Set<DesignatedStandard>();
    public DbSet<PpeCategory> PpeCategories => Set<PpeCategory>();
    public DbSet<PpeProductType> PpeProductTypes => Set<PpeProductType>();
    public DbSet<ProtectionAgainstRisk> ProtectionAgainstRisks => Set<ProtectionAgainstRisk>();
    public DbSet<AreaOfCompetency> AreaOfCompetencies => Set<AreaOfCompetency>();
    public DbSet<Models.Workflow.WorkflowTask> WorkflowTasks => Set<Models.Workflow.WorkflowTask>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<UserAccountRequest> UserAccountRequests => Set<UserAccountRequest>();
    public DbSet<CABDocumentBlob> Documents => Set<CABDocumentBlob>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
