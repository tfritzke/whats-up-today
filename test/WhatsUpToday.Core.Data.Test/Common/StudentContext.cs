using System;
using Microsoft.EntityFrameworkCore;

namespace WhatsUpToday.Core.Data.Test.Common;

public class StudentContext : DbContext
{
    public StudentContext()
    { }

    public StudentContext(DbContextOptions<StudentContext> options) : base(options)
    { }

    public DbSet<Student> Students { get; set; }
}
