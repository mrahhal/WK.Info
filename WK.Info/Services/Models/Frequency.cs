using System.Collections.Generic;
using System.Diagnostics;

namespace WK.Info.Services
{
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
