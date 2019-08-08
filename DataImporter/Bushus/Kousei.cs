using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataImporter.Bushus {
  public class Kousei {
    public readonly string Kanji;
    public readonly string[] Bushus;

    static string[] CJKV = new string[] { "⿰", "⿱", "⿲", "⿳", "⿴", "⿵", "⿶", "⿷", "⿸", "⿹", "⿺", "⿻" };

    public Kousei(string kanji, IDictionary<string, string[]> structuresDict) {
      HashSet<string> bushus = new HashSet<string>(GetParts(kanji, structuresDict));
      HashSet<string> searched = new HashSet<string>();
      int lastBushusLength, newBushusLength;
      do {
        lastBushusLength = bushus.Count;
        foreach (string bushu in bushus.ToArray()) {
          if (searched.Contains(bushu)) {
            continue;
          }

          bushus.UnionWith(GetParts(bushu, structuresDict));

          searched.Add(bushu);
        }
        newBushusLength = bushus.Count;
      } while (lastBushusLength != newBushusLength);

      Kanji = kanji;
      Bushus = bushus.ToArray();
    }

    private static string[] GetParts(string kanji, IDictionary<string, string[]> structuresDict) {
      HashSet<string> totalElements = new HashSet<string>();
      string[] structures = structuresDict[kanji];
      foreach (string structure in structures) {
        totalElements.UnionWith(ExtractElements(structure));
      }
      return totalElements.ToArray();
    }

    private static string[] ExtractElements(string str) {
      List<string> elements = new List<string>();
      TextElementEnumerator tEnum = StringInfo.GetTextElementEnumerator(str);
      while (tEnum.MoveNext()) {
        elements.Add(tEnum.GetTextElement());
      }
      return elements.Where(s => !CJKV.Contains(s)).ToArray();
    }
  }
}
