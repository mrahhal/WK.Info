using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

		private static Task Main(string[] args)
		{
			return ContainerAccessor.Container.Resolve<Program>().RunAsync();
		}

		public Program(
			IEnumerable<ISetupService> setupServices,
			IFrequencyDictionaryService frequencyDictionaryService)
		{
			_setupServices = setupServices;
			_frequencyDictionaryService = frequencyDictionaryService;
		}

		public async Task RunAsync()
		{
			Console.WriteLine("Preparing...");
			var sw = Stopwatch.StartNew();

			await Task.WhenAll(_setupServices.Select(s => s.SetupAsync()));

			Console.WriteLine($"Preparing finished: {sw.Elapsed.TotalSeconds} sec");

			var k = _frequencyDictionaryService.Kanjis;
		}
	}
}
