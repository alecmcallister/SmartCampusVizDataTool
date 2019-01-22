using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
	public partial class InputField : UserControl
	{
		public event Action<string> SubmitEvent = new Action<string>(s => { });
		public event Action<string> TextChangedEvent = new Action<string>(s => { });

		public bool HasText { get { return TextBox?.Text.Length > 0; } }

		public string Text { get { return TextBox?.Text; } set { TextBox.Text = value; } }

		double _defaultValue = 0;
		public double DefaultValue { get { return _defaultValue; } set { _defaultValue = value; TextBox.Text = value.ToString(); } }

		public double CurrentValue { get { return double.Parse(TextBox.Text); } set { TextBox.Text = value.ToString(); } }

		public InputField()
		{
			InitializeComponent();
		}

		public void SetReadOnly(bool value)
		{
			TextBox.IsReadOnly = value;
		}

		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);
			TextBox.SelectAll();
		}

		void TextChanged(object sender, TextChangedEventArgs e)
		{
			TextChangedEvent(Text);
		}

		void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.Key == Key.Enter || e.Key == Key.Return) && HasText)
				SubmitEvent(Text);
		}

		void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsTextAllowed(e.Text);
		}

		static Regex _regex = new Regex("[^0-9.-]+");
		static bool IsTextAllowed(string text)
		{
			return !_regex.IsMatch(text);
		}
	}
}
