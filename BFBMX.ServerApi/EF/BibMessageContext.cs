using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace BFBMX.ServerApi.EF;


public class BibMessageContext : DbContext
{
    public DbSet<WinlinkMessageModel> WinlinkMessageModels { get; set; } = null!;
    public DbSet<FlaggedBibRecordModel> FlaggedBibRecordModels { get; set; } = null!;
    public BibMessageContext(DbContextOptions<BibMessageContext> options) : base(options) { }
}
