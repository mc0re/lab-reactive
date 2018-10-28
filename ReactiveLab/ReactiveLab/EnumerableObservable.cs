using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;


namespace ReactiveLab
{
	internal class EnumerableObservable
	{
		internal static void Run()
		{
			IEnumerable<int> someInts = new List<int> { 1, 2, 3, 4, 5 };

			// To convert a generic IEnumerable into an IObservable, use the ToObservable extension method.
			var observable = someInts.ToObservable();
			observable.Subscribe(a => Console.Write("{0} ", a));
			Thread.Sleep(200);
			Console.WriteLine();
		}
	}
}