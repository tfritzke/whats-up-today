using System;
using Microsoft.EntityFrameworkCore;
using WhatsUpToday.Core.Data.Interfaces;

namespace WhatsUpToday.Core.Data.Extensions;

public static class DbContextAutoSaveExtensions
{
    #region SAVE_CHANGES

    private static string _currentUser = "";
    private static bool _useUTCTime = false;
    private static bool _autoSaveCreatedBy = false;
    private static bool _autoSaveModifiedBy = false;
    private static bool _autoSaveCreationDate = false;
    private static bool _autoSaveModificationDate = false;

    /// <summary>
    /// Specifies which entity fields are automatically saved.
    /// </summary>
    /// <param name="AutoSaveCreatedBy">Entity is IEntityCreatedBy</param>
    /// <param name="AutoSaveModifiedBy">Entity is IEntityModifiedBy</param>
    /// <param name="AutoSaveCreationDate">Entity is IEntityCreatedDate or IEntityDateCreated</param>
    /// <param name="AutoSaveModificationDate">Entity is IEntityDateModified or IEntityModifiedDate</param>
    /// <param name="CurrentUser"></param>
    /// <param name="UseUTCTime"></param>
    public static void AutoSaveEntityOptions(
        this DbContext context,
        string CurrentUser = "",
        bool AutoSaveCreatedBy = false,
        bool AutoSaveModifiedBy = false,
        bool AutoSaveCreationDate = false,
        bool AutoSaveModificationDate = false,
        bool UseUTCTime = false
        )
    {
        _currentUser = CurrentUser;
        _useUTCTime = UseUTCTime;
        _autoSaveCreatedBy = AutoSaveCreatedBy;
        _autoSaveModifiedBy = AutoSaveModifiedBy;
        _autoSaveCreationDate = AutoSaveCreationDate;
        _autoSaveModificationDate = AutoSaveModificationDate;
    }

    /// <summary>
    /// Automatically sets the Entity creation/modification dates and user
    /// for tracked entities based on an object's interfaces.
    /// Caller should SaveChanges after this completes.
    /// </summary>
    /// <param name="context">Extension</param>
    public static void AutoSaveEntityChanges(
        this DbContext context
        )
    {
        // get the current timestamp
        var now = DateTime.Now;
        if (_useUTCTime) now = now.ToUniversalTime();

        // find what's changed
        context.ChangeTracker.DetectChanges();

        // get new or changed entities
        var entries = context.ChangeTracker.Entries()
                            .Where(e => e.State == EntityState.Added
                                     || e.State == EntityState.Modified);

        // update fields according to parameters and entity interface
        foreach (var entry in entries)
        {
            // when entity is added:
            if (entry.State == EntityState.Added)
            {
                // "CreatedBy"
                if (_autoSaveCreatedBy)
                {
                    if (entry.Entity is IAutoSaveEntityCreatedBy)
                        ((IAutoSaveEntityCreatedBy)entry.Entity).CreatedBy = _currentUser;
                }

                // "CreatedDate" or "DateCreated"
                if (_autoSaveCreationDate)
                {
                    if (entry.Entity is IAutoSaveEntityCreatedDate)
                        ((IAutoSaveEntityCreatedDate)entry.Entity).CreatedDate = now;
                    if (entry.Entity is IAutoSaveEntityDateCreated)
                        ((IAutoSaveEntityDateCreated)entry.Entity).DateCreated = now;
                }
            }

            // when entity is updated:
            if (entry.State == EntityState.Modified)
            {
                // "ModifiedBy"
                if (_autoSaveModifiedBy)
                {
                    if (entry.Entity is IAutoSaveEntityModifiedBy)
                        ((IAutoSaveEntityModifiedBy)entry.Entity).ModifiedBy = _currentUser;
                }

                // "ModifiedDate" or "DateModified"
                if (_autoSaveModificationDate)
                {
                    if (entry.Entity is IAutoSaveEntityDateModified)
                        ((IAutoSaveEntityDateModified)entry.Entity).DateModified = now;
                    if (entry.Entity is IAutoSaveEntityModifiedDate)
                        ((IAutoSaveEntityModifiedDate)entry.Entity).ModifiedDate = now;
                }
            }
        }

    }

    #endregion

}
