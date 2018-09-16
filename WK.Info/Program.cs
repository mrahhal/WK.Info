using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using WK.Info.Services;

namespace WK.Info
{
	public class Program
	{
		private readonly IEnumerable<ISetupService> _setupServices;
		private readonly IFrequencyDictionaryService _frequencyDictionaryService;
		private readonly IKanjiDictionaryService _kanjiDictionaryService;
		private readonly IVocabDictionaryService _vocabDictionaryService;
		private readonly IWaniKaniDictionaryService _waniKaniDictionaryService;
		private readonly IAggregatorService _aggregatorService;

		private static Task Main(string[] args)
		{
			return ContainerAccessor.Container.Resolve<Program>().RunAsync();
		}

		public Program(
			IEnumerable<ISetupService> setupServices,
			IFrequencyDictionaryService frequencyDictionaryService,
			IKanjiDictionaryService kanjiDictionaryService,
			IVocabDictionaryService vocabDictionaryService,
			IWaniKaniDictionaryService waniKaniDictionaryService,
			IAggregatorService aggregatorService)
		{
			_setupServices = setupServices;
			_frequencyDictionaryService = frequencyDictionaryService;
			_kanjiDictionaryService = kanjiDictionaryService;
			_vocabDictionaryService = vocabDictionaryService;
			_waniKaniDictionaryService = waniKaniDictionaryService;
			_aggregatorService = aggregatorService;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Preparing...");
			var sw = Stopwatch.StartNew();

			await Task.WhenAll(_setupServices.Select(s => s.SetupAsync()));

			Console.WriteLine($"Preparing finished: {sw.Elapsed.TotalSeconds} sec");

			var f = _frequencyDictionaryService.Kanjis;
			var k = _kanjiDictionaryService.Kanjis;
			var v = _vocabDictionaryService.Vocabs;
			var wk = _waniKaniDictionaryService.Kanjis;
			var wv = _waniKaniDictionaryService.Vocabs;

			Console.WriteLine("Preparing...");
			sw.Restart();

			var aggregationResult = await _aggregatorService.AggregateDataAsync();

			Console.WriteLine($"Preparing finished: {sw.Elapsed.TotalSeconds} sec");

			var ak = aggregationResult.Kanjis;
			var av = aggregationResult.Vocabs;

			var item = av.FirstOrDefault(x => x.Tags.Any(t => t.Key == "v5"));

			Console.WriteLine(item.Kana);
		}
	}
}
