using MagniseTestTask.Models;
using Microsoft.EntityFrameworkCore;

namespace MagniseTestTask.DatabaseContext;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public ApplicationDbContext()
    {
    }

    public virtual DbSet<Asset> Assets { get; set; }
}