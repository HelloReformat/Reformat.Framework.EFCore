﻿using Microsoft.EntityFrameworkCore;
using Reformat.Data.EFCore.Core;

namespace Reformat.Data.EFCore;

public static class Startup
{
    private const string DB_SETTING = "Database:";
    public const string DB_TYPE = DB_SETTING + "DbType";
    public const string DB_CONNECTION = DB_SETTING + "DbConn";
    public const string SNOW_ID_KEY = DB_SETTING + "SnowId";
    
    public static void AddEFCoreSupport(this WebApplicationBuilder builder)
    {
        // 启动配置
        IConfiguration cfg = builder.Configuration;
        string? dbType = cfg.GetValue<string>(DB_TYPE);
        string? dbConn = cfg.GetValue<string>(DB_CONNECTION);
        string? snowId = cfg.GetValue<string>(SNOW_ID_KEY);
        
        builder.Services.AddDbContextFactory<EFCoreDbContext>((provider, builder) =>
        {
            if (dbType.Equals("Mysql"))
            {
                Console.WriteLine(dbType + " : " + dbConn);
                builder.UseMySql(cfg.GetConnectionString(dbConn), MySqlServerVersion.LatestSupportedServerVersion, b =>
                {
                    // b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    // b.MigrationsAssembly("SQ.Train.Api");
                });
            }
        });
    }
    
    public static void AddEFCoreSupport<T>(this WebApplicationBuilder builder) where T : DbContext
    {
        // 启动配置
        IConfiguration cfg = builder.Configuration;
        string? dbType = cfg.GetValue<string>(DB_TYPE);
        string? dbConn = cfg.GetValue<string>(DB_CONNECTION);
        string? snowId = cfg.GetValue<string>(SNOW_ID_KEY);
        
        builder.Services.AddDbContextFactory<T>((provider, builder) =>
        {
            builder.UseMySql(cfg.GetConnectionString("Mysql"), MySqlServerVersion.LatestSupportedServerVersion, b =>
            {
                //b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                // b.MigrationsAssembly("SQ.Train.Api");
            });
        });
    }
}