using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShisaKanjiDatabaseContext.Models {
  [Table("kanjis")]
  public class Kanji {
    [Key][Required]
    [Column("id")][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("character", TypeName = "VARCHAR(5)")][Required]
    public string Character { get; set; }

    public ICollection<Structure> Structures { get; set; }

    public ICollection<Part> MinorParts { get; set; }
    public ICollection<Part> MajorParts { get; set; }
  }
}
