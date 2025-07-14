using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace BFBMX.ServerApi.EF;

public class BibMessageContextFactory : IDesignTimeDbContextFactory<BibMessageContext>
{
    public BibMessageContext CreateDbContext(string[] args)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        var dbFolder = Path.Combine(path, "BFBMX");
        if (!Directory.Exists(dbFolder))
        {
            Directory.CreateDirectory(dbFolder);
        }
        var optionsBuilder = new DbContextOptionsBuilder<BibMessageContext>();
        optionsBuilder.UseSqlite($"Data Source={Path.Combine(path, "BFBMX", "BFBMX-Messages.db")}");
        return new BibMessageContext(optionsBuilder.Options);
    }
}