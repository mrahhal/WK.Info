using System.Collections.Generic;

namespace WK.Info.Services
{
	public class AggregateKanjiModel
	{
		public WaniKaniKanji WaniKaniKanji { get; set; }
		public KanjiModel KanjiModel { get; set; }
		public FrequencyModel FrequencyModel { get; set; }

		public int Level => WaniKaniKanji.Level;
		//public string Character => WaniKaniKanji.Character;
		public string Meaning => WaniKaniKanji.Meaning;
		public string Onyomi => WaniKaniKanji.Onyomi;
		public string Kunyomi => WaniKaniKanji.Kunyomi;
		public string ImportantReading => WaniKaniKanji.ImportantReading;
		public string Nanori => WaniKaniKanji.Nanori;

		public string Kanji => KanjiModel?.Kanji ?? WaniKaniKanji.Character;
		//public string Onyomi => KanjiModel.Onyomi;
		//public string Kunyomi => KanjiModel.Kunyomi;
		public List<string> Meanings => KanjiModel?.Meanings;
		public List<TagModel> Tags => KanjiModel?.Tags ?? new List<TagModel>();

		public int Frequency => FrequencyModel?.Frequency ?? 0;
	}

	public class AggregateVocabModel
	{
		public WaniKaniVocab WaniKaniVocab { get; set; }
		public VocabModel VocabModel { get; set; }
		public FrequencyModel FrequencyModel { get; set; }

		public int Level => WaniKaniVocab.Level;
		//public string Character => WaniKaniVocab.Character;
		public string Kana => WaniKaniVocab.Kana;
		public string Meaning => WaniKaniVocab.Meaning;

		public string Vocab => VocabModel?.Vocab ?? WaniKaniVocab.Character;
		//public string Kana => VocabModel?.Kana;
		public List<string> Meanings => VocabModel?.Meanings;
		public List<TagModel> Tags => VocabModel?.Tags ?? new List<TagModel>();

		public int Frequency => FrequencyModel?.Frequency ?? 0;
	}

	public class AggregationResult
	{
		public List<AggregateKanjiModel> Kanjis { get; set; }
		public List<AggregateVocabModel> Vocabs { get; set; }
	}
}
