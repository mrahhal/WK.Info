﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WK.Info.Helpers;

namespace WK.Info.Services
{
	public interface IVocabDictionaryService : ISetupService
	{
		Dictionary<string, TagModel> Tags { get; }

		Dictionary<string, VocabModel> Vocabs { get; }
	}

	public class VocabDictionaryService : TagDictionaryServiceBase, IVocabDictionaryService
	{
		private readonly IDictionaryProvider _dictionaryProvider;

		public VocabDictionaryService(
			IDictionaryProvider dictionaryProvider)
		{
			_dictionaryProvider = dictionaryProvider;
		}

		public Dictionary<string, VocabModel> Vocabs { get; private set; }

		public async Task SetupAsync()
		{
			var tagFiles = await _dictionaryProvider.CollectVocabTagFilesAsync();
			ProcessTagBanks(tagFiles);

			var vocabFiles = await _dictionaryProvider.CollectVocabFilesAsync();
			Vocabs = ProcessVocabBanks(vocabFiles);
		}

		private Dictionary<string, VocabModel> ProcessVocabBanks(List<FileInfo> fileInfoes)
		{
			var serialzer = new JsonSerializer();
			var map = new Dictionary<string, VocabModel>();

			foreach (var fileInfo in fileInfoes)
			{
				using (var fs = fileInfo.OpenRead())
				using (var sr = new StreamReader(fs))
				using (var jtr = new JsonTextReader(sr))
				{
					var raw = serialzer.Deserialize<VocabModelRaw>(jtr);
					var models = raw.Select(l =>
					{
						var tags = (string)l[2];
						var tags2 = (string)l[3];
						var tagModels = TagHelper.SplitTags(tags, tags2).Select(x =>
						{
							if (Tags.TryGetValue(x, out var tagModel))
							{
								return tagModel;
							}
							return null;
						}).Where(x => x != null).ToList();

						var meanings = ((JArray)l[5]).ToObject<List<string>>();

						return new VocabModel
						{
							Vocab = (string)l[0],
							Kana = (string)l[1],
							Tags = tagModels,
							Meanings = meanings,
						};
					});

					foreach (var model in models)
					{
						map[model.Vocab] = model;
					}
				}
			}

			return map;
		}
	}
}