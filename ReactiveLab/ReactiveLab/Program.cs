using System;
using System.Reactive.Linq;
using System.Threading;


namespace ReactiveLab
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("*** Start background task as Observable");
			BackgroundObservable.Run();

			Console.WriteLine("\n*** Cancellable cycle");
			CancelableObservable.Run();

			Console.WriteLine("\n*** Subscribe to an event");
			EventObservable.Run();

			Console.WriteLine("\n*** Reading stream");
			AsyncModelObservable.Run();

			Console.WriteLine("\n*** Reading an IEnumerable");
			EnumerableObservable.Run();

			Console.WriteLine("\n*** Time ticker");
			TimeObservable.Run();

			Console.WriteLine("\n*** Buffer");
			BufferedObservable.Run();

			Console.WriteLine("\n*** Time intervals");
			TimeIntervalObservable.Run();

			Console.WriteLine("\n*** Timeout");
			TimeoutObservable.Run();

			Console.WriteLine("\n*** GroupJoin");
			GroupJoinObservable.Run();

			Console.WriteLine("\n*** Generate");
			GenerateObservable.Run();

			Console.WriteLine("\n*** ISubject ping-pong");
			PingPong.Run();

			Console.WriteLine("\n*** Shared subscription");
			SharedObservable.Run();

			Console.WriteLine("\n*** Merge and Zip");
			MergeObservable.Run();

			Console.WriteLine();
			Console.WriteLine("Press enter");
			Console.ReadLine();
		}
	}
}
