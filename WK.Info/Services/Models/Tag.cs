using System.Collections.Generic;
using System.Diagnostics;

namespace WK.Info.Services
{
	[DebuggerDisplay("{Key,nq}: {Value,nq}")]
	public class TagModel
	{
		public string Key { get; set; }
		public string Value { get; set; }
		public int Order { get; set; }
	}

	public class TagModelRaw : List<List<object>>
	{
	}
}
