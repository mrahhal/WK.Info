using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WK.Info.Services
{
	public interface IFrequencyDictionaryService : ISetupService
	{
		Dictionary<string, FrequencyModel> Kanjis { get; }

		Dictionary<string, FrequencyModel> Vocabs { get; }
	}

	public class FrequencyDictionaryService : IFrequencyDictionaryService
	{
		private readonly IDictionaryProvider _dictionaryProvider;

		public FrequencyDictionaryService(
			IDictionaryProvider dictionaryProvider)
		{
			_dictionaryProvider = dictionaryProvider;
		}

		public Dictionary<string, FrequencyModel> Kanjis { get; private set; }

		public Dictionary<string, FrequencyModel> Vocabs { get; private set; }

		public async Task SetupAsync()
		{
			var kanjiFiles = await _dictionaryProvider.CollectKanjiFrequencyFilesAsync();
			var vocabFiles = await _dictionaryProvider.CollectVocabFrequencyFilesAsync();

			Kanjis = Process(kanjiFiles, false);
			Vocabs = Process(vocabFiles, true);
		}

		private Dictionary<string, FrequencyModel> Process(List<FileInfo> fileInfoes, bool skipSingleFrequency)
		{
			var serialzer = new JsonSerializer();
			var map = new Dictionary<string, FrequencyModel>();

			foreach (var fileInfo in fileInfoes)
			{
				using (var fs = fileInfo.OpenRead())
				using (var sr = new StreamReader(fs))
				using (var jtr = new JsonTextReader(sr))
				{
					var raw = serialzer.Deserialize<FrequencyModelRaw>(jtr);
					var models = raw.Select(l =>
					{
						var term = (string)l[0];
						var frequency = Convert.ToInt32(l[2]);

						return new FrequencyModel { Term = term, Frequency = frequency };
					});

					foreach (var model in models.Where(x => !skipSingleFrequency || x.Frequency > 1))
					{
						map[model.Term] = model;
					}
				}
			}

			return map;
		}
	}

	[DebuggerDisplay("{Term,nq}: {Frequency,nq}")]
	public class FrequencyModel
	{
		public string Term { get; set; }

		public int Frequency { get; set; }
	}

	public class FrequencyModelRaw : List<List<object>>
	{
	}
}
