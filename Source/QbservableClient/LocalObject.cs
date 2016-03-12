using SharedLibrary;

namespace QbservableClient
{
	class LocalObject
	{
		public string LocalInstanceProperty
		{
			get
			{
				ConsoleTrace.PrintCurrentMethod();

				return "=";
			}
		}

		public int LocalInstanceMethod()
		{
			ConsoleTrace.PrintCurrentMethod();

			return 100;
		}
	}
}