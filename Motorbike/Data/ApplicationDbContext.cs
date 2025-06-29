using Microsoft.EntityFrameworkCore;
using Motorbike.Models;
using System.Diagnostics;

namespace Motorbike.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, 
                                   ILogger<ApplicationDbContext> logger = null) 
            : base(options)
        {
            _logger = logger;
        }
        
        public DbSet<Models.Motorbike> Motorbikes { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        // Thêm DbSet cho Contact
        public DbSet<Contact> Contact { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Account>()
            .HasOne(a => a.UserRole)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId);
            // Cấu hình Brand là bảng motorbike_brand
            modelBuilder.Entity<Brand>().ToTable("motorbike_brand");

            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderId, od.MotorbikeId });

            // Cấu hình cho OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => od.DetailId);
                
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.DetailId)
                .ValueGeneratedOnAdd();
                
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Motorbike)
                .WithMany()
                .HasForeignKey(od => od.MotorbikeId);
    
            // Quan trọng: Map tên cột trong database
            modelBuilder.Entity<OrderDetail>()
                .ToTable("order_details", t => t.ExcludeFromMigrations())
                .Property(od => od.DetailId).HasColumnName("detail_id");
    
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.OrderId).HasColumnName("order_id");
    
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.MotorbikeId).HasColumnName("motorbike_id");
    
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Quantity).HasColumnName("quantity");
    
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice).HasColumnName("unit_price");

            // Cấu hình rõ ràng cho Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("account");
                entity.HasKey(e => e.AccountId);
                entity.Property(e => e.AccountId).HasColumnName("account_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Username).HasColumnName("username").IsRequired();
                entity.Property(e => e.Password).HasColumnName("password").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
            });

            // Cấu hình tên bảng nếu cần
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<Contact>().ToTable("contact");
            
            // Cấu hình khóa ngoại
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Account)
                .WithMany()
                .HasForeignKey(o => o.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cấu hình chi tiết cho Contact
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("contact");
                entity.HasKey(e => e.ContactId);
                entity.Property(e => e.ContactId).HasColumnName("contact_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.Subject).HasColumnName("subject").IsRequired();
                entity.Property(e => e.Message).HasColumnName("message").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.Response).HasColumnName("response");
                entity.Property(e => e.ResponseAt).HasColumnName("response_at");
            });

            modelBuilder.Entity<Models.Motorbike>()
        .Property(m => m.Description)
        .HasColumnName("description")
        .HasColumnType("nvarchar(max)");
        }

        // Ghi đè SaveChanges để log thông tin chi tiết hơn
        public override int SaveChanges()
        {
            try
            {
                var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
                foreach (var entry in entries)
                {
                    _logger?.LogInformation($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                    foreach (var prop in entry.Properties)
                    {
                        _logger?.LogInformation($"Property: {prop.Metadata.Name}, IsModified: {prop.IsModified}, Value: {prop.CurrentValue}");
                    }
                }
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in SaveChanges");
                throw;
            }
        }
        
        // Ghi đè SaveChangesAsync để log thông tin chi tiết hơn
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
                foreach (var entry in entries)
                {
                    _logger?.LogInformation($"Saving Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                    
                    // Kiểm tra xem entity có property AccountId không
                    var accountIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "AccountId");
                    if (accountIdProperty != null)
                    {
                        _logger?.LogInformation($"EntityId: {accountIdProperty.CurrentValue}");
                    }
                    
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            _logger?.LogInformation($"Property changed: {prop.Metadata.Name}, OldValue: {prop.OriginalValue}, NewValue: {prop.CurrentValue}");
                        }
                    }
                }
                return base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in SaveChangesAsync with details: {Message}", ex.Message);
                throw;
            }
        }
    }
}