using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataImporter.Bushus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShisaKanjiDatabaseContext;
using ShisaKanjiDatabaseContext.Models;

namespace DataImporter {
  class Program {
    static KanjiDbContext DatabaseContext;
    static NpgsqlConnection Connection;
    static Regex StructurePattern;
    static IDictionary<string, Kanji> KanjiDict;

    static string GetArgument(string[] args, string findFlag) {
      for (int i = 0; i < args.Length; i++) {
        if (args[i] == findFlag && i < args.Length - 1) {
          return args[i + 1];
        }
      }

      return null;
    }

    static void TruncateAll() {
      Console.WriteLine("Truncating tables");
      Console.Write($"  kanjis...");
      DatabaseContext.Database.ExecuteSqlCommand("TRUNCATE kanjis RESTART IDENTITY CASCADE");
      Console.WriteLine("OK");
      Console.Write($"  structures...");
      DatabaseContext.Database.ExecuteSqlCommand("TRUNCATE structures RESTART IDENTITY CASCADE");
      Console.WriteLine("OK");
      Console.Write($"  parts...");
      DatabaseContext.Database.ExecuteSqlCommand("TRUNCATE parts RESTART IDENTITY CASCADE");
      Console.WriteLine("OK");
    }

    static void TruncateParts() {
      Console.Write($"Truncating parts...");
      DatabaseContext.Database.ExecuteSqlCommand("TRUNCATE parts RESTART IDENTITY CASCADE");
      Console.WriteLine("OK");
    }

    static void AddStructure(long kanjiId, string structure) {
      Match match = StructurePattern.Match(structure);
      string note = match.Success ? match.Groups[1].Value : null;
      string sql = note == null ?
        "INSERT INTO structures (kanji, structure) VALUES (@k, @s) ON CONFLICT DO NOTHING" :
        "INSERT INTO structures (kanji, structure, note) VALUES (@k, @s, @n) ON CONFLICT DO NOTHING";
      if (note != null) {
        structure = StructurePattern.Replace(structure, "");
      }

      using (NpgsqlCommand command = new NpgsqlCommand(sql, Connection)) {
        command.Parameters.AddWithValue("k", kanjiId);
        command.Parameters.AddWithValue("s", structure);
        if (note != null) {
          command.Parameters.AddWithValue("n", note);
        }
        command.ExecuteNonQuery();
      }
    }

    static void AddKanji(string kanjiCharacter, string[] structures) {
      long? kanjiId = null;
      using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO kanjis (character) VALUES (@k) ON CONFLICT DO NOTHING RETURNING id", Connection)) {
        command.Parameters.AddWithValue("k", kanjiCharacter);
        object result = command.ExecuteScalar();
        kanjiId = result == null ? (long?)null : (long)result;
      }

      if (kanjiId != null) {
        foreach (string structure in structures) {
          AddStructure(kanjiId.Value, structure);
        }
      }
    }

    //static List<Kanji> GetBasicKanjis() {
    //  List<Structure> basicStructure = DatabaseContext.Structures
    //    .Where(s => s.Kanji.Character == s.StructureString)
    //    .ToList();
    //  List<long> basicKanjiIds = basicStructure.Select(s => s.KanjiId).ToList();

    //  return DatabaseContext.Kanjis
    //    .Where(k => basicKanjiIds.Contains(k.Id))
    //    .ToList();
    //}

    static Kanji GetKanjiRecord(string kanji) {
      if (KanjiDict.ContainsKey(kanji)) {
        return KanjiDict[kanji];
      }
      Kanji kanjiRecord = DatabaseContext.Kanjis.Include(k => k.Structures).SingleOrDefault(k => k.Character == kanji);
      if (kanjiRecord == null) {
        kanjiRecord = new Kanji {
          Character = kanji,
          Structures = new Structure[] {}
        };
        DatabaseContext.Kanjis.Add(kanjiRecord);
        DatabaseContext.SaveChanges();
      }
      KanjiDict.Add(kanji, kanjiRecord);
      return KanjiDict[kanji];
    }

    static void AddPart(Kousei kousei) {
      long kanjiId = GetKanjiRecord(kousei.Kanji).Id;
      long[] values = kousei.Bushus.Select(k => GetKanjiRecord(k).Id).ToArray();
      string[] sqlValues = values.Select((v, i) => $"(@k, @{i})").ToArray();
      using (NpgsqlCommand command = new NpgsqlCommand($"INSERT INTO parts (kanji, part) VALUES {string.Join(',', sqlValues)} ON CONFLICT DO NOTHING", Connection)) {
        command.Parameters.AddWithValue("k", kanjiId);
        for (int i = 0; i < kousei.Bushus.Length; i++) {
          command.Parameters.AddWithValue(i.ToString(), values[i]);
        }
        command.ExecuteNonQuery();
      }
    }

    static void ExecuteParts() {
      Console.Write("Preloading all data...");
      Kanji[] allKanjis = DatabaseContext.Kanjis.Include(k => k.Structures).ToArray();
      Console.WriteLine("OK");

      Console.Write("Creating structures map...");
      IDictionary<string, string[]> structuresDict = allKanjis
        .ToDictionary(k => k.Character, k => k.Structures.Select(s => s.StructureString).ToArray());
      Console.WriteLine("OK");

      Console.Write("Generating kouseis map...");
      Kousei[] kouseis = allKanjis.Select(k => new Kousei(k.Character, structuresDict)).ToArray();
      Console.WriteLine("OK");

      Console.WriteLine("Adding to database...");
      int i = 0;
      foreach (Kousei kousei in kouseis) {
        AddPart(kousei);

        if (i % 500 == 0) {
          Console.WriteLine($"{i}/{kouseis.Length}");
        }

        i++;
      }
      Console.WriteLine($"{i}/{kouseis.Length}");
      Console.WriteLine("OK");
    }

    static void ReadData(string sourcePath) {
      int count = 0;
      using (StreamReader reader = new StreamReader(sourcePath)) {
        while (!reader.EndOfStream) {
          string line = reader.ReadLine().Trim();
          if (line.StartsWith('#')) {
            continue;
          }

          string[] row = line.Split('\t');
          if (row.Length < 3) {
            continue;
          }

          AddKanji(row[1], row[2].Split(' '));
          if (count % 1000 == 0) {
            Console.Write($"{count} executed\r");
          }
          count++;
        }
        reader.Close();
      }
      Console.Write($"{count} executed\r");
      Console.WriteLine();
    }

    static void Main(string[] args) {
      DatabaseContext = KanjiDbContext.CreateDefaultContext();
      Connection = (NpgsqlConnection)DatabaseContext.Database.GetDbConnection();
      Connection.Open();
      StructurePattern = new Regex(@"\[(.*?)\]");
      KanjiDict = new Dictionary<string, Kanji>();

      string sourcePath = GetArgument(args, "-s");
      sourcePath = sourcePath == null ? "data.txt" : sourcePath;

      if (args.Contains("-t")) {
        TruncateAll();
      } else if (args.Contains("-tp")) {
        TruncateParts();
      }

      if (args.Contains("--skip-file")) {
        Console.WriteLine("Skip importing from file");
      } else {
        ReadData(sourcePath);
      }

      ExecuteParts();

      Console.Write("Closing connection...");
      Connection.Close();
      Console.WriteLine("Finished!");

      Console.WriteLine("\nProcess finished!");
    }
  }
}
