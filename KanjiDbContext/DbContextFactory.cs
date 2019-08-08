using Microsoft.EntityFrameworkCore.Design;

namespace ShisaKanjiDatabaseContext {
  public class DbContextFactory: IDesignTimeDbContextFactory<KanjiDbContext> {
    public KanjiDbContext CreateDbContext(string[] args) {
      return KanjiDbContext.CreateDefaultContext();
    }
  }
}
