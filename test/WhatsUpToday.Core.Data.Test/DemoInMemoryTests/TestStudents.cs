using System;
using Microsoft.EntityFrameworkCore;
using WhatsUpToday.Core.Data.Test.Common;

namespace WhatsUpToday.Core.Data.Test.DemoInMemoryTests;

public class TestStudents
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

        db.Students.Add(new Student { FirstName = "John", LastName = "Doe" });
        db.Students.Add(new Student { FirstName = "Jane", LastName = "Doe" });
        db.SaveChanges();
    }

    [Test]
    public void CanAddStudent()
    {
        var db = GetMemoryContext();
        var student = new Student { FirstName = "Jakob", LastName = "Soerensen" };
        var processor = new StudentProcessor(db);
        processor.AddStudent(student);
        db.SaveChanges();

        int rows = db.Students.Count();
        Assert.That(rows, Is.EqualTo(3));
    }

    [Test]
    public void DoesStudentFailsOnBlankFirstName()
    {
        var db = GetMemoryContext();
        // FirstName should not be allowed to be null.
        var student = new Student { FirstName = null, LastName = "Soerensen" };
        var processor = new StudentProcessor(db);

        // NUnit allows you to check if a specific type of 
        // exception is thrown. In this particular scenario,
        // we would expect an ArgumentException.
        Assert.Throws<ArgumentException>(() =>
            processor.AddStudent(student));
    }
}
