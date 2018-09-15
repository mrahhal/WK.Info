using System.Collections.Generic;

namespace WK.Info
{
	public class UserInformation
	{
		public int Level { get; set; }
		public string Title { get; set; }
		public string Username { get; set; }
	}

	public class Vocabulary
	{
		public int Level { get; set; }
		public string Character { get; set; }
		public string Kana { get; set; }
		public string Meaning { get; set; }
	}

	public class VocabularyModelRequestedInformation
	{
		public List<Vocabulary> General { get; set; }
	}

	public class VocabularyModel
	{
		public UserInformation UserInformation { get; set; }
		public VocabularyModelRequestedInformation RequestedInformation { get; set; }
	}
}
