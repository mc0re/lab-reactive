using System;
using System.Diagnostics;


namespace IntroToRx
{
	public class TimeIt : IDisposable
	{
		private readonly string _name;

		private readonly Stopwatch _watch;


		public TimeIt(string name)
		{
			_name = name;
			_watch = Stopwatch.StartNew();
		}


		public void Dispose()
		{
			_watch.Stop();
			Console.WriteLine("{0} took {1}", _name, _watch.Elapsed);
		}
	}
}
