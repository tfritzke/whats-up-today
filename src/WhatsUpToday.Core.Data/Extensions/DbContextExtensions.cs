using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WhatsUpToday.Core.Data.Extensions;

public static class DbContextExtensions
{
    #region IDENTITY_INSERT

    /*
     * IdentityInsert: https://stackoverflow.com/questions/40896047
     */

    public static void EnableIdentityInsert<T>(this DbContext context) 
        => SetIdentityInsert<T>(context, true);
    public static void DisableIdentityInsert<T>(this DbContext context) 
        => SetIdentityInsert<T>(context, false);

    public static async Task EnableIdentityInsertAsync<T>(this DbContext context) 
        => await SetIdentityInsertAsync<T>(context, true);
    public static async Task DisableIdentityInsertAsync<T>(this DbContext context) 
        => await SetIdentityInsertAsync<T>(context, false);

    private static void SetIdentityInsert<T>([NotNull] DbContext context, bool enable)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var entityType = context.Model.FindEntityType(typeof(T));
        var value = enable ? "ON" : "OFF";
        context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {entityType.GetSchema()}.{entityType.GetTableName()} {value}");
    }

    public static void SaveChangesWithIdentityInsert<T>([NotNull] this DbContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        using var transaction = context.Database.BeginTransaction();
        context.EnableIdentityInsert<T>();
        context.SaveChanges();
        context.DisableIdentityInsert<T>();
        transaction.Commit();
    }

    private static async Task SetIdentityInsertAsync<T>([NotNull] DbContext context, bool enable)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var entityType = context.Model.FindEntityType(typeof(T));
        var value = enable ? "ON" : "OFF";
        await context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT {entityType.GetSchema()}.{entityType.GetTableName()} {value}");
    }

    public static async Task SaveChangesWithIdentityInsertAsync<T>([NotNull] this DbContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        await using var transaction = await context.Database.BeginTransactionAsync();
        await context.EnableIdentityInsertAsync<T>();
        await context.SaveChangesAsync();
        await context.DisableIdentityInsertAsync<T>();
        await transaction.CommitAsync();
    }

    #endregion

    #region TRUNCATE_TABLE

    /*
     * https://www.codeproject.com/Articles/5339402/Delete-All-Rows-in-Entity-Framework-Core-6
     */

    private const string kDbo = "dbo";

    private static string GetName(
        IEntityType entityType,
        string defaultSchemaName = kDbo
        )
    {
        /* 3.0.1 these were working */
        //var schemaName = entityType.GetSchema();
        //var tableName = entityType.GetTableName();

        /* 5 and 6 these are working */
        var schema = entityType.FindAnnotation("Relational:Schema").Value;
        string tableName = entityType.GetAnnotation
                           ("Relational:TableName").Value.ToString();
        string schemaName = schema == null ? defaultSchemaName : schema.ToString();

        /* table full name */
        string name = string.Format("[{0}].[{1}]", schemaName, tableName);
        return name;
    }

    public static string TableName<T>(DbContext dbContext) where T : class
    {
        var entityType = dbContext.Model.FindEntityType(typeof(T));
        return GetName(entityType);
    }

    public static string TableName<T>(DbSet<T> dbSet) where T : class
    {
        var entityType = dbSet.EntityType;
        return GetName(entityType);
    }

    public static string Truncate<T>(this DbSet<T> dbSet) where T : class
    {
        string cmd = $"TRUNCATE TABLE {TableName(dbSet)}";
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        context.Database.ExecuteSqlRaw(cmd);
        return cmd;
    }

    public static string Delete<T>(this DbSet<T> dbSet) where T : class
    {
        string cmd = $"DELETE FROM {TableName(dbSet)}";
        var context = dbSet.GetService<ICurrentDbContext>().Context;
        context.Database.ExecuteSqlRaw(cmd);
        return cmd;
    }

    public static void Clear<T>(this DbContext context) where T : class
    {
        DbSet<T> dbSet = context.Set<T>();
        if (dbSet.Any())
        {
            dbSet.RemoveRange(dbSet.ToList());
        }
    }

    public static void Clear<T>(this DbSet<T> dbSet) where T : class
    {
        if (dbSet.Any())
        {
            dbSet.RemoveRange(dbSet.ToList());
        }
    }

    public static string Truncate(
        this DbContext context,
        string tableName,
        string schemaName = kDbo
        )
    {
        string name = string.Format("[{0}].[{1}]", schemaName, tableName);
        string cmd = $"TRUNCATE TABLE {name}";
        context.Database.ExecuteSqlRaw(cmd);
        return cmd;
    }

    public static string Delete(
        this DbContext context,
        string tableName,
        string schemaName = kDbo
        )
    {
        string name = string.Format("[{0}].[{1}]", schemaName, tableName);
        string cmd = $"DELETE FROM {name}";
        context.Database.ExecuteSqlRaw(cmd);
        return cmd;
    }

    #endregion

    /// <summary>
    /// Generate a script to create all tables for the current model
    /// </summary>
    /// <returns>A SQL script</returns>
    public static string GenerateCreateScript(this DbContext context)
    {
        return context.Database.GenerateCreateScript();
    }

}
