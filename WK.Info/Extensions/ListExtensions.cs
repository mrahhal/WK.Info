using System.Collections.Generic;
using System.Linq;
using WK.Info.Services;

namespace WK.Info
{
	public static class ListExtensions
	{
		public static string Join(this IEnumerable<TagModel> list)
		{
			return string.Join(" ", list.Select(x => x.Key));
		}
	}
}
