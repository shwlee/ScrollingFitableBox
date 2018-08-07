using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ZoomableCanvasTest
{
	public class ZoomableCanvas : Canvas
	{
		public static DependencyProperty ZoomFactorProperty = 
			DependencyProperty.Register(
				"ZoomFactor", 
				typeof(double), 
				typeof(ZoomableCanvas), 
				new FrameworkPropertyMetadata(
					1d, 
					FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
					OnZoomFactorChanged));

		private static void OnZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is ZoomableCanvas zoomableCanvas))
			{
				return;
			}

			zoomableCanvas.InvalidateVisual();
		}

		public double ZoomFactor
		{
			get => (double) this.GetValue(ZoomFactorProperty);
			set => this.SetValue(ZoomFactorProperty, value);
		}

		static ZoomableCanvas()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomableCanvas), new FrameworkPropertyMetadata(typeof(ZoomableCanvas)));
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			Size measureSize = new Size(availableSize.Width / ZoomFactor, availableSize.Height / ZoomFactor);
			Size baseSize = base.MeasureOverride(measureSize);

			return new Size(baseSize.Width * ZoomFactor, baseSize.Height * ZoomFactor);
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			Size arrangeSize = new Size(finalSize.Width / ZoomFactor, finalSize.Height / ZoomFactor);
			base.ArrangeOverride(arrangeSize);

			return finalSize;
		}

		//protected override Size MeasureOverride(Size constraint)
		//{
		//	double bottomMost = 0d;
		//	double rightMost = 0d;

		//	foreach (object obj in Children)
		//	{
		//		FrameworkElement child = obj as FrameworkElement;

		//		if (child != null)
		//		{
		//			child.Measure(constraint);


		//			if (double.IsNaN(child.DesiredSize.Height) || double.IsNaN(child.DesiredSize.Width))
		//			{
		//				continue;
		//			}

		//			if (double.IsNaN(GetTop(child)) || double.IsNaN(GetLeft(child)))
		//			{
		//				continue;
		//			}

		//			bottomMost = Math.Max(bottomMost, GetTop(child) + child.DesiredSize.Height);
		//			rightMost = Math.Max(rightMost, GetLeft(child) + child.DesiredSize.Width);
		//		}
		//	}
		//	return new Size(rightMost, bottomMost);
		//}
	}
}
