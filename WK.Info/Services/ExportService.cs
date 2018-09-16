using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace WK.Info.Services
{
	public interface IExportService
	{
		Task ExportAsync(AggregationResult result);
	}

	public class ExportService : IExportService
	{
		public Task ExportAsync(AggregationResult result)
		{
			var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"WK.Info.xlsx");

			var workbook = new XSSFWorkbook();
			var ch = workbook.GetCreationHelper();

			var kanjis = result.Kanjis;
			var vocabs = result.Vocabs;

			var homonyms = vocabs.GroupBy(v => v.Kana)
				.Select(g => new { Reading = g.Key, Count = g.Count(), Items = g.ToList() })
				.OrderByDescending(x => x.Count)
				.ToList();

			//var orangeStyle = workbook.CreateCellStyle();
			//orangeStyle.FillForegroundColor = HSSFColor.Orange.Index;
			//orangeStyle.FillPattern = FillPattern.SolidForeground;

			CreateStatsSheet();
			CreateHomonymsSheet();
			CreateKanjiSheet();

			using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
			{
				workbook.Write(fs);
			}

			return Task.CompletedTask;

			void CreateHeader(ISheet sheet, List<string> columns)
			{
				var headerRow = sheet.CreateRow(0);
				for (var i = 0; i < columns.Count; i++)
				{
					var c = columns[i];
					headerRow.CreateCell(i).SetCellValue(c);
				}
			}

			void CreateStatsSheet()
			{
				var sheet = workbook.CreateSheet("Stats");

				var columns = new List<string> { "Type", "Count" };
				CreateHeader(sheet, columns);

				var i = 1;

				var kanjisRow = sheet.CreateRow(i++);
				CreateName(kanjisRow, "Kanjis");
				CreateNumericCell(kanjisRow, kanjis.Count);

				var vocabsRow = sheet.CreateRow(i++);
				CreateName(vocabsRow, "Vocabs");
				CreateNumericCell(vocabsRow, vocabs.Count);

				var homonymsRow = sheet.CreateRow(i++);
				CreateName(homonymsRow, "Homonyms");
				CreateNumericCell(homonymsRow, homonyms.Where(x => x.Count > 1).Count());

				ICell CreateName(IRow row, string name)
				{
					var cell = row.CreateCell(0);
					cell.SetCellValue(name);
					return cell;
				}

				ICell CreateNumericCell(IRow row, int value)
				{
					var cell = row.CreateNumericCell(1);
					cell.SetCellValue(value);
					return cell;
				}
			}

			void CreateHomonymsSheet()
			{
				var data = homonyms;

				var sheet = workbook.CreateSheet("Homonyms");

				var columns = new List<string> { "Reading", "Count", "Vocab", "Frequency", "Meaning" };
				CreateHeader(sheet, columns);

				var i = 1;
				for (var j = 0; j < data.Count; j++, i++)
				{
					var item = data[j];
					var row = sheet.CreateRow(i++);

					row.CreateCell(0).SetCellValue(item.Reading);
					row.CreateNumericCell(1).SetCellValue(item.Count);

					row.CreateCell(2).SetCellValue(item.Items.First().Vocab);
					row.CreateCell(3).SetCellValue(item.Items.First().Frequency);
					row.CreateCell(4).SetCellValue(item.Items.First().Meaning);

					if (item.Items.Count > 1)
					{
						foreach (var kanji in item.Items.Skip(1))
						{
							var rowOther = sheet.CreateRow(i++);

							rowOther.CreateCell(2).SetCellValue(kanji.Vocab);
							rowOther.CreateCell(3).SetCellValue(kanji.Frequency);
							rowOther.CreateCell(4).SetCellValue(kanji.Meaning);
						}
					}
				}
			}

			void CreateKanjiSheet()
			{
				var data = kanjis.OrderByDescending(x => x.Frequency).ToList();

				var sheet = workbook.CreateSheet("Kanji");

				var columns = new List<string> { "Kanji", "Frequency", "Level", "On", "Kun", "Tags", "Meaning" };
				CreateHeader(sheet, columns);

				for (var i = 0; i < data.Count; i++)
				{
					var item = data[i];
					var row = sheet.CreateRow(i + 1);

					var j = 0;
					row.CreateCell(j++).SetCellValue(item.Kanji);
					row.CreateNumericCell(j++).SetCellValue(item.Frequency);
					row.CreateNumericCell(j++).SetCellValue(item.Level);
					row.CreateCell(j++).SetCellValue(item.Onyomi);
					row.CreateCell(j++).SetCellValue(item.Kunyomi);
					row.CreateCell(j++).SetCellValue(item.Tags.Join());
					row.CreateCell(j++).SetCellValue(item.Meaning);
				}
			}
		}
	}
}
