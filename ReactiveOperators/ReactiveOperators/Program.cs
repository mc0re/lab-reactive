using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveOperators
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			//Console.WriteLine("*** Creation");
			CreationOps();

			//Console.WriteLine("\n*** Combinations");
			CombineOps();

			Console.WriteLine("\n*** Operations");
			MathOps();

			Console.WriteLine("\n*** Continuations");
			Continuations();

			Console.WriteLine("\nPress ENTER");
			Console.ReadLine();
		}


		#region Parts

		private static void CreationOps()
		{
			var ret = Observable.Return(3.14);
			Dump(ret);

			var range = Observable.Range(1, 5);
			Dump(range);

			var repeat = Observable.Repeat(range, 2);
			Dump(repeat);

			var starts = Observable.StartWith(Observable.Empty('z'), 's', 't', 'a', 'r', 't');
			Dump(starts);

			var toObs = new[] { 1, 1, 2, 3, 5, 8, 13 }.ToObservable();
			Dump(toObs);

			var timer = Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.2)).Take(10);
			Dump(timer);

			var defaultIfEmpty = Observable.DefaultIfEmpty(Observable.Empty(0), 555);
			Dump(defaultIfEmpty);

			var create = Observable.Create(
				(IObserver<string> observer) =>
				{
					Console.Write("Subscribed. ");
					observer.OnNext("a");
					observer.OnNext("b");
					observer.OnCompleted();
					Thread.Sleep(1000);
					return Disposable.Create(() => Console.WriteLine("Unsubscribed."));
				});
			Dump(create);
		}


		private static void CombineOps()
		{
			// 0.05, 0.15, 0.25, 0.35
			var chars = Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(0.05), TimeSpan.FromSeconds(0.1))
				.Select((_, n) => new[] { 'a', 'b', 'c', 'd' }[n]).Take(4);

			// 0.1, 0.2, 0.3, 0.4
			var ints1to4 = Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1))
				.Select(n => (int) n + 1).Take(4);

			// 0.05, 0.3, 0.55, 0.8
			var ints10to13 = Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(0.05), TimeSpan.FromSeconds(0.25))
				.Select(n => (int) n + 10).Take(4);

			// 0.1, 0.2, 0.3
			var ints4to6 = Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1))
				.Select(n => (int) n + 4).Take(3);

			var tick022 = Observable.Interval(TimeSpan.FromSeconds(0.22)).Take(1);


			var seqEqual = Observable.SequenceEqual(
				Observable.Range(0, 3).Select(a => (long) a),
				Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.2)).Take(3));
			Dump(seqEqual);

			var where = Observable.Range(1, 10).Repeat(2).Where(a => a > 7);
			Dump(where);

			var combineLatest = chars.CombineLatest(ints1to4, (x, y) => string.Format($"{x}{y}"));
			Dump(combineLatest);

			var concat = ints1to4.Concat(ints10to13);
			Dump(concat);

			var merge = ints1to4.Merge(ints10to13);
			Dump(merge);

			var withLatest = chars.WithLatestFrom(ints1to4, (x, y) => string.Format($"{x}{y}"));
			Dump(withLatest);

			var zip = chars.Zip(ints1to4, (x, y) => string.Format($"{x}{y}"));
			Dump(zip);

			var sample = chars.Sample(TimeSpan.FromSeconds(0.2));
			Dump(sample);

			var throttle = chars.Throttle(TimeSpan.FromSeconds(0.1));
			Dump(throttle);

			// 1, 1, 1, 2, 3, 4, 1, 2, 3, 4, 4, 5, 6
			var distinct = ints1to4.Repeat(2).StartWith(1, 1).Concat(ints4to6).Distinct();
			Dump(distinct);

			var distinctUntil = ints1to4.Repeat(2).StartWith(1, 1).Concat(ints4to6).DistinctUntilChanged();
			Dump(distinctUntil);

			var elementAt5 = ints1to4.Repeat(2).StartWith(1, 1).Concat(ints4to6).ElementAt(5);
			Dump(elementAt5);

			var first3 = ints1to4.Repeat(2).Concat(ints4to6).FirstAsync(n => n == 3);
			Dump(first3);

			var takeLast3 = ints1to4.Repeat(2).Concat(ints4to6).TakeLast(3);
			Dump(takeLast3);

			var takeUntil = ints1to4.Repeat(2).Concat(ints4to6).TakeUntil(tick022);
			Dump(takeUntil);

			var skipUntil = ints1to4.Repeat(2).Concat(ints4to6).SkipUntil(tick022);
			Dump(skipUntil);
		}


		private static void MathOps()
		{
			var ints1to9 = Observable.Range(1, 9);

			var count = ints1to9.Count();
			Dump(count);

			var aggregate = ints1to9.Aggregate((x, y) => x + y);
			Dump(aggregate);

			var scan = ints1to9.Scan((x, y) => x + y);
			Dump(scan);

			var any = ints1to9.Any();
			Dump(any);

			var append = ints1to9.Append(100);
			Dump(append);

			var average = ints1to9.Average();
			Dump(average);

			var collect = ints1to9.Collect(() => 0, (r, n) => r + n);
			Dump(collect);

			var doAction = ints1to9.Do(_ => Console.Write("."));
			Dump(doAction);

			Console.Write("forEach: ");
			var forEach = ints1to9.ForEachAsync(_ => Console.Write("."));
			forEach.Wait();
			Console.WriteLine();

			// TODO: DOes not really group, only creates the keys
			var groupBy = ints1to9.GroupBy(n => n % 2 == 0 ? "even" : "odd");
			Dump(groupBy);
			Task.Delay(200).Wait();

			var materialize = ints1to9.Materialize();
			Dump(materialize);
		}


		private static void Continuations()
		{
			var ints1to9 = Observable.Range(1, 9);
			var ints1to5ex = Observable.Range(1, 5).Concat(Observable.Throw<int>(new ArgumentException("Test")));

			// 0.05, 0.15, 0.25, 0.35
			var chars = Observable.Timer(DateTime.Now + TimeSpan.FromSeconds(0.05), TimeSpan.FromSeconds(0.1))
				.Select((_, n) => new[] { 'a', 'b', 'c', 'd' }[n]).Take(4);

			try
			{
				Dump(ints1to5ex);
			}
			catch (Exception ex)
			{
				Console.WriteLine("\nException: {0}", ex.Message);
			}

			var catchCont = ints1to5ex.Catch(Observable.Range(-1, 1));
			Dump(catchCont);

			var bufferCount = ints1to9.Buffer(4);
			Dump(bufferCount);

			var latest = ints1to9.Delay(TimeSpan.FromSeconds(0.1)).Latest();
			var enLatest = latest.GetEnumerator();
			enLatest.MoveNext();
			Console.WriteLine("{0}: {1}", nameof(latest), enLatest.Current);

			var mostRecent = chars.Delay(TimeSpan.FromSeconds(0.1)).MostRecent('@');
			var enRecent = mostRecent.GetEnumerator();
			enRecent.MoveNext();
			Console.WriteLine("{0}: {1}", nameof(mostRecent), enRecent.Current);
			Task.Delay(200).ContinueWith(_ =>
				{ enRecent.MoveNext(); Console.WriteLine("{0}: {1}", nameof(mostRecent), enRecent.Current); })
				.Wait();

			var chunk = ints1to9.Chunkify();
			Dump(chunk);

			var delay = chars.Delay(TimeSpan.FromSeconds(1));
			Dump(delay);

			var delaySubscr = chars.DelaySubscription(TimeSpan.FromSeconds(1));
			Dump(delaySubscr);

			var forkJoin = ints1to9.ForkJoin(chars, (x, y) => string.Format($"{x}{y}"));
			Dump(forkJoin);

			var awaiter = chars.GetAwaiter();
			Console.WriteLine("awaiter: {0}", awaiter.GetResult());

			Console.Write("next: ");
			foreach (var v in chars.Next())
				Console.Write("{0} ", v);
			Console.WriteLine();
		}

		#endregion


		#region Utility

		private static void Dump<T>(IObservable<T> seq)
		{
			// Get parameter name
			Console.Write("{0}: ", GetParameterName(1));

			// Show the sequence
			var ev = new ManualResetEvent(false);

			using (var s = seq.Subscribe(a => DumpValue(a), () => ev.Set()))
			{
				ev.WaitOne();
			}

			Console.WriteLine();
		}


		private static void Dump<T>(IEnumerable<T> seq)
		{
			// Get parameter name
			Console.Write("{0}: ", GetParameterName(1));

			// Show the sequence
			foreach (var s in seq)
			{
				DumpValue(s);
			}

			Console.WriteLine();
		}


		private static string GetParameterName(int level = 1)
		{
			var stackFrame = new StackTrace(true).GetFrame(level + 1);
			string fileName = stackFrame.GetFileName();
			int lineNumber = stackFrame.GetFileLineNumber();

			var file = new System.IO.StreamReader(fileName);
			for (int i = 0; i < lineNumber - 1; i++)
			{
				file.ReadLine();
			}
			var varName = file.ReadLine().Split(new char[] { '(', ')' })[1];
			return varName;
		}


		private static void DumpValue<T>(T a)
		{
			if (typeof(T).IsGenericType &&
				typeof(System.Collections.IEnumerable).IsAssignableFrom(typeof(T).GetGenericTypeDefinition()))
			{
				var asList = new List<object>();
				foreach (var v in (System.Collections.IEnumerable) a)
				{
					asList.Add(v);
				}

				Console.Write("{{{0}}} ", string.Join(",", asList));
			}
			else if (typeof(T).IsGenericType &&
				typeof(IGroupedObservable<,>).IsAssignableFrom(typeof(T).GetGenericTypeDefinition()))
			{
				var typeArgs = typeof(T).GetGenericArguments();
				var methodDef = typeof(Program).GetMethod(nameof(DumpGroup), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				var method = methodDef.MakeGenericMethod(typeArgs);
				method.Invoke(null, new object[] { a });
			}
			else
			{
				Console.Write("{0} ", a);
			}
		}


		private static void DumpGroup<T1, T2>(IGroupedObservable<T1, T2> seq)
		{
			Task.Factory.StartNew(() =>
			{
				Console.Write($"Key={seq.Key}: ");
				var ev = new ManualResetEvent(false);

				using (var s = seq.Subscribe(a => DumpValue(a), () => ev.Set()))
				{
					ev.WaitOne();
				}
			});
		}

		#endregion
	}
}
