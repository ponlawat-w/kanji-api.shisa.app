using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShisaKanjiDatabaseContext.Models {
  [Table("structures")]
  public class Structure {
    [Key][Required]
    [Column("id")][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("kanji")][Required]
    public long KanjiId { get; set; }
    public Kanji Kanji { get; set; }

    [Column("structure", TypeName = "VARCHAR(200)")][Required]
    public string StructureString { get; set; }

    [Column("note", TypeName = "VARCHAR(100)")]
    public string Note { get; set; }
  }
}
