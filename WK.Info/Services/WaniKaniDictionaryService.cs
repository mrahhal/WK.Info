using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WK.Info.Services
{
	public interface IWaniKaniDictionaryService
	{
		List<WaniKaniKanji> Kanjis { get; }

		List<WaniKaniVocab> Vocabs { get; }

		List<WaniKaniVocab> IgnoredVocabs { get; }
	}

	public class WaniKaniDictionaryService : IWaniKaniDictionaryService, ISetupService
	{
		private readonly IWaniKaniService _waniKaniService;
		private readonly IVocabDictionaryService _vocabDictionaryService;

		public WaniKaniDictionaryService(
			IWaniKaniService waniKaniService,
			IVocabDictionaryService vocabDictionaryService)
		{
			_waniKaniService = waniKaniService;
			_vocabDictionaryService = vocabDictionaryService;
		}

		public List<WaniKaniKanji> Kanjis { get; private set; }

		public List<WaniKaniVocab> Vocabs { get; private set; }

		public List<WaniKaniVocab> IgnoredVocabs { get; private set; }

		public async Task SetupAsync()
		{
			var kanjisTask = _waniKaniService.GetKanjisAsync();
			var vocabsTask = _waniKaniService.GetVocabsAsync();

			await Task.WhenAll(kanjisTask, vocabsTask);

			Kanjis = kanjisTask.Result.RequestedInformation;

			var (Filtered, Ignored) = FilterVocabs(vocabsTask.Result.RequestedInformation.General);
			Vocabs = Filtered;
			IgnoredVocabs = Ignored;
		}

		private (List<WaniKaniVocab> Filtered, List<WaniKaniVocab> Ignored) FilterVocabs(List<WaniKaniVocab> vocabs)
		{
			var ignored = new List<WaniKaniVocab>();

			var filtered = vocabs.Where(vocab =>
			{
				if (!_vocabDictionaryService.Vocabs.TryGetValue(vocab.Character, out var vocabModel))
				{
					ignored.Add(vocab);
					return false;
				}

				return true;
			}).ToList();

			return (filtered, ignored);
		}
	}
}
