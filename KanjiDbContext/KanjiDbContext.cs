using ShisaKanjiDatabaseContext.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ShisaKanjiDatabaseContext {
  public class KanjiDbContext: DbContext {

    public DbSet<Kanji> Kanjis { get; set; }
    public DbSet<Structure> Structures { get; set; }
    public DbSet<Part> Parts { get; set; }

    public KanjiDbContext(DbContextOptions<KanjiDbContext> opts): base(opts) { }

    protected override void OnModelCreating(ModelBuilder builder) {
      base.OnModelCreating(builder);

      builder.Entity<Kanji>()
        .HasKey(k => k.Id)
        .HasName("pkey_kanjis");

      builder.Entity<Structure>()
        .HasKey(s => s.Id)
        .HasName("pkey_structures");

      builder.Entity<Part>()
        .HasKey(p => p.Id)
        .HasName("pkey_parts");

      builder.Entity<Structure>()
        .HasOne(s => s.Kanji)
        .WithMany(k => k.Structures)
        .HasForeignKey(s => s.KanjiId)
        .HasPrincipalKey(k => k.Id)
        .HasConstraintName("fkey_structures_kanji")
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();

      builder.Entity<Part>()
        .HasOne(p => p.Kanji)
        .WithMany(k => k.MinorParts)
        .HasForeignKey(p => p.KanjiId)
        .HasPrincipalKey(k => k.Id)
        .HasConstraintName("fkey_parts_kanji")
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();

      builder.Entity<Part>()
        .HasOne(p => p.PartialKanji)
        .WithMany(k => k.MajorParts)
        .HasForeignKey(p => p.PartialKanjiId)
        .HasPrincipalKey(k => k.Id)
        .HasConstraintName("fkey_parts_part")
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();

      builder.Entity<Kanji>()
        .HasIndex(k => k.Character)
        .HasName("idx_kanjis_character")
        .IsUnique();

      builder.Entity<Structure>()
        .HasIndex(s => new { s.KanjiId, s.StructureString })
        .HasName("idx_structures_kanji_structure")
        .IsUnique();

      builder.Entity<Part>()
        .HasIndex(p => new { p.KanjiId, p.PartialKanjiId })
        .HasName("idx_parts_kanji_part")
        .IsUnique();
    }

    private static string ReadConnectionStringFromFile() {
      string connectionString;
      using (StreamReader reader = new StreamReader("DefaultConnectionString.txt")) {
        connectionString = reader.ReadToEnd();
        reader.Close();
      }
      return connectionString;
    }

    public static string GetDefaultConnectionString() {
      string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
      return connectionString == null ? ReadConnectionStringFromFile() : connectionString;
    }

    public static KanjiDbContext CreateDefaultContext(string connectionString = null) {
      DbContextOptionsBuilder<KanjiDbContext> optionsBuilder = new DbContextOptionsBuilder<KanjiDbContext>();
      optionsBuilder.UseNpgsql(connectionString == null ? GetDefaultConnectionString() : connectionString);
      return new KanjiDbContext(optionsBuilder.Options);
    }
  }
}
