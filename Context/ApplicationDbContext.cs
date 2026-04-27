using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Exceptions;
using WorkOrderApp.Helpers.Paging;
using System.Linq.Expressions;

namespace WorkOrderApp.AppDbContext
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;
        private readonly IMapper _mapper;

        // ── Core ──────────────────────────────────────────────────────────────
        public DbSet<User>     Users     => Set<User>();
        public DbSet<Location> Locations => Set<Location>();

        public DbSet<UserLocation> UserLocations => Set<UserLocation>();

        // ── MMS domain ────────────────────────────────────────────────────────
        public DbSet<Request>      Requests      => Set<Request>();
        public DbSet<RequestLog>   RequestLogs   => Set<RequestLog>();
        public DbSet<WorkOrder>    WorkOrders    => Set<WorkOrder>();
        public DbSet<WorkOrderLog> WorkOrderLogs => Set<WorkOrderLog>();
        public DbSet<Cost>         Costs         => Set<Cost>();
        public DbSet<Part>         Parts         => Set<Part>();
        public DbSet<Attachment>   Attachments   => Set<Attachment>();

        // ── BaseEnum subtypes (TPC — each gets its own table) ─────────────────
        public DbSet<Currency>           Currencies           => Set<Currency>();
        public DbSet<FeeType>            FeeTypes             => Set<FeeType>();
        public DbSet<IdentificationType> IdentificationTypes  => Set<IdentificationType>();
        public DbSet<PaymentType>        PaymentTypes         => Set<PaymentType>();
        public DbSet<LeaseType>          LeaseTypes           => Set<LeaseType>();
        public DbSet<Appliance>          Appliances           => Set<Appliance>();
        public DbSet<PropertyType>       PropertyTypes        => Set<PropertyType>();
        public DbSet<Role>               Roles                => Set<Role>();

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ILogger<ApplicationDbContext> logger,
            IMapper mapper)
            : base(options)
        {
            _logger = logger;
            _mapper = mapper;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Global UTC converter for all DateTimeOffset columns ────────────
            var keepUtc = new ValueConverter<DateTimeOffset, DateTimeOffset>(
                v => new DateTimeOffset(v.DateTime, TimeSpan.Zero),
                v => new DateTimeOffset(v.DateTime, TimeSpan.Zero));
            var keepUtcNullable = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
                v => v.HasValue ? new DateTimeOffset(v.Value.DateTime, TimeSpan.Zero) : v,
                v => v.HasValue ? new DateTimeOffset(v.Value.DateTime, TimeSpan.Zero) : v);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
                foreach (var prop in entity.GetProperties())
                {
                    if (prop.ClrType == typeof(DateTimeOffset))        prop.SetValueConverter(keepUtc);
                    else if (prop.ClrType == typeof(DateTimeOffset?))  prop.SetValueConverter(keepUtcNullable);
                }

            // ── TPC strategies ────────────────────────────────────────────────
            modelBuilder.Entity<BaseEntity>().UseTpcMappingStrategy();
            modelBuilder.Entity<BaseEnum>().UseTpcMappingStrategy();

            // ── Sequences for auto-numbered entities ──────────────────────────
            modelBuilder.HasSequence<int>("request_number_seq").StartsAt(1000).IncrementsBy(1);
            modelBuilder.HasSequence<int>("work_order_number_seq").StartsAt(1000).IncrementsBy(1);

            modelBuilder.Entity<Request>()
                .Property(r => r.Number)
                .HasDefaultValueSql("nextval('request_number_seq')")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<WorkOrder>()
                .Property(wo => wo.Number)
                .HasDefaultValueSql("nextval('work_order_number_seq')")
                .ValueGeneratedOnAdd();

            // ── Enum → string storage ─────────────────────────────────────────
            modelBuilder.Entity<Request>()
                .Property(r => r.Status).HasConversion<string>();
            modelBuilder.Entity<Request>()
                .Property(r => r.Priority).HasConversion<string>();
            modelBuilder.Entity<WorkOrder>()
                .Property(wo => wo.Status).HasConversion<string>();
            modelBuilder.Entity<WorkOrder>()
                .Property(wo => wo.Priority).HasConversion<string>();

            // ── User.Settings → jsonb ─────────────────────────────────────────
            modelBuilder.Entity<User>()
                .Property(u => u.Settings)
                .HasColumnType("jsonb");

            // ── Unique indexes ────────────────────────────────────────────────
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Request>()
                .HasIndex(r => r.Number).IsUnique();
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => wo.Number).IsUnique();
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => wo.RequestId).IsUnique();

            // ── Query indexes ─────────────────────────────────────────────────
            modelBuilder.Entity<Request>()
                .HasIndex(r => r.Status);
            modelBuilder.Entity<Request>()
                .HasIndex(r => r.LocationId);
            modelBuilder.Entity<Request>()
                .HasIndex(r => r.RequestedById);
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => wo.Status);
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => wo.LocationId);
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => wo.AssignedToId);
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => wo.CustomerId);
            modelBuilder.Entity<Cost>()
                .HasIndex(c => c.WorkOrderId);
            modelBuilder.Entity<Part>()
                .HasIndex(p => p.WorkOrderId);
            modelBuilder.Entity<Part>()
                .HasIndex(p => p.QRCode);
            modelBuilder.Entity<RequestLog>()
                .HasIndex(l => l.RequestId);
            modelBuilder.Entity<WorkOrderLog>()
                .HasIndex(l => l.WorkOrderId);

            // ── UserLocation (M:M join) ───────────────────────────────────────
            modelBuilder.Entity<UserLocation>()
                .HasKey(ul => new { ul.UserId, ul.LocationId });

            modelBuilder.Entity<UserLocation>()
                .HasOne(ul => ul.User)
                .WithMany(u => u.UserLocations)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLocation>()
                .HasOne(ul => ul.Location)
                .WithMany(l => l.UserLocations)
                .HasForeignKey(ul => ul.LocationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Request relationships ─────────────────────────────────────────
            modelBuilder.Entity<Request>()
                .HasOne(r => r.Location)
                .WithMany()
                .HasForeignKey(r => r.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.RequestedBy)
                .WithMany()
                .HasForeignKey(r => r.RequestedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasMany(r => r.Attachments)
                .WithOne(a => a.Request)
                .HasForeignKey(a => a.RequestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Request>()
                .HasMany(r => r.Logs)
                .WithOne(l => l.Request)
                .HasForeignKey(l => l.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── WorkOrder relationships ────────────────────────────────────────
            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.Request)
                .WithOne(r => r.WorkOrder)
                .HasForeignKey<WorkOrder>(wo => wo.RequestId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.Customer)
                .WithMany()
                .HasForeignKey(wo => wo.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.Location)
                .WithMany()
                .HasForeignKey(wo => wo.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.AssignedTo)
                .WithMany()
                .HasForeignKey(wo => wo.AssignedToId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.AssignedBy)
                .WithMany()
                .HasForeignKey(wo => wo.AssignedById)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(wo => wo.Costs)
                .WithOne(c => c.WorkOrder)
                .HasForeignKey(c => c.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(wo => wo.Parts)
                .WithOne(p => p.WorkOrder)
                .HasForeignKey(p => p.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(wo => wo.Attachments)
                .WithOne(a => a.WorkOrder)
                .HasForeignKey(a => a.WorkOrderId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkOrder>()
                .HasMany(wo => wo.Logs)
                .WithOne(l => l.WorkOrder)
                .HasForeignKey(l => l.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Cost / Part → User (executor) ─────────────────────────────────
            modelBuilder.Entity<Cost>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Part>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── RequestLog / WorkOrderLog → User ──────────────────────────────
            modelBuilder.Entity<RequestLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkOrderLog>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

        }

        // ── SaveChanges overrides ─────────────────────────────────────────────

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            try { return base.SaveChanges(acceptAllChangesOnSuccess); }
            catch (Exception ex) { throw new ArgumentException(ex.Message); }
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            try { return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveChanges failed");
                throw new InternalErrorException();
            }
        }

        private void OnBeforeSaving()
        {
            var now = (DateTimeOffset)DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Metadata.FindProperty("CreatedAt") == null ||
                    entry.Metadata.FindProperty("UpdatedAt") == null)
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property("CreatedAt").CurrentValue = now;
                        entry.Property("UpdatedAt").CurrentValue = now;
                        break;
                    case EntityState.Modified:
                        entry.Property("CreatedAt").IsModified   = false;
                        entry.Property("UpdatedAt").CurrentValue = now;
                        break;
                }
            }
        }

        // ── Generic CRUD ──────────────────────────────────────────────────────

        public async Task<int> CreateAsync<T>(T data) where T : BaseEntity
        {
            Set<T>().Add(data);
            return await SaveChangesAsync();
        }

        public async Task<int> UpdateAsync<T>(T data) where T : BaseEntity
        {
            Set<T>().Update(data);
            return await SaveChangesAsync();
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : BaseEntity
        {
            Set<T>().Remove(entity);
            return await SaveChangesAsync();
        }

        public async Task<int> ActivateAsync<T>(string id) where T : BaseEntity
        {
            var entity = await Set<T>().FirstOrDefaultAsync(i => i.Id == id)
                         ?? throw new NotFoundException();
            entity.Active = true;
            Set<T>().Update(entity);
            return await SaveChangesAsync();
        }

        public async Task<int> DeactivateAsync<T>(string id) where T : BaseEntity
        {
            var entity = await Set<T>().FirstOrDefaultAsync(i => i.Id == id)
                         ?? throw new NotFoundException();
            entity.Active = false;
            Set<T>().Update(entity);
            return await SaveChangesAsync();
        }

        // ── Generic reads ─────────────────────────────────────────────────────

        public async Task<T> GetByIdAsync<T>(string id, params Expression<Func<T, object>>[] includes)
            where T : BaseEntity
        {
            IQueryable<T> query = Set<T>();
            foreach (var inc in includes) query = query.Include(inc);
            return await query.FirstOrDefaultAsync(i => i.Id == id)
                   ?? throw new NotFoundException($"{typeof(T).Name} not found");
        }

        public async Task<T?> GetEntityByFieldAsync<T>(string fieldName, string value)
            where T : BaseEntity
        {
            var param    = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, fieldName);
            var constant = Expression.Constant(value);
            var equal    = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda   = Expression.Lambda<Func<T, bool>>(equal, param);
            return await Set<T>().FirstOrDefaultAsync(lambda);
        }

        public async Task<IEnumerable<T>> GetEntitiesByFieldAsync<T>(
            string fieldName, string value,
            params Expression<Func<T, object>>[] includes)
            where T : BaseEntity
        {
            var param    = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(param, fieldName);
            var constant = Expression.Constant(value);
            var equal    = Expression.Equal(property, Expression.Convert(constant, property.Type));
            var lambda   = Expression.Lambda<Func<T, bool>>(equal, param);

            IQueryable<T> query = Set<T>().Where(lambda).AsNoTracking();
            foreach (var inc in includes) query = query.Include(inc);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetEntitiesByFieldsAsync<T>(
            Dictionary<string, object> conditions)
            where T : BaseEntity
        {
            var param = Expression.Parameter(typeof(T), "x");
            Expression? body = null;

            foreach (var kvp in conditions)
            {
                var prop  = Expression.Property(param, kvp.Key);
                var val   = Expression.Constant(kvp.Value);
                var equal = Expression.Equal(prop, Expression.Convert(val, prop.Type));
                body = body == null ? equal : Expression.AndAlso(body, equal);
            }

            if (body == null) return await Set<T>().AsNoTracking().ToListAsync();

            var lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return await Set<T>().Where(lambda).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(
            params Expression<Func<T, object>>[] includes)
            where T : BaseEntity
        {
            IQueryable<T> query = Set<T>().AsNoTracking();
            foreach (var inc in includes) query = query.Include(inc);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllActiveAsync<T>(
            params Expression<Func<T, object>>[] includes)
            where T : BaseEntity
        {
            IQueryable<T> query = Set<T>().AsNoTracking().Where(e => e.Active);
            foreach (var inc in includes) query = query.Include(inc);
            return await query.ToListAsync();
        }

        // ── Reads with DTO projection ─────────────────────────────────────────

        public async Task<IEnumerable<Q>> GetAllDetailsAsync<T, Q>(
            params Expression<Func<T, object>>[] includes)
            where T : BaseEntity where Q : BaseDetails
        {
            IQueryable<T> query = Set<T>().AsNoTracking();
            foreach (var inc in includes) query = query.Include(inc);
            var result = await query.ToListAsync();
            return result.Select(_mapper.Map<Q>);
        }

        public async Task<IEnumerable<Q>> GetAllActiveDetailsAsync<T, Q>(
            params Expression<Func<T, object>>[] includes)
            where T : BaseEntity where Q : BaseDetails
        {
            IQueryable<T> query = Set<T>().AsNoTracking().Where(e => e.Active);
            foreach (var inc in includes) query = query.Include(inc);
            var result = await query.ToListAsync();
            return result.Select(_mapper.Map<Q>);
        }

        // ── Paged query ───────────────────────────────────────────────────────

        public async Task<PagedResult<Q>> GetPagedDetailsAsync<T, Q>(
            QueryOptions options,
            params Expression<Func<T, object>>[] includes)
            where T : BaseEntity where Q : BaseDetails
        {
            IQueryable<T> query = Set<T>().AsNoTracking();
            foreach (var inc in includes) query = query.Include(inc);

            query = query
                .ApplyFilters(options.Filters)
                .ApplySearch(options.SearchTerm ?? string.Empty, options.SearchColumns)
                .ApplySorting(options.SortBy, options.SortDescending);

            var total = await query.CountAsync();
            var items = await query
                .Skip((options.PageNumber - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToListAsync();

            var mapped = items.Select(_mapper.Map<Q>).ToList().AsReadOnly();
            return new PagedResult<Q>(mapped, total, options.PageNumber, options.PageSize);
        }
    }
}
