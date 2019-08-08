using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShisaKanjiDatabaseContext.Models {
  [Table("parts")]
  public class Part {
    [Key][Required]
    [Column("id")][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("kanji")][Required]
    public long KanjiId { get; set; }
    public Kanji Kanji { get; set; }

    [Column("part")][Required]
    public long PartialKanjiId { get; set; }
    public Kanji PartialKanji { get; set; }
  }
}
