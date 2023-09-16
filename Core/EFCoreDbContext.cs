using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Reformat.Data.EFCore.Config;
using Reformat.Framework.Core.Common.Extensions.lang;

namespace Reformat.Data.EFCore.Core;

public class EFCoreDbContext : DbContext,IDisposable
{
    private const string DB_SETTING = "Database:";
    public const string DB_TYPE = DB_SETTING + "DbType";
    public const string DB_CONNECTION = DB_SETTING + "DbConn";

    private IConfiguration cfg;

    public EFCoreDbContext(DbContextOptions options, IConfiguration cfg) : base(options)
    {
        this.cfg = cfg;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string? dbType = cfg.GetValue<string>(DB_TYPE);
        string? dbConn = cfg.GetValue<string>(DB_CONNECTION);

        if (dbType.Equals("Mysql"))
        {
            optionsBuilder.UseMySql(dbConn, MySqlServerVersion.LatestSupportedServerVersion, b =>
            {
                // b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                // b.MigrationsAssembly("SQ.Train.Api");
            });
        }
        else
        {
            throw new NotImplementedException("尚未配置其他数据库的链接方式");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        MappingEntityTypes(modelBuilder);
        ModelBuilderConfig(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<T> GetDbSet<T>() where T : EFCoreEntity
    {
        if (Model.FindEntityType(typeof(T)) != null) return Set<T>();
        throw new Exception($"类型{typeof(T).Name}未在数据库上下文中注册，请先在DbContextOption设置ModelAssemblyName以将所有实体类型注册到数据库上下文中。");
    }

    private void MappingEntityTypes(ModelBuilder modelBuilder)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var types = assemblies
            .SelectMany(f => f.GetTypes())
            .Where(c=>c.IsSubClassOf(typeof(EFCoreEntity))||c.IsSubclassOf(typeof(EFCoreView)));
        
        foreach (var type in types)
        {
            var entityType = modelBuilder.Model.FindEntityType(type);
            if (entityType == null)
            {
                modelBuilder.Model.AddEntityType(type);
            }
        }
    }

    private void ModelBuilderConfig(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();
        foreach (var entityType in entityTypes)
        {
            // 获取实体类型的 CLR 类型
            var entityTypeClrType = entityType.ClrType;
            if(!entityType.ClrType.IsSubClassOf(typeof(EFCoreEntity))) continue;
            
            // 构造实体类型对应的配置类型的实例
            var configurationType = typeof(EntitySnowIdConfiguration<>).MakeGenericType(entityTypeClrType);
            var configuration = Activator.CreateInstance(configurationType);

            // 调用 ApplyConfiguration 方法应用配置
            modelBuilder.ApplyConfiguration((dynamic)configuration);
        }
    }
}