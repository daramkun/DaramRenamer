using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Daramee.Winston.Dialogs;
using DaramRenamer.Converters;

namespace DaramRenamer
{
	public partial class CommandWindow : Window
	{
		public ICommand Command { get; }
		public ICondition Condition { get; }

		public event EventHandler ValueChanged;

		public CommandWindow(ICommand command)
		{
			InitializeComponent();
			Command = command;

			Title = new LocalizationConverter().Convert(Command, null, null, null) as string;

			Initialize(command, command.GetType());
		}

		public CommandWindow(ICondition condition)
		{
			InitializeComponent();
			Condition = condition;

			Title = new LocalizationConverter ().Convert (Condition, null, null, null) as string;

			Initialize (condition, condition.GetType ());
		}

		private void Initialize(object obj, Type type)
		{
			var props = type.GetProperties();

			var propDict =
				(from prop in props
					let attrs = prop.GetCustomAttributes(typeof(LocalizationKeyAttribute), true)
					where attrs.Length > 0
					select prop).ToList();

			for (var i = 0; i < propDict.Count; ++i)
				contentGrid.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(24)});

			TextBox firstTextBox = null;

			foreach (var propPair in propDict)
			{
				var prop = propPair;
				var attrs = prop.GetCustomAttributes(typeof(LocalizationKeyAttribute), true);
				var textBlock = new TextBlock
				{
					Text = Strings.Instance[(attrs[0] as LocalizationKeyAttribute).LocalizationKey],
					VerticalAlignment = VerticalAlignment.Center
				};
				Grid.SetRow(textBlock, propDict.IndexOf(propPair));
				Grid.SetColumn(textBlock, 0);
				contentGrid.Children.Add(textBlock);

				FrameworkElement control = null;

				if (prop.PropertyType == typeof(string) || prop.PropertyType == typeof(Regex))
				{
					control = new TextBox()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					(control as TextBox).Text = prop.GetValue(obj) as string;
					(control as TextBox).TextChanged += (sender, e) =>
					{
						var text = (sender as TextBox)?.Text;
						try
						{
							prop.SetValue(obj,
								prop.PropertyType == typeof(Regex) ? new Regex(text ?? string.Empty) : text);
						}
						catch
						{
							// ignored
						}

						OnValueChanged(sender, e);
					};

					firstTextBox ??= control as TextBox;
				}
				else if (prop.PropertyType == typeof(bool))
				{
					control = new CheckBox()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					((CheckBox) control).IsChecked = (bool?) prop.GetValue(obj) == true;
					((CheckBox) control).Checked += (sender, e) =>
					{
						prop.SetValue(obj, true);
						OnValueChanged(sender, e);
					};
					((CheckBox) control).Unchecked += (sender, e) =>
					{
						prop.SetValue(obj, false);
						OnValueChanged(sender, e);
					};
				}
				else if (prop.PropertyType == typeof(bool?))
				{
					control = new CheckBox()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					var checkBox = (CheckBox) control;
					checkBox.IsThreeState = true;
					((CheckBox)control).IsChecked = (bool?)prop.GetValue(obj);
					checkBox.Checked += (sender, e) =>
					{
						checkBox.IsChecked = checkBox.IsChecked switch
						{
							null => false,
							true => true,
							false => null
						};
						e.Handled = true;
						OnValueChanged(sender, e);
					};
				}
				else if (prop.PropertyType == typeof(uint) || prop.PropertyType == typeof(uint?))
				{
					var container = new Grid()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					container.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(9, GridUnitType.Pixel)});
					container.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(9, GridUnitType.Pixel)});
					container.ColumnDefinitions.Add(new ColumnDefinition());
					container.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(24)});
					var text = new TextBox
					{
						VerticalAlignment = VerticalAlignment.Center,
						Text = prop.GetValue(obj)?.ToString()
					};
					text.PreviewTextInput += (sender, e) => { e.Handled = Regex.IsMatch(e.Text, "[^0-9]"); };
					text.TextChanged += (sender, e) =>
					{
						if (text.Text.Trim() == string.Empty)
							text.Text = prop.PropertyType == typeof(uint) ? "0" : string.Empty;
						if (text.Text != string.Empty)
						{
							if (uint.TryParse(text.Text, out var parsed))
								prop.SetValue(obj, parsed);
							OnValueChanged(sender, e);
						}
						else
						{
							prop.SetValue(obj, null);
						}
					};
					firstTextBox ??= text;
					Grid.SetRowSpan(text, 2);
					container.Children.Add(text);

					var upButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Bottom,
						Content = "▲",
						FontSize = 5.5
					};
					upButton.Click += (sender, e) =>
					{
						if (text.Text.Trim() == string.Empty)
							text.Text = "0";
						var num = uint.Parse(text.Text) + 1;
						if (num == uint.MinValue) num = uint.MaxValue;
						prop.SetValue(obj, num);
						text.Text = num.ToString();
					};
					Grid.SetRow(upButton, 0);
					Grid.SetColumn(upButton, 1);
					container.Children.Add(upButton);

					var downButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Top,
						Content = "▼",
						FontSize = 5.5
					};
					downButton.Click += (sender, e) =>
					{
						if (text.Text.Trim() == "")
							text.Text = prop.PropertyType == typeof(uint) ? "0" : string.Empty;
						if (text.Text != string.Empty || (text.Text == "0" && prop.PropertyType != typeof(uint?)))
						{
							var num = uint.Parse(text.Text) - 1;
							if (num == uint.MaxValue)
								num = uint.MinValue;
							prop.SetValue(obj, num);
							text.Text = num.ToString();
						}
						else
						{
							prop.SetValue(obj, null);
						}
					};
					Grid.SetRow(downButton, 1);
					Grid.SetColumn(downButton, 1);
					container.Children.Add(downButton);

					control = container;
				}
				else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
				{
					var container = new Grid()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					container.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(9, GridUnitType.Pixel)});
					container.RowDefinitions.Add(new RowDefinition() {Height = new GridLength(9, GridUnitType.Pixel)});
					container.ColumnDefinitions.Add(new ColumnDefinition());
					container.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(24)});
					var text = new TextBox()
					{
						VerticalAlignment = VerticalAlignment.Center,
						Text = prop.GetValue(obj)?.ToString()
					};
					text.PreviewTextInput += (sender, e) => { e.Handled = !Regex.IsMatch(e.Text, "[\\-0-9][0-9]*"); };
					text.TextChanged += (sender, e) =>
					{
						if (text.Text.Trim() == string.Empty)
							text.Text = prop.PropertyType == typeof(int) ? "0" : string.Empty;
						if (int.TryParse(text.Text, out var parsed))
							prop.SetValue(obj, parsed);
						OnValueChanged(sender, e);
					};
					firstTextBox ??= text;
					Grid.SetRowSpan(text, 2);
					container.Children.Add(text);

					var upButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Bottom,
						Content = "▲",
						FontSize = 5.5
					};
					upButton.Click += (sender, e) =>
					{
						if (text.Text.Trim() == string.Empty)
							text.Text = "0";
						var num = int.Parse(text.Text) + 1;
						if (num == int.MinValue) num = int.MaxValue;
						text.Text = num.ToString();
						prop.SetValue(obj, num);
						text.Text = num.ToString();
					};
					Grid.SetRow(upButton, 0);
					Grid.SetColumn(upButton, 1);
					container.Children.Add(upButton);

					var downButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Top,
						Content = "▼",
						FontSize = 5.5
					};
					downButton.Click += (sender, e) =>
					{
						if (text.Text.Trim() == string.Empty)
							text.Text = prop.PropertyType == typeof(int) ? "0" : string.Empty;
						if (text.Text != string.Empty)
						{
							var num = int.Parse(text.Text) - 1;
							if (num == int.MaxValue) num = int.MinValue;
							text.Text = num.ToString();
							prop.SetValue(obj, num);
							text.Text = num.ToString();
						}
						else
						{
							prop.SetValue(obj, null);
						}
					};
					Grid.SetRow(downButton, 1);
					Grid.SetColumn(downButton, 1);
					container.Children.Add(downButton);

					control = container;
				}
				else if (prop.PropertyType == typeof(Position1) || prop.PropertyType == typeof(Position2))
				{
					control = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						VerticalAlignment = VerticalAlignment.Center
					};
					var radioPre = new RadioButton()
					{
						Content = Strings.Instance[Position2.StartPoint.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioPre.Checked += (sender, e) =>
					{
						prop.SetValue(obj, Position1.StartPoint);
						OnValueChanged(sender, e);
					};
					radioPre.IsChecked = ((Position1) (int) prop?.GetValue(obj)) == Position1.StartPoint;
					((StackPanel) control).Children.Add(radioPre);
					var radioPost = new RadioButton()
					{
						Content = Strings.Instance[Position2.EndPoint.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioPost.Checked += (sender, e) =>
					{
						prop.SetValue(obj, Position1.EndPoint);
						OnValueChanged(sender, e);
					};
					radioPost.IsChecked = ((Position1) (int) prop?.GetValue(obj)) == Position1.EndPoint;
					((StackPanel) control).Children.Add(radioPost);
				}
				else if (prop.PropertyType == typeof(Position2))
				{
					var radioBoth = new RadioButton()
					{
						Content = Strings.Instance[Position2.BothPoint.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioBoth.Checked += (sender, e) => { prop.SetValue(obj, Position2.BothPoint); OnValueChanged(sender, e); };
					radioBoth.IsChecked = ((Position2) prop?.GetValue(obj)) == Position2.BothPoint;
					(control as StackPanel).Children.Add(radioBoth);
				}
				else if (prop.PropertyType == typeof(Casecast2) || prop.PropertyType == typeof(Casecast1))
				{
					control = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						VerticalAlignment = VerticalAlignment.Center
					};
					var radioLower = new RadioButton()
					{
						Content = Strings.Instance[Casecast2.LowercaseAll.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioLower.Checked += (sender, e) =>
					{
						prop.SetValue(obj, Casecast1.LowercaseAll);
						OnValueChanged(sender, e);
					};
					radioLower.IsChecked = ((Casecast1) prop?.GetValue(obj)) == Casecast1.LowercaseAll;
					((StackPanel) control).Children.Add(radioLower);
					var radioUpper = new RadioButton()
					{
						Content = Strings.Instance[Casecast2.UppercaseAll.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioUpper.Checked += (sender, e) =>
					{
						prop.SetValue(obj, Casecast1.UppercaseAll);
						OnValueChanged(sender, e);
					};
					radioUpper.IsChecked = ((Casecast1) prop?.GetValue(obj)) == Casecast1.UppercaseAll;
					((StackPanel) control).Children.Add(radioUpper);
				}
				else if (prop.PropertyType == typeof(Casecast2))
				{
					var radioUpperFirstLetter = new RadioButton()
					{
						Content = Strings.Instance[Casecast2.UppercaseFirstLetter.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioUpperFirstLetter.Checked += (sender, e) =>
					{
						prop.SetValue(obj, Casecast2.UppercaseFirstLetter);
						OnValueChanged(sender, e);
					};
					radioUpperFirstLetter.IsChecked =
						((Casecast2) prop?.GetValue(obj)) == Casecast2.UppercaseFirstLetter;
					(control as StackPanel)?.Children.Add(radioUpperFirstLetter);
				}
				else if (prop.PropertyType == typeof(DirectoryInfo))
				{
					var grid = new Grid();
					grid.ColumnDefinitions.Add(new ColumnDefinition());
					grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(32)});
					var textBox = new TextBox()
					{
						IsReadOnly = true,
						Text = (prop.GetValue(obj) as DirectoryInfo).FullName,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(0, 0, 5, 0)
					};
					textBox.TextChanged += (sender, e) =>
					{
						prop.SetValue(obj, new DirectoryInfo(textBox.Text));
						OnValueChanged(sender, e);
					};
					firstTextBox ??= textBox;
					grid.Children.Add(textBox);
					var button = new Button()
					{
						Style = this.Resources["ButtonStyle"] as Style,
						Content = "...",
						VerticalAlignment = VerticalAlignment.Center
					};
					button.Click += (sender, e) =>
					{
						FileDialog fbd = new OpenFolderDialog()
						{
							InitialDirectory = textBox.Text
						};
						if (fbd.ShowDialog() == true)
						{
							textBox.Text = fbd.FileName;
							prop.SetValue(obj, new DirectoryInfo(textBox.Text));
						}
					};
					Grid.SetColumn(button, 1);
					grid.Children.Add(button);
					control = grid;
				}
				else if (prop.PropertyType == typeof(FileDateKind))
				{
					control = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						VerticalAlignment = VerticalAlignment.Center
					};
					var radioCreated = new RadioButton()
					{
						Content = Strings.Instance[FileDateKind.Creation.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioCreated.Checked += (sender, e) =>
					{
						prop.SetValue(obj, FileDateKind.Creation);
						OnValueChanged(sender, e);
					};
					radioCreated.IsChecked = ((FileDateKind) prop?.GetValue(obj)) == FileDateKind.Creation;
					((StackPanel) control).Children.Add(radioCreated);
					var radioModified = new RadioButton()
					{
						Content = Strings.Instance[FileDateKind.LastModify.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioModified.Checked += (sender, e) =>
					{
						prop.SetValue(obj, FileDateKind.LastModify);
						OnValueChanged(sender, e);
					};
					radioModified.IsChecked = ((FileDateKind) prop?.GetValue(obj)) == FileDateKind.LastModify;
					((StackPanel) control).Children.Add(radioModified);
					var radioAccessed = new RadioButton()
					{
						Content = Strings.Instance[FileDateKind.LastAccess.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioAccessed.Checked += (sender, e) =>
					{
						prop.SetValue(obj, FileDateKind.LastAccess);
						OnValueChanged(sender, e);
					};
					radioAccessed.IsChecked = ((FileDateKind) prop?.GetValue(obj)) == FileDateKind.LastAccess;
					((StackPanel) control).Children.Add(radioAccessed);
					var radioNow = new RadioButton()
					{
						Content = Strings.Instance[FileDateKind.Now.ToString()],
						Margin = new Thickness(0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioNow.Checked += (sender, e) =>
					{
						prop.SetValue(obj, FileDateKind.Now);
						OnValueChanged(sender, e);
					};
					radioNow.IsChecked = ((FileDateKind) prop?.GetValue(obj)) == FileDateKind.Now;
					((StackPanel) control).Children.Add(radioNow);
				}
				else if (prop.PropertyType.IsEnum)
				{
					var comboBox = new ComboBox()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach (var item in Enum.GetValues(prop.PropertyType))
						comboBox.Items.Add(Strings.Instance[item.ToString()]);
					comboBox.SelectedIndex = Convert.ToInt32(prop.GetValue(obj) ?? 0);
					comboBox.SelectionChanged += (sender, e) =>
					{
						prop.SetValue(obj, Enum.ToObject(prop.PropertyType, comboBox.SelectedIndex));
						OnValueChanged(sender, e);
					};

					control = comboBox;
				}

				if (control == null)
					continue;

				Grid.SetRow(control, propDict.IndexOf(propPair));
				Grid.SetColumn(control, 1);

				contentGrid.Children.Add(control);
			}

			firstTextBox?.Focus();
		}

		private void ButtonOK_Click (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void ButtonCancel_Click (object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void OnValueChanged(object sender, RoutedEventArgs e)
		{
			ValueChanged?.Invoke(sender, e);
		}
	}
}
