
namespace mvcDapper3.Models.Tables;

public partial class dbEntities : DbContext
{
    public dbEntities(){}

    public dbEntities(DbContextOptions<dbEntities> options)
        : base(options){}

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optBuilder)
    {
        if (!optBuilder.IsConfigured)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true);
            IConfigurationRoot config = builder.Build();
            string connStrName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.ToLower() == "development"
                ? "dbconn"
                : "ProductionDbConn";
            var connStr = config.GetConnectionString("dbconn");
            optBuilder.UseSqlServer(connStr);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.HasIndex(e => e.UserNo, "IX_Users_no").IsClustered();

            entity.Property(e => e.ActivationToken).HasMaxLength(255);
            entity.Property(e => e.CalendarPreference).HasMaxLength(50);
            entity.Property(e => e.CodeNo).HasMaxLength(50);
            entity.Property(e => e.ContactAddress).HasMaxLength(250);
            entity.Property(e => e.ContactEmail).HasMaxLength(50);
            entity.Property(e => e.ContactTel).HasMaxLength(50);
            entity.Property(e => e.DeptNo).HasMaxLength(50);
            entity.Property(e => e.GenderCode).HasMaxLength(50);
            entity.Property(e => e.NotifyPassword).HasMaxLength(250);
            entity.Property(e => e.Password).HasMaxLength(250);
            entity.Property(e => e.Remark).HasMaxLength(250);
            entity.Property(e => e.RoleNo).HasMaxLength(50);
            entity.Property(e => e.TitleNo).HasMaxLength(50);
            entity.Property(e => e.TokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.UserNo).HasMaxLength(50);
            entity.Property(e => e.ValidateCode).HasMaxLength(250);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

// DB first
// generate by this command: 只轉換 dbo.Users 這個表
// dotnet ef dbcontext scaffold "Name=ConnectionStrings:dbconn" Microsoft.EntityFrameworkCore.SqlServer -n mvcDapper.Models -o Models/Tables --context-dir Models -c dbEntities -f --use-database-names --no-pluralize --table dbo.Users