using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveLab
{
	internal class MergeObservable
	{
		private static IObservable<int> Xs
		{
			get { return Generate(0, 1, new List<double> { 0.1, 0.3, 0.1, 0.2, 0.2 }); }
		}


		private static IObservable<int> Ys
		{
			get { return Generate(100, 10, new List<double> { 0.2, 0.1, 0.3, 0.2, 0.2 }); }
		}


		private static IObservable<int> Generate(int initialValue, int step, IList<double> intervals)
		{
			// work-around for Observable.Generate calling timeInterval before resultSelector
			intervals.Add(0);

			return Observable.Generate(0,
									   n => n < intervals.Count - 1,
									   n => n + 1,
									   n => initialValue + n * step,
									   n => TimeSpan.FromSeconds(intervals[n]));
		}


		internal static void Run()
		{
			var ev = new ManualResetEvent(false);

			Console.WriteLine("Merge X and Y");
			using (Xs.Merge(Ys).Subscribe(
				z => Console.Write("{0} ", z),
				() => ev.Set()))
			{
				ev.WaitOne();
			}

			Console.WriteLine();
			ev.Reset();

			Console.WriteLine("Zip X, Y -> X + Y");
			using (Xs.Zip(Ys, (x, y) => x + y)
					.Subscribe(
						z => Console.Write("{0} ", z),
						() => ev.Set()))
			{
				ev.WaitOne();
			}

			Console.WriteLine();
			ev.Reset();

			Console.WriteLine("Combine X, Y -> X + Y");
			using (Xs.CombineLatest(Ys, (x, y) => x + y)
					.Subscribe(
						z => Console.Write("{0} ", z),
						() => ev.Set()))
			{
				ev.WaitOne();
			}

			Console.WriteLine();
			ev.Reset();

			Console.WriteLine("Concat X and Y");
			using (Xs.Concat(Ys)
					.Subscribe(
						z => Console.Write("{0} ", z),
						() => ev.Set()))
			{
				ev.WaitOne();
			}

			Console.WriteLine();
			ev.Reset();
		}
	}
}
