using System;
using WhatsUpToday.Core.Data.Test.Common;

namespace WhatsUpToday.Core.Data.Test.DemoInMemoryTests;

public class StudentProcessor
{
    private readonly StudentContext db;

    public StudentProcessor(StudentContext db)
    {
        // The dependency injection is cruical. This allows us to
        // easily switch between InMemory and actual databases.
        this.db = db;
    }

    public void AddStudent(Student student)
    {
        // This is quite a simple validation. Real world
        // scenarios usually involve a lot more complex code. 
        if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        {
            var err = "Empty first/last name not allowed.";
            throw new ArgumentException(err);
        }
        db.Students.Add(student);
    }
}
