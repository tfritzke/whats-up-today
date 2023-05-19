using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsUpToday.Core.Data.Test.Common;

namespace WhatsUpToday.Core.Data.Test.DbContextAutoSaveTests;

public class AuditProcessor
{
    private readonly StudentContext db;

    public AuditProcessor(StudentContext db)
    {
        // The dependency injection is cruical. This allows us to
        // easily switch between InMemory and actual databases.
        this.db = db;
    }

    public void AddStudent(Student student)
    {
        student.DateCreated = DateTime.MinValue;
        student.DateModified = DateTime.MinValue;

        db.Students.Add(student);
    }
}
