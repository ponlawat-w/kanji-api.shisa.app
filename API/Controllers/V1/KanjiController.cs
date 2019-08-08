using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ShisaKanjiDatabaseContext;
using ShisaKanjiDatabaseContext.Models;
using ShisaKanjis.ApiObjects;

namespace ShisaKanjis.Controllers.V1 {
  [Route("api/v1/kanji")]
  [ApiController]
  public class KanjiController: BaseController {
    public KanjiDbContext Database;

    public KanjiController(KanjiDbContext database) {
      Database = database;
    }

    [HttpGet("search/{keyword}")]
    public IActionResult SearchApi(string keyword) {
      Kanji kanji = Database.Kanjis
        .Include(k => k.Structures)
        .Include(k => k.MajorParts)
        .Include(k => k.MinorParts)
        .SingleOrDefault(k => k.Character == keyword);
      if (kanji == null) {
        return NotFoundResponse(ErrorMessages.KanjiNotFound);
      }

      {
        HashSet<long> majorKanjiIds = new HashSet<long>(kanji.MajorParts.Select(p => p.Id));
        IDictionary<long, Kanji> majorPartDict = Database.Parts
          .Include(p => p.Kanji)
          .Where(p => majorKanjiIds.Contains(p.Id))
          .ToDictionary(p => p.Id, p => p.Kanji);
        foreach (Part part in kanji.MajorParts) {
          part.Kanji = majorPartDict[part.Id];
        }
      }

      {
        HashSet<long> minorKanjiIds = new HashSet<long>(kanji.MinorParts.Select(p => p.Id));
        IDictionary<long, Kanji> minorPartDict = Database.Parts
          .Include(p => p.PartialKanji)
          .Where(p => minorKanjiIds.Contains(p.Id))
          .ToDictionary(p => p.Id, p => p.PartialKanji);
        foreach (Part part in kanji.MinorParts) {
          part.PartialKanji = minorPartDict[part.Id];
        }
      }

      return OkResponse(new ApiKanjiSearchResultObject(kanji));
    }

    [HttpGet("kumiawase/{keyword}")]
    public IActionResult KumiawaseApi(string keyword) {
      string[] elements = Utils.ToTextElements(keyword).Distinct().ToArray();
      if (elements.Length == 0) {
        return OkResponse(new string[] { });
      }

      long[] minorIds = Database.Kanjis
        .Where(k => elements.Contains(k.Character))
        .Select(k => k.Id)
        .ToArray();
      if (minorIds.Length == 0) {
        return OkResponse(new string[] { });
      }

      string[] subQueries = minorIds.Select((id, idx) => $"EXISTS (SELECT * FROM parts WHERE kanji = k.id AND part = @{idx})").ToArray();
      NpgsqlParameter[] parameters = minorIds.Select((id, idx) => new NpgsqlParameter($"@{idx}", id)).ToArray();
      string[] results = Database.Kanjis
        .FromSql($"SELECT * FROM kanjis k WHERE {string.Join(" AND ", subQueries)}", parameters)
        .Select(k => k.Character)
        .ToArray();

      return OkResponse(results);
    }
  }
}
