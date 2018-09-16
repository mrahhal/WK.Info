using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WK.Info.Helpers;

namespace WK.Info.Services
{
	public interface IKanjiDictionaryService : ISetupService
	{
		Dictionary<string, TagModel> Tags { get; }

		Dictionary<string, KanjiModel> Kanjis { get; }
	}

	public class KanjiDictionaryService : IKanjiDictionaryService
	{
		private readonly IDictionaryProvider _dictionaryProvider;

		public KanjiDictionaryService(
			IDictionaryProvider dictionaryProvider)
		{
			_dictionaryProvider = dictionaryProvider;
		}

		public Dictionary<string, TagModel> Tags { get; private set; }

		public Dictionary<string, KanjiModel> Kanjis { get; private set; }

		public async Task SetupAsync()
		{
			var tagFiles = await _dictionaryProvider.CollectKanjiTagFilesAsync();
			Tags = ProcessTagBanks(tagFiles);

			var kanjiFiles = await _dictionaryProvider.CollectKanjiFilesAsync();
			Kanjis = ProcessKanjiBanks(kanjiFiles);
		}

		private Dictionary<string, TagModel> ProcessTagBanks(List<FileInfo> fileInfoes)
		{
			var serialzer = new JsonSerializer();
			var map = new Dictionary<string, TagModel>();

			foreach (var fileInfo in fileInfoes)
			{
				using (var fs = fileInfo.OpenRead())
				using (var sr = new StreamReader(fs))
				using (var jtr = new JsonTextReader(sr))
				{
					var raw = serialzer.Deserialize<TagModelRaw>(jtr);
					var models = raw.Select(l =>
					{
						var key = (string)l[0];
						var order = Convert.ToInt32(l[2]);
						var value = (string)l[3];

						return new TagModel { Key = key, Value = value, Order = order };
					});

					foreach (var model in models)
					{
						map[model.Key] = model;
					}
				}
			}

			return map;
		}

		private Dictionary<string, KanjiModel> ProcessKanjiBanks(List<FileInfo> fileInfoes)
		{
			var serialzer = new JsonSerializer();
			var map = new Dictionary<string, KanjiModel>();

			foreach (var fileInfo in fileInfoes)
			{
				using (var fs = fileInfo.OpenRead())
				using (var sr = new StreamReader(fs))
				using (var jtr = new JsonTextReader(sr))
				{
					var raw = serialzer.Deserialize<KanjiModelRaw>(jtr);
					var models = raw.Select(l =>
					{
						var tag = (string)l[3];
						var tags = TagHelper.SplitTags(tag).Select(x =>
						{
							if (Tags.TryGetValue(x, out var tagModel))
							{
								return tagModel;
							}
							return null;
						}).Where(x => x != null).ToList();

						var meanings = ((JArray)l[4]).ToObject<List<string>>();

						return new KanjiModel
						{
							Kanji = (string)l[0],
							Onyomi = (string)l[1],
							Kunyomi = (string)l[2],
							Tags = tags,
							Meanings = meanings,
						};
					});

					foreach (var model in models)
					{
						map[model.Kanji] = model;
					}
				}
			}

			return map;
		}
	}
}
