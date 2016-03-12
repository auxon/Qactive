using System;

namespace SharedLibrary
{
	[Serializable]
	public sealed class FeedServiceArgument
	{
		public bool IsAtom
		{
			get;
			set;
		}

		public Uri Url
		{
			get;
			set;
		}

		public override string ToString()
		{
			return "{" + (IsAtom ? "Atom: " : "RSS: ") + Url + "}";
		}
	}
}