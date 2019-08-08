using ShisaKanjiDatabaseContext.Models;
using System.Collections.Generic;
using System.Linq;

namespace ShisaKanjis.ApiObjects {
  public class ApiKanjiSearchResultObject {
    public string Kanji { get; set; }

    public IEnumerable<string> StructuresWithCJKV { get; set; }
    public IEnumerable<string> StructuresWithoutCJKV { get; set; }

    public IEnumerable<string> MajorParts { get; set; }
    public IEnumerable<string> MinorParts { get; set; }

    public ApiKanjiSearchResultObject(Kanji record) {
      Kanji = record.Character;
      StructuresWithCJKV = record.Structures.Select(s => s.StructureString);
      StructuresWithoutCJKV = StructuresWithCJKV.Select(s => Utils.StripCJKV(s));
      MajorParts = record.MajorParts.Select(p => p.Kanji.Character);
      MinorParts = record.MinorParts.Select(p => p.PartialKanji.Character);
    }
  }
}
