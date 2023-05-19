
using Microsoft.EntityFrameworkCore;
using WhatsUpToday.Core.Data.Extensions;
using WhatsUpToday.Core.Data.Interfaces;
using WhatsUpToday.Core.Data.Test.Common;

namespace WhatsUpToday.Core.Data.Test.DbContextAutoSaveTests;

public class TestAutoSave
{
    private static StudentContext GetMemoryContext()
    {
        var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

        return new StudentContext(options);
    }

    [SetUp]
    public void Setup()
    {
        var db = GetMemoryContext();
        db.Database.EnsureDeleted();
    }

    [Test]
    public void DoesStudentAutoSaveDateCreated()
    {
        var db = GetMemoryContext();
        var student = new Student { FirstName = "Jakob", LastName = "Soerensen" };
        var processor = new AuditProcessor(db);
        processor.AddStudent(student);

        db.AutoSaveEntityOptions(
            CurrentUser: "NUnit3",
            AutoSaveCreationDate: true,
            AutoSaveModificationDate: true,
            AutoSaveModifiedBy: true
        );

        // Save!
        db.AutoSaveEntityChanges();
        db.SaveChanges();

        // Assert one row added...
        int rows = db.Students.Count();
        Assert.That(rows, Is.EqualTo(1));

        student = db.Students.FirstOrDefault();
        Assert.That(student, Is.Not.Null);
        Assert.Pass("DateCreated is {0}", student.DateCreated.ToLongTimeString());
    }

    [Test]
    public void DoesStudentAutoSaveDateModified()
    {
        var db = GetMemoryContext();
        var student = new Student { FirstName = "Jakob", LastName = "Soerensen" };
        var processor = new AuditProcessor(db);
        processor.AddStudent(student);

        db.AutoSaveEntityOptions(
            CurrentUser: "NUnit3",
            AutoSaveCreationDate: true,
            AutoSaveModificationDate: true,
            AutoSaveModifiedBy: true
        );

        // Save!
        db.AutoSaveEntityChanges();
        db.SaveChanges();

        // Assert one row added...
        int rows = db.Students.Count();
        Assert.That(rows, Is.EqualTo(1));

        student = db.Students.First();
        Assert.That(student is IAutoSaveEntityDateModified, Is.True);

        // Modify
        student.FirstName = "Jacob";
        db.AutoSaveEntityChanges();
        db.SaveChanges();

        Assert.Pass("DateModified is {0}", student.DateModified.ToLongTimeString());
    }
}
