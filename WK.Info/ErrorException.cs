using System;

namespace WK.Info
{
	[Serializable]
	public class ErrorException : Exception
	{
		public ErrorException(string message) : base(message)
		{
		}
	}
}
