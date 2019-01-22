using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

namespace SSACCV_WPF
{
	public partial class MainWindow : Window
	{
		public static MainWindow Instance;

		CsvManager csvManager;
		ParticipantManager participantManager;

		public MainWindow()
		{
			Instance = this;
			Console.Clear();
			Console.SetWindowSize(80, 30);
			Console.SetBufferSize(80, 30);
			Console.CursorVisible = false;

			Affectors.Instance = new Affectors();

			csvManager = new CsvManager();
			participantManager = new ParticipantManager();

			InitializeComponent();
			PopulateAffectorPanel();

			InputFilePath.HeaderLabel.Text = "Input file:";
			InputFilePath.LoadRequested += LoadData;

			CalcPathButton.IsEnabled = CalcStayButton.IsEnabled = false;

			CsvManager.BusyChanged += b => { CalcPathButton.IsEnabled = CalcStayButton.IsEnabled = !b; };
		}

		public void SetOpacity(double opacity)
		{
			Opacity = opacity;
		}

		void LoadData(string path)
		{
			SetOpacity(0.5d);
			Task<List<DataPoint>> read = csvManager.ReadAsync(path);
			read.Wait();

			List<DataPoint> points = read.Result;

			if (points?.Count > 0)
				participantManager.AddDataPoints(points);
		}

		void OnCloseClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		void PopulateAffectorPanel()
		{
			List<PropertyInfo> properties = typeof(Affectors).GetProperties().ToList();

			foreach (PropertyInfo property in properties)
			{
				object value = property.GetValue(Affectors.Instance);

				if (property.PropertyType == typeof(double))
				{
					AffectorPanel.Children.Add(new AffectorControl(property.Name, (double)value, property));
				}
				else if (property.PropertyType == typeof(decimal))
				{
					AffectorPanel.Children.Add(new AffectorControl(property.Name, (decimal)value, property));
				}
				else if (property.PropertyType == typeof(int))
				{
					AffectorPanel.Children.Add(new AffectorControl(property.Name, (int)value, property));
				}
			}
		}

		#region Staypoint button

		void StayClick(object sender, RoutedEventArgs e)
		{
			if (!participantManager.ReadyForCalc)
				return;

			Opacity = 0.5d;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "CSV (*.csv)|*.csv";
				dialog.FileName = "Staypoints_" + DateTime.Today.Date.ToString("MMMdd") + "_" + Affectors.Instance.GetStaypointIdentityString() + ".csv";
				string path = "";

				if (dialog.ShowDialog() == true)
				{
					path = dialog.FileName;

					csvManager.Write(path, participantManager.GetStayPointOutput());
				}
			}
			catch
			{
				Console.Error.WriteLine("Error calculating staypoints.");
			}

			Opacity = 1d;
		}

		void StayMouseEnter(object sender, MouseEventArgs e)
		{
			CalcStayBorder.Background = (Brush)TryFindResource("Primary-Dark-Brush");
		}

		void StayMouseLeave(object sender, MouseEventArgs e)
		{
			CalcStayBorder.Background = (Brush)TryFindResource("Primary-Brush");
		}

		#endregion

		#region Path button

		void PathClick(object sender, RoutedEventArgs e)
		{
			if (!participantManager.ReadyForCalc)
				return;

			Opacity = 0.5d;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "CSV (*.csv)|*.csv";
				dialog.FileName = "Paths_" + DateTime.Today.Date.ToString("MMMdd") + "_" + Affectors.Instance.GetPathIdentityString() + ".csv";
				string path = "";

				if (dialog.ShowDialog() == true)
				{
					path = dialog.FileName;

					csvManager.Write(path, participantManager.GetPathOutput());
				}
			}
			catch
			{
				Console.Error.WriteLine("Error calculating paths.");
			}

			Opacity = 1d;
		}

		void PathMouseEnter(object sender, MouseEventArgs e)
		{
			CalcPathBorder.Background = (Brush)TryFindResource("Primary-Dark-Brush");
		}

		void PathMouseLeave(object sender, MouseEventArgs e)
		{
			CalcPathBorder.Background = (Brush)TryFindResource("Primary-Brush");
		}

		#endregion

	}
}
