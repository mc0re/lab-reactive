using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;


namespace ReactiveDrawWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private IDisposable mCanvasSubscription;


		public MainWindow()
		{
			InitializeComponent();
			this.Closed += WindowClosed;
			SubscribeMouse(TraceCanvas);
		}


		private void WindowClosed(object sender, EventArgs args)
		{
			mCanvasSubscription.Dispose();
			mCanvasSubscription = null;
		}


		private void SubscribeMouse(Canvas e)
		{
			// Get the initial position and dragged points using LINQ to Events
			var mouseDragPoints =
				from md in e.GetMouseDown()
				let startpos = md.GetPosition(e)
				from mm in e.GetMouseMove().
					Sample(TimeSpan.FromSeconds(0.05)).
					TakeUntil(e.GetMouseUp()).
					ObserveOnDispatcher()
				select new
				{
					StartPos = startpos,
					CurrentPos = mm.GetPosition(e)
				};

			// Subscribe and draw a line from start position to current position
			mCanvasSubscription = mouseDragPoints.SubscribeOnDispatcher().Subscribe(item =>
				{
					e.Children.Add(new Line()
					{
						Stroke = Brushes.Red,
						X1 = item.StartPos.X,
						X2 = item.CurrentPos.X,
						Y1 = item.StartPos.Y,
						Y2 = item.CurrentPos.Y
					});

					var ellipse = new Ellipse()
					{
						Stroke = Brushes.Blue,
						StrokeThickness = 10,
						Fill = Brushes.Blue
					};
					Canvas.SetLeft(ellipse, item.CurrentPos.X);
					Canvas.SetTop(ellipse, item.CurrentPos.Y);
					e.Children.Add(ellipse);
				});
		}
	}
}
