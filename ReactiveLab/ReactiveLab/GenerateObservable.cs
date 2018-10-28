using System;
using System.Reactive.Linq;
using System.Threading;


namespace ReactiveLab
{
	internal class GenerateObservable
	{
		internal static void Run()
		{
			var ev = new ManualResetEvent(false);

			var observable = Observable.Generate(
				1, x => x < 6, x => x + 1, x => x,
				x => TimeSpan.FromMilliseconds(250)).Timestamp();

			using (observable.Subscribe(
				x => Console.WriteLine("{0}, {1}", x.Value, x.Timestamp),
				() => ev.Set()))
			{
				ev.WaitOne();
			}
		}
	}
}