using System.Threading.Tasks;

namespace WK.Info.Services
{
	public interface IWaniKaniDictionaryService
	{
		WaniKaniKanjiModel Kanjis { get; }

		WaniKaniVocabModel Vocabs { get; }
	}

	public class WaniKaniDictionaryService : IWaniKaniDictionaryService, ISetupService
	{
		private readonly IWaniKaniService _waniKaniService;

		public WaniKaniDictionaryService(
			IWaniKaniService waniKaniService)
		{
			_waniKaniService = waniKaniService;
		}

		public WaniKaniKanjiModel Kanjis { get; private set; }

		public WaniKaniVocabModel Vocabs { get; private set; }

		public async Task SetupAsync()
		{
			var kanjisTask = _waniKaniService.GetKanjisAsync();
			var vocabsTask = _waniKaniService.GetVocabsAsync();

			await Task.WhenAll(kanjisTask, vocabsTask);

			Kanjis = kanjisTask.Result;
			Vocabs = vocabsTask.Result;
		}
	}
}
