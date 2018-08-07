using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZoomBoxTest.Controls;

namespace ZoomBoxTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void FitAllButton_OnClick(object sender, RoutedEventArgs e)
		{
			this.xZoomBox.FitMode = FitModes.Both;
		}

		private void FitWidthButton_OnClick(object sender, RoutedEventArgs e)
		{
			this.xZoomBox.FitMode = FitModes.Width;
		}

		private void FitHeightButton_OnClick(object sender, RoutedEventArgs e)
		{
			this.xZoomBox.FitMode = FitModes.Height;
		}

		private void SetCanvasButton_OnClick(object sender, RoutedEventArgs e)
		{
			this.xCanvas.Width = double.Parse(this.xCanvasWidth.Text);
			this.xCanvas.Height = double.Parse(this.xCanvasHeight.Text);
		}
	}
}
