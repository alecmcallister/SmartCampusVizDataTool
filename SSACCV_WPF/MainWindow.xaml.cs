using Microsoft.Win32;
using SSACCV_WPF.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
			Console.SetWindowSize(80, 40);
			Console.SetBufferSize(80, 40);
			Console.CursorVisible = false;

			Affectors.Instance = new Affectors();

			csvManager = new CsvManager();
			participantManager = new ParticipantManager();

			InitializeComponent();
			PopulateAffectorPanel();

			InputFilePath.HeaderLabel.Text = "Input file:";
			InputFilePath.LoadRequested += LoadData;

			CalcPathButton.IsEnabled = CalcStayButton.IsEnabled = CalcAnonStayButton.IsEnabled = CalcAnonPathButton.IsEnabled = false;

			CsvManager.BusyChanged += b => { CalcPathButton.IsEnabled = CalcStayButton.IsEnabled = CalcAnonStayButton.IsEnabled = CalcAnonPathButton.IsEnabled = !b; };
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

		#region Restpoint button

		void StayClick(object sender, RoutedEventArgs e)
		{
			if (!participantManager.ReadyForCalc)
				return;

			Opacity = 0.5d;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "CSV (*.csv)|*.csv";
				dialog.FileName = "Restpoints_" + DateTime.Today.Date.ToString("MMMdd") + "_" + Affectors.Instance.GetRestpointIdentityString() + ".csv";
				string path = "";

				if (dialog.ShowDialog() == true)
				{
					path = dialog.FileName;

					csvManager.Write(path, participantManager.GetRestpointOutput());
				}
			}
			catch
			{
				Console.Error.WriteLine("Error calculating restpoints.");
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

		#region Anon Restpoint button

		void AnonStayClick(object sender, RoutedEventArgs e)
		{
			if (!participantManager.ReadyForCalc)
				return;

			Opacity = 0.5d;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "CSV (*.csv)|*.csv";
				dialog.FileName = "Restpoints_" + DateTime.Today.Date.ToString("MMMdd") + "_" + Affectors.Instance.GetRestpointIdentityString() + "_ANON.csv";
				string path = "";

				if (dialog.ShowDialog() == true)
				{
					path = dialog.FileName;

					csvManager.Write(path, participantManager.GetAnonRestpointOutput());
				}
			}
			catch
			{
				Console.Error.WriteLine("Error calculating anon restpoints.");
			}

			Opacity = 1d;
		}

		void AnonStayMouseEnter(object sender, MouseEventArgs e)
		{
			AnonStayBorder.Background = (Brush)TryFindResource("Primary-Dark-Brush");
		}

		void AnonStayMouseLeave(object sender, MouseEventArgs e)
		{
			AnonStayBorder.Background = (Brush)TryFindResource("Primary-Brush");
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

		#region Anon Path button

		void AnonPathClick(object sender, RoutedEventArgs e)
		{
			if (!participantManager.ReadyForCalc)
				return;

			Opacity = 0.5d;

			try
			{
				SaveFileDialog dialog = new SaveFileDialog();
				dialog.Filter = "CSV (*.csv)|*.csv";
				dialog.FileName = "Paths_" + DateTime.Today.Date.ToString("MMMdd") + "_" + Affectors.Instance.GetPathIdentityString() + "_ANON.csv";
				string path = "";

				if (dialog.ShowDialog() == true)
				{
					path = dialog.FileName;

					csvManager.Write(path, participantManager.GetAnonPathOutput());
				}
			}
			catch
			{
				Console.Error.WriteLine("Error calculating anon paths.");
			}

			Opacity = 1d;
		}

		void AnonPathMouseEnter(object sender, MouseEventArgs e)
		{
			CalcAnonPathBorder.Background = (Brush)TryFindResource("Primary-Dark-Brush");
		}

		void AnonPathMouseLeave(object sender, MouseEventArgs e)
		{
			CalcAnonPathBorder.Background = (Brush)TryFindResource("Primary-Brush");
		}

		#endregion

	}
}
