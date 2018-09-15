using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WK.Info.Services
{
	public interface IWaniKaniService
	{
		Task<WaniKaniKanjiModel> GetKanjisAsync();

		Task<WaniKaniVocabularyModel> GetVocabsAsync();
	}

	public class WaniKaniService : IWaniKaniService
	{
		private const string ApiKeyEnvironmentVariable = "WK_INFO_API_KEY";

		private readonly string ApiUrl;
		private readonly string ApiVocabularyUrl;
		private readonly string ApiKanjiUrl;

		private readonly JsonSerializerSettings _jsonSerializerSettings;
		private readonly HttpClient _client;

		public WaniKaniService()
		{
			var apiKey = Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);
			ApiUrl = $"https://www.wanikani.com/api/user/{apiKey}";
			ApiVocabularyUrl = $"{ApiUrl}/vocabulary";
			ApiKanjiUrl = $"{ApiUrl}/kanji";

			var contractResolver = new DefaultContractResolver
			{
				NamingStrategy = new SnakeCaseNamingStrategy()
			};
			_jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = contractResolver };

			_client = new HttpClient();
		}

		public async Task<WaniKaniKanjiModel> GetKanjisAsync()
		{
			var response = await _client.GetAsync(ApiVocabularyUrl);
			TryThrowRequestFailed(response);

			var responseText = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<WaniKaniKanjiModel>(responseText, _jsonSerializerSettings);
		}

		public async Task<WaniKaniVocabularyModel> GetVocabsAsync()
		{
			var response = await _client.GetAsync(ApiVocabularyUrl);
			TryThrowRequestFailed(response);

			var responseText = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<WaniKaniVocabularyModel>(responseText, _jsonSerializerSettings);
		}

		private void TryThrowRequestFailed(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				return;
			}

			throw new Exception("request failed.");
		}
	}

	public class WaniKaniUserInformation
	{
		public int Level { get; set; }
		public string Title { get; set; }
		public string Username { get; set; }
	}

	public class WaniKaniVocabulary
	{
		public int Level { get; set; }
		public string Character { get; set; }
		public string Kana { get; set; }
		public string Meaning { get; set; }
	}

	public class WaniKaniVocabularyModelRequestedInformation
	{
		public List<WaniKaniVocabulary> General { get; set; }
	}

	public class WaniKaniVocabularyModel
	{
		public WaniKaniUserInformation UserInformation { get; set; }
		public WaniKaniVocabularyModelRequestedInformation RequestedInformation { get; set; }
	}

	public class WaniKaniKanji
	{
		public int Level { get; set; }
		public string Character { get; set; }
		public string Meaning { get; set; }
		public string Onyomi { get; set; }
		public string Kunyomi { get; set; }
		public string ImportantReading { get; set; }
		public string Nanori { get; set; }
	}

	public class WaniKaniKanjiModelRequestedInformation
	{
		public List<WaniKaniKanji> General { get; set; }
	}

	public class WaniKaniKanjiModel
	{
		public WaniKaniUserInformation UserInformation { get; set; }
		public WaniKaniKanjiModelRequestedInformation RequestedInformation { get; set; }
	}
}
