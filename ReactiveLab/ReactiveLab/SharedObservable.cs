using System;
using System.Reactive.Linq;


namespace ReactiveLab
{
	internal class SharedObservable
	{
		internal static void Run()
		{
			var unshared = Observable.Range(1, 4);

			// Each subscription starts a new sequence
			unshared.Subscribe(i => Console.WriteLine("Unshared Subscription #1: " + i));
			unshared.Subscribe(i => Console.WriteLine("Unshared Subscription #2: " + i));

			Console.WriteLine();

			// By using publish the subscriptions are shared, but the sequence doesn't start until Connect() is called.
			var shared = unshared.Publish();
			shared.Subscribe(i => Console.WriteLine("Shared Subscription #1: " + i));
			shared.Subscribe(i => Console.WriteLine("Shared Subscription #2: " + i));
			shared.Connect();
		}
	}
}