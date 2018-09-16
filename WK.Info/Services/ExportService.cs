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
					var cell = row.CreateCell(1);
					cell.SetCellType(CellType.Numeric);
					cell.SetCellValue(value);
					return cell;
				}
			}

			void CreateHomonymsSheet()
			{
				var data = vocabs.GroupBy(v => v.Kana)
					.Select(g => new { Reading = g.Key, Count = g.Count(), Kanjis = g.ToList() })
					.OrderByDescending(x => x.Count)
					.ToList();

				var sheet = workbook.CreateSheet("Homonyms");

				var columns = new List<string> { "Reading", "Count", "Vocab", "Frequency", "Meaning" };
				CreateHeader(sheet, columns);

				var i = 1;
				for (var j = 0; j < data.Count; j++, i++)
				{
					var item = data[j];
					var row = sheet.CreateRow(i++);

					row.CreateCell(0).SetCellValue(item.Reading);

					var countCell = row.CreateCell(1);
					countCell.SetCellType(CellType.Numeric);
					countCell.SetCellValue(item.Count);
					if (item.Count > 1)
					{
						//countCell.CellStyle = orangeStyle;
					}

					row.CreateCell(2).SetCellValue(item.Kanjis.First().Vocab);
					row.CreateCell(3).SetCellValue(item.Kanjis.First().Frequency);
					row.CreateCell(4).SetCellValue(item.Kanjis.First().Meaning);

					if (item.Kanjis.Count > 1)
					{
						foreach (var kanji in item.Kanjis.Skip(1))
						{
							var rowOther = sheet.CreateRow(i++);

							rowOther.CreateCell(2).SetCellValue(kanji.Vocab);
							rowOther.CreateCell(3).SetCellValue(kanji.Frequency);
							rowOther.CreateCell(4).SetCellValue(kanji.Meaning);
						}
					}
				}
			}
		}
	}
}
