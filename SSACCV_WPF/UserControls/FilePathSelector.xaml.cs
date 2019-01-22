using Microsoft.Win32;
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

namespace SSACCV_WPF
{
	public partial class FilePathSelector : UserControl
	{
		public event Action<string> LoadRequested = new Action<string>(s => { });
		public string Path { get; set; }

		public FilePathSelector()
		{
			InitializeComponent();
			FilePath.SetReadOnly(true);
		}

		void FileDialogClick(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog();

			dialog.Filter = "CSV (*.csv)|*.csv|All files (*.*)|*.*";
			MainWindow.Instance.SetOpacity(0.5d);

			if (dialog.ShowDialog() == true)
			{
				Path = dialog.FileName;
				FilePath.Text = dialog.SafeFileName;
				LoadRequested(Path);
			}
			MainWindow.Instance.SetOpacity(1);
		}
	}
}
