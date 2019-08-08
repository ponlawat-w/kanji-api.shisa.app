using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ShisaKanjis {
  public static class Utils {
    public static string[] CJKV = new string[] { "⿰", "⿱", "⿲", "⿳", "⿴", "⿵", "⿶", "⿷", "⿸", "⿹", "⿺", "⿻" };

    public static string StripCJKV(string structureString) {
      StringBuilder builder = new StringBuilder();
      TextElementEnumerator textEnum = StringInfo.GetTextElementEnumerator(structureString);
      while (textEnum.MoveNext()) {
        string element = textEnum.GetTextElement();
        if (CJKV.Contains(element)) {
          continue;
        }
        builder.Append(element);
      }

      return builder.ToString();
    }

    public static string[] ToTextElements(string str) {
      List<string> elements = new List<string>();
      TextElementEnumerator textEnum = StringInfo.GetTextElementEnumerator(str);
      while (textEnum.MoveNext()) {
        elements.Add(textEnum.GetTextElement());
      }
      return elements.ToArray();
    }
  }
}
