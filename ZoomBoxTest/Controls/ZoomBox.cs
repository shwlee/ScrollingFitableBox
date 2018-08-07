using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZoomBoxTest.Controls
{
	/// <summary>
	/// </summary>
	[TemplatePart(Name = "PART_ZoomableCanvas", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Contents", Type = typeof(ContentControl))]
	[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
	public class ZoomBox : Control
	{
		#region DPs

		public static DependencyProperty ContentsProperty =
			DependencyProperty.Register("Contents",
				typeof(object),
				typeof(ZoomBox),
				new UIPropertyMetadata((object)null, OnContentsPropertyChanged));

		[Bindable(true)]
		public object Contents
		{
			get => this.GetValue(ContentsProperty);
			set => this.SetValue(ContentsProperty, value);
		}

		private static void OnContentsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is ZoomBox zoomBox))
			{
				return;
			}

			if (zoomBox._scrollViewer == null)
			{
				return;
			}

			// TODO : scrollbar 부분 크기를 감안한 처리 추가.
			zoomBox.RecalcViewport(new Size(zoomBox._scrollViewer.ActualWidth, zoomBox._scrollViewer.ActualHeight));
		}

		public static DependencyProperty FitModeProperty =
			DependencyProperty.Register("FitMode",
				typeof(FitModes),
				typeof(ZoomBox),
				new UIPropertyMetadata(FitModes.None, OnFitModePropertyChanged));

		public FitModes FitMode
		{
			get => (FitModes)this.GetValue(FitModeProperty);
			set => this.SetValue(FitModeProperty, value);
		}

		private static void OnFitModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is ZoomBox zoomBox))
			{
				return;
			}

			if (zoomBox._scrollViewer == null)
			{
				return;
			}

			zoomBox.RecalcViewport(new Size(zoomBox._scrollViewer.ActualWidth, zoomBox._scrollViewer.ActualHeight));
		}

		public static DependencyProperty ZoomProperty =
			DependencyProperty.Register(
				"Zoom",
				typeof(double),
				typeof(ZoomBox),
				new FrameworkPropertyMetadata(
					100d,
					FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
					OnZoomFactorPropertyChanged));

		public double Zoom
		{
			get => (double)this.GetValue(ZoomProperty);
			set => this.SetValue(ZoomProperty, value);
		}

		private static void OnZoomFactorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is ZoomBox zoomBox))
			{
				return;
			}

			if (zoomBox._scrollViewer == null)
			{
				return;
			}

			zoomBox.RecalcViewport(new Size(zoomBox._scrollViewer.ActualWidth, zoomBox._scrollViewer.ActualHeight));
		}

		#endregion

		#region Fields

		private Canvas _zoomableCanvas;

		private ContentControl _contentControl;

		private ScrollViewer _scrollViewer;

		private TranslateTransform _translateTransform = new TranslateTransform();

		private RotateTransform _rotateTransform = new RotateTransform();

		private ScaleTransform _zoomTransform = new ScaleTransform();

		private TransformGroup _transformGroup = new TransformGroup();

		#endregion

		#region Constructors

		static ZoomBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomBox), new FrameworkPropertyMetadata(typeof(ZoomBox)));
		}

		public ZoomBox()
		{
			this._transformGroup.Children.Add(this._translateTransform);
			this._transformGroup.Children.Add(this._rotateTransform);
			this._transformGroup.Children.Add(this._zoomTransform);
		}

		#endregion

		#region Properties

		public double ZoomFactor
		{
			get => this.Zoom / 100;
			set => this.Zoom = value * 100;
		}

		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.Focusable = true;

			this._scrollViewer = this.GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
			this._scrollViewer.SizeChanged += this.ScrollViewerOnSizeChanged;

			this._zoomableCanvas = this.GetTemplateChild("PART_ZoomableCanvas") as Canvas;
			this._contentControl = this.GetTemplateChild("PART_Contents") as ContentControl;
			this._contentControl.LayoutTransform = this._transformGroup;
		}

		// called on panel resize, zoom mode change
		private void RecalcViewport(Size parentSize, Size? childOriginSize = null)
		{
			parentSize.Width -= SystemParameters.VerticalScrollBarWidth;
			parentSize.Height -= SystemParameters.HorizontalScrollBarHeight;

			// contentControl의 크기를 직접 변경한 것이 아니라 scaling만 했으므로
			// 실제 contentControl의 크기를 바꾸지 않으면 ActualWidth.Height 를 항상 같다.
			//var childSize = new Size(this._contentControl.ActualWidth, this._contentControl.ActualHeight);
			var childSize = childOriginSize.HasValue == false ?
				new Size(this._contentControl.ActualWidth, this._contentControl.ActualHeight) :
				childOriginSize.Value;

			if (double.IsNaN(childSize.Width) || double.IsInfinity(childSize.Width) ||
			    double.IsNaN(childSize.Height) || double.IsInfinity(childSize.Height))
			{
				return;
			}

			if (Math.Abs(childSize.Width) < double.Epsilon || Math.Abs(childSize.Height) < double.Epsilon)
			{
				return;
			}

			double desiredWidth;
			double desiredHeight;

			double zoomX;
			double zoomY;

			double panX;
			double panY;

			double minDimension = 5;

			switch (this.FitMode)
			{
				case FitModes.None:
					break;
				case FitModes.Both:
					desiredWidth = parentSize.Width - this.Padding.Left - this.Padding.Right;
					if (desiredWidth < minDimension)
					{
						desiredWidth = minDimension;
					}

					zoomX = desiredWidth / childSize.Width;
					desiredHeight = parentSize.Height - this.Padding.Top - this.Padding.Bottom;

					if (desiredHeight < minDimension)
					{
						desiredHeight = minDimension;
					}

					zoomY = desiredHeight / childSize.Height;

					//if (zoomX <= zoomY)
					//{
					//	this.ZoomFactor = zoomX;
					//	panX = this.Padding.Left;
					//	panY = this.CalcCenterOffset(parentSize.Height, childSize.Height, this.Padding.Top);
					//}
					//else
					//{
					//	this.ZoomFactor = zoomY;
					//	panX = this.CalcCenterOffset(parentSize.Width, childSize.Width, this.Padding.Left);
					//	panY = this.Padding.Top;
					//}

					var zoom = zoomX <= zoomY ? zoomX : zoomY;

					//this.ApplyZoom(false, panX, panY);
					this.ApplyZoom(zoom, parentSize);

					break;
				case FitModes.Width:
					desiredWidth = parentSize.Width - this.Padding.Left - this.Padding.Right;
					if (desiredWidth < minDimension)
					{
						desiredWidth = minDimension;
					}

					zoomX = desiredWidth / childSize.Width;

					//this.ZoomFactor = zoomX;

					//panX = this.Padding.Left;
					//panY = this.CalcCenterOffset(parentSize.Height, childSize.Height, Padding.Top);

					//this.ApplyZoom(false, panX, panY);
					this.ApplyZoom(zoomX, childSize);

					break;
				case FitModes.Height:
					desiredHeight = parentSize.Height - this.Padding.Top - this.Padding.Bottom;
					if (desiredHeight < minDimension)
					{
						desiredHeight = minDimension;
					}

					zoomY = desiredHeight / childSize.Height;

					//this.ZoomFactor = zoomY;

					//panX = this.CalcCenterOffset(parentSize.Width, childSize.Width, this.Padding.Left);
					//panY = this.Padding.Top;

					//this.ApplyZoom(false, panX, panY);

					this.ApplyZoom(zoomY, childSize);

					break;
			}
		}

		protected double CalcCenterOffset(double parent, double child, double margin)
		{
			double offset = 0;
			offset = (parent - (child * this.ZoomFactor)) / 2;
			if (offset > margin)
			{
				return offset;
			}

			return margin;
		}

		protected void ApplyZoom(bool animate, double panX, double panY)
		{
			this._zoomTransform.ScaleX = this.ZoomFactor;
			this._zoomTransform.ScaleY = this.ZoomFactor;

			this._zoomableCanvas.Width = this._contentControl.ActualWidth * this.ZoomFactor;
			this._zoomableCanvas.Height = this._contentControl.ActualHeight * this.ZoomFactor;
		}

		private void ApplyZoom(double zoom, Size childSize)
		{
			this._zoomTransform.ScaleX = zoom;
			this._zoomTransform.ScaleY = zoom;

			this._zoomableCanvas.Width = childSize.Width * zoom;
			this._zoomableCanvas.Height = childSize.Height * zoom;
		}

		private void ScrollViewerOnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			this.RecalcViewport(e.NewSize);
		}
	}
}
