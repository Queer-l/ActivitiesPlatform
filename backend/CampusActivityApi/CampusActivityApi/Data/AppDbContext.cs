using Microsoft.EntityFrameworkCore;
using CampusActivityApi.Models;

namespace CampusActivityApi.Data
{
    /// <summary>
    /// Entity Framework Core 数据库上下文。
    /// </summary>
    /// <remarks>
    /// 该上下文集中声明系统所有实体集合，并在 <see cref="OnModelCreating"/> 中维护表名、字段转换、唯一索引和级联删除规则。
    /// </remarks>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// 通过依赖注入接收数据库连接配置。
        /// </summary>
        /// <param name="options">EF Core 上下文配置，包含 SQL Server 连接串。</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 用户表集合。
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// 活动表集合，包含待审核、已通过和已驳回活动。
        /// </summary>
        public DbSet<MyActivity> Activities { get; set; }

        /// <summary>
        /// 活动留言表集合。
        /// </summary>
        public DbSet<Message> Messages { get; set; }

        /// <summary>
        /// 报名记录表集合。
        /// </summary>
        public DbSet<Enroll> Enrolls { get; set; }

        /// <summary>
        /// 签到记录表集合。
        /// </summary>
        public DbSet<SignIn> SignIns { get; set; }

        /// <summary>
        /// 活动签到码表集合。
        /// </summary>
        public DbSet<ActivitySignInCode> ActivitySignInCodes { get; set; }

        /// <summary>
        /// 密码重置申请表集合。
        /// </summary>
        public DbSet<ResetPwdApply> ResetPwdApplies { get; set; }

        /// <summary>
        /// 配置实体与数据库表结构之间的映射关系。
        /// </summary>
        /// <param name="modelBuilder">EF Core 模型构建器。</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 显式指定表名，避免类名与数据库表名不一致导致运行时找不到表。
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<MyActivity>().ToTable("Activity");
            modelBuilder.Entity<Message>().ToTable("Message");
            modelBuilder.Entity<Enroll>().ToTable("Enroll");
            modelBuilder.Entity<SignIn>().ToTable("SignIn");
            modelBuilder.Entity<ActivitySignInCode>().ToTable("ActivitySignInCode");
            modelBuilder.Entity<ResetPwdApply>().ToTable("ResetPwdApply");

            // 代码属性名为 PassWord，数据库列名为 Password。
            modelBuilder.Entity<User>()
                .Property(u => u.PassWord)
                .HasColumnName("Password");

            // SQL Server 中使用 tinyint 保存角色和启用状态，代码中分别使用 int/bool。
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<byte>();

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasConversion<byte>();

            // 审核状态枚举以 tinyint 形式保存，便于数据库约束和前端数值判断。
            modelBuilder.Entity<MyActivity>()
                .Property(a => a.AuditStatus)
                .HasConversion<byte>();

            modelBuilder.Entity<ResetPwdApply>()
                .Property(a => a.Status)
                .HasConversion<byte>();

            // 用户名必须唯一，避免登录账号冲突。
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // 同一用户对同一活动只能报名一次。
            modelBuilder.Entity<Enroll>()
                .HasIndex(e => new { e.UserName, e.ActivityId })
                .IsUnique();

            // 同一用户对同一活动只能签到一次。
            modelBuilder.Entity<SignIn>()
                .HasIndex(s => new { s.UserName, s.ActivityId })
                .IsUnique();

            // 签到码需要全局唯一，扫码时可直接通过 Code 定位活动。
            modelBuilder.Entity<ActivitySignInCode>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // 删除活动时同步删除留言，避免孤立留言残留。
            modelBuilder.Entity<MyActivity>()
                .HasMany(a => a.Messages)
                .WithOne()
                .HasForeignKey(m => m.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
