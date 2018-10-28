using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;


namespace ReactiveLab
{
	internal class GroupJoinObservable
	{
		internal static void Run()
		{
			var leftList = new List<string[]>
			{
				new string[] { "2013-01-01 02:00:00", "Batch1" },
				new string[] { "2013-01-01 03:00:00", "Batch2" },
				new string[] { "2013-01-01 04:00:00", "Batch3" }
			};

			var rightList = new List<string[]>
			{
				new string[] { "2013-01-01 01:00:00", "Production=2" },
				new string[] { "2013-01-01 02:00:00", "Production=0" },
				new string[] { "2013-01-01 03:00:00", "Production=3" }
			};

			var l = leftList.ToObservable();
			var r = rightList.ToObservable();

			var q = l.GroupJoin(r,
				_ => Observable.Never<Unit>(), // windows from each left event going on forever
				_ => Observable.Never<Unit>(), // windows from each right event going on forever
				(left, obsOfRight) => Tuple.Create(left, obsOfRight)); // create tuple of left event with observable of right events

			// e is a tuple with two items, left and obsOfRight
			q.Subscribe(e =>
			{
				var elem = e.Item2;
				elem.Where(x => x[0] == e.Item1[0]) // filter only when datetime matches
					.Subscribe(v =>
					{
						Console.WriteLine(
							string.Format("{0},{1} and {2},{3} occur at the same time",
							e.Item1[0],
							e.Item1[1],
							v[0],
							v[1]
						));
					});
			});
		}
	}
}