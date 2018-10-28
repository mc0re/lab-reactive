using System;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading;


namespace ReactiveLab
{
	class AsyncModelObservable
	{
		public static void Run()
		{
			// We will use Stream's BeginRead and EndRead for this sample.
			const string input = "Non-ASCII символы";
			var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));

			var buf = new char[1];
			var read = Observable.Using(
				() => new StreamReader(inputStream),
				rdr => Observable.FromAsync(
					() => rdr.ReadAsync(buf, 0, 1))
					.Repeat()
					.TakeWhile(a => a > 0)
					.Select(a => buf[0]));

			read.Subscribe(b => Console.Write("{0} ", b));
			Thread.Sleep(200);
			Console.WriteLine();
		}
	}
}
