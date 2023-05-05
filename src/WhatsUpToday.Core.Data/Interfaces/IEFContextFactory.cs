using Microsoft.EntityFrameworkCore;

namespace WhatsUpToday.Core.Data.Interfaces;

public interface IEFContextFactory
{
    DbContext CreateContext();
}
