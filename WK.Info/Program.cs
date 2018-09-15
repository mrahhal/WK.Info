using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace WK.Info
{
	public class Program
	{
		private const string ApiKeyEnvironmentVariable = "WK_INFO_API_KEY";

		private readonly string ApiUrl;
		private readonly string ApiVocabularyUrl;

		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly HttpClient _client;

		public static Task Main(string[] args)
		{
			return new Program().RunAsync();
		}

		public Program()
		{
			var apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);
			ApiUrl = $"https://www.wanikani.com/api/user/{apiKey}";
			ApiVocabularyUrl = $"{ApiUrl}/vocabulary";

			var contractResolver = new DefaultContractResolver
			{
				NamingStrategy = new SnakeCaseNamingStrategy()
			};
			_jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = contractResolver };

			_client = new HttpClient();
		}

		public async Task RunAsync()
		{
			var response = await _client.GetAsync(ApiVocabularyUrl);
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception("request failed.");
			}

			var responseText = await response.Content.ReadAsStringAsync();
			var model = JsonConvert.DeserializeObject<VocabularyModel>(responseText, _jsonSerializerSettings);

			await ProcessAsync(model);
		}

		private async Task ProcessAsync(VocabularyModel model)
		{
			var vocabs = model.RequestedInformation.General;
			await ExportExcelAsync(vocabs);
		}

		private Task ExportExcelAsync(List<Vocabulary> vocabs)
		{
			var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"WK.Info.xlsx");

			var workbook = new XSSFWorkbook();
			var ch = workbook.GetCreationHelper();

			var orangeStyle = workbook.CreateCellStyle();
			orangeStyle.FillForegroundColor = HSSFColor.Orange.Index;
			orangeStyle.FillPattern = FillPattern.SolidForeground;

			CreateHomonymsCountSheet();
			CreateHomonymsWithKanjiSheet();

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

			void CreateHomonymsCountSheet()
			{
				var data = vocabs.GroupBy(v => v.Kana)
					.Select(g => new { Reading = g.Key, Count = g.Count() })
					.OrderByDescending(x => x.Count)
					.ToList();

				var sheet = workbook.CreateSheet("Homonyms Count");

				var columns = new List<string> { "Reading", "Count" };
				CreateHeader(sheet, columns);

				for (var i = 0; i < data.Count; i++)
				{
					var item = data[i];
					var row = sheet.CreateRow(i + 1);

					row.CreateCell(0).SetCellValue(item.Reading);

					var countCell = row.CreateCell(1);
					countCell.SetCellType(CellType.Numeric);
					countCell.SetCellValue(item.Count);
					if (item.Count > 1)
					{
						//countCell.CellStyle = orangeStyle;
					}
				}
			}

			void CreateHomonymsWithKanjiSheet()
			{
				var data = vocabs.GroupBy(v => v.Kana)
					.Select(g => new { Reading = g.Key, Count = g.Count(), Kanjis = g.ToList() })
					.OrderByDescending(x => x.Count)
					.ToList();

				var sheet = workbook.CreateSheet("Homonyms with Kanji");

				var columns = new List<string> { "Reading", "Count", "Kanji", "Meaning" };
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

					row.CreateCell(2).SetCellValue(item.Kanjis.First().Character);
					row.CreateCell(3).SetCellValue(item.Kanjis.First().Meaning);

					if (item.Kanjis.Count > 1)
					{
						foreach (var kanji in item.Kanjis.Skip(1))
						{
							var rowOther = sheet.CreateRow(i++);

							rowOther.CreateCell(2).SetCellValue(kanji.Character);
							rowOther.CreateCell(3).SetCellValue(kanji.Meaning);
						}
					}
				}
			}
		}
	}
}
