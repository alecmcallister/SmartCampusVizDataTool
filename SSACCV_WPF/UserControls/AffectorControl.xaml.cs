using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
	public partial class AffectorControl : UserControl
	{
		PropertyInfo Property;

		public AffectorControl()
		{
			InitializeComponent();
		}

		public AffectorControl(string affectorName, double defaultValue, PropertyInfo property)
		{
			InitializeComponent();

			AffectorName.Text = affectorName;
			Field.DefaultValue = defaultValue;
			Property = property;

			Field.TextChangedEvent += d => { Property.SetValue(Affectors.Instance, double.Parse(d == "" || d == "." ? "0" : d)); };
		}

		public AffectorControl(string affectorName, decimal defaultValue, PropertyInfo property)
		{
			InitializeComponent();

			AffectorName.Text = affectorName;
			Field.DefaultValue = (double)defaultValue;
			Property = property;

			Field.TextChangedEvent += d => { Property.SetValue(Affectors.Instance, decimal.Parse(d == "" || d == "." ? "0" : d)); };
		}

		public AffectorControl(string affectorName, int defaultValue, PropertyInfo property)
		{
			InitializeComponent();

			AffectorName.Text = affectorName;
			Field.DefaultValue = defaultValue;
			Property = property;

			Field.TextChangedEvent += d => { int value = (int)double.Parse(d == "" || d == "." ? "0" : d); Property.SetValue(Affectors.Instance, value); Field.Text = value.ToString(); };
		}

		void ResetButtonClicked(object sender, RoutedEventArgs e)
		{
			Field.Text = Field.DefaultValue.ToString();
		}
	}
}
