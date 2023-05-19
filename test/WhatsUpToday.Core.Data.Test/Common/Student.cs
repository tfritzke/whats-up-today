using System;
using System.Diagnostics.CodeAnalysis;
using WhatsUpToday.Core.Data.Interfaces;

namespace WhatsUpToday.Core.Data.Test.Common;

public class Student : IAutoSaveEntityCreatedBy, IAutoSaveEntityModifiedBy, IAutoSaveEntityDateCreated, IAutoSaveEntityDateModified
{
    public Student()
    {
        DateCreated = DateTime.MinValue;
        DateModified = DateTime.MinValue;
        CreatedBy = "";
        ModifiedBy = "";
    }

    public int StudentId { get; set; }

    [AllowNull]
    public string FirstName { get; set; }
    [AllowNull]
    public string LastName { get; set; }

    // Auditing - 
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
}
