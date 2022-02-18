namespace DescriptionModel.oa {
    using DescriptionModel.Simulate;
    using Microsoft.EntityFrameworkCore;

    public class OaContext : DbContext {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<R_ComanyDepartment> R_ComanyDepartments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companys { get; set; }
        public OaContext() {
            new OaGenerator(this);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder op) {
            //op.UseNpgsql("Server=47.75.130.231;Port=5432;User Id=postgres;Password=257@admin;Database=testdb");
            //op.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=SSPI;Database=testdb");
            //op.UseSqlite("Data Source=./testdb.sqlite");
            //op.UseMySQL("Server=47.75.130.231;Port=5432;User Id=postgres;Password=257@admin;Database=testdb");
            op.UseInMemoryDatabase("testdb");
        }
        protected override void OnModelCreating(ModelBuilder mb) {
            var employeeConfig = mb.Entity<Employee>();
            employeeConfig.Property(x => x.RoleTitle).HasColumnName("RoleTitle").HasColumnType("nvarchar(50)").IsRequired(false);
            employeeConfig.Property(x => x.RoleDescribtion).HasColumnName("RoleDescribtion").HasColumnType("nvarchar(200)").IsRequired(false);
            employeeConfig.Property(x => x.OwnerCompany).HasColumnName("OwnerCompany").HasColumnType("int").IsRequired(false);
            employeeConfig.Property(x => x.OwnerDepartment).HasColumnName("OwnerDepartment ").HasColumnType("int").IsRequired(false);
            employeeConfig.HasKey(x => x.Id);
            employeeConfig.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(20)").IsRequired(false);
            employeeConfig.Property(x => x.Pwd).HasColumnName("Pwd").HasColumnType("varchar(100)").IsRequired(false);
            employeeConfig.Property(x => x.Phone).HasColumnName("Phone").HasColumnType("varchar(20)").IsRequired(false);
            employeeConfig.Property(x => x.Email).HasColumnName("Email").HasColumnType("varchar(50)").IsRequired(false);
            employeeConfig.Property(x => x.IdentityCode).HasColumnName("IdentityCode").HasColumnType("varchar(50)").IsRequired(false);
            employeeConfig.Property(x => x.Sex).HasColumnName("Sex").HasColumnType("tinyint").IsRequired(false);
            employeeConfig.Property(x => x.Address).HasColumnName("Address").HasColumnType("nvarchar(100)").IsRequired(false);
            var departmentConfig = mb.Entity<Department>();
            departmentConfig.HasKey(x => x.Id);
            departmentConfig.Property(x => x.Pid).HasColumnName("Pid").HasColumnType("int").IsRequired(false);
            departmentConfig.Property(x => x.Summary).HasColumnName("Summary").HasColumnType("nvarchar(200)").IsRequired(false);
            departmentConfig.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(20)").IsRequired(false);
            var r_comanydepartmentConfig = mb.Entity<R_ComanyDepartment>();
            r_comanydepartmentConfig.HasKey(x => x.Id);
            r_comanydepartmentConfig.Property(x => x.Company).HasColumnName("Company").HasColumnType("int").IsRequired(false);
            r_comanydepartmentConfig.Property(x => x.Demaprtment).HasColumnName("Demaprtment").HasColumnType("int").IsRequired(false);
            var productConfig = mb.Entity<Product>();
            productConfig.HasKey(x => x.Id);
            productConfig.Property(x => x.Pid).HasColumnName("Pid").HasColumnType("int").IsRequired(false);
            productConfig.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(20)").IsRequired(false);
            productConfig.Property(x => x.Summary).HasColumnName("Summary").HasColumnType("nvarchar(200)").IsRequired(false);
            productConfig.Property(x => x.Code).HasColumnName("Code").HasColumnType("varchar(50)").IsRequired(false);
            productConfig.Property(x => x.Price).HasColumnName("Price").HasColumnType("decimal").IsRequired(false);
            productConfig.Property(x => x.OwnerCompany).HasColumnName("OwnerCompany").HasColumnType("int").IsRequired(false);
            var companyConfig = mb.Entity<Company>();
            companyConfig.HasKey(x => x.Id);
            companyConfig.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(20)").IsRequired(false);
            companyConfig.Property(x => x.Address).HasColumnName("Address").HasColumnType("nvarchar(100)").IsRequired(false);
            companyConfig.Property(x => x.OfficialWebsite).HasColumnName("OfficialWebsite").HasColumnType("varchar(50)").IsRequired(false);
            companyConfig.Property(x => x.RegisteredCapital).HasColumnName("RegisteredCapital").HasColumnType("float").IsRequired(false);
            companyConfig.Property(x => x.Industry).HasColumnName("Industry").HasColumnType("varchar(50)").IsRequired(false);
            companyConfig.Property(x => x.Telphone).HasColumnName("Telphone").HasColumnType("varchar(20)").IsRequired(false);
        }
    }
}