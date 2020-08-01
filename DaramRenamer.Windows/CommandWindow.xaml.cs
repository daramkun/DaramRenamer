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

		public CommandWindow (ICommand command)
		{
			InitializeComponent ();
			Command = command;

			Title = new LocalizationConverter().Convert(Command, null, null, null) as string;

			var type = command.GetType ();
			var props = type.GetProperties ();

			var propDict =
				(from prop in props
					let attrs = prop.GetCustomAttributes(typeof(LocalizationKeyAttribute), true)
					where attrs.Length > 0
					select prop).ToList();

			for (var i = 0; i < propDict.Count; ++i)
				contentGrid.RowDefinitions.Add (new RowDefinition () { Height = new GridLength (24) });

			foreach (var propPair in propDict)
			{
				var prop = propPair;
				var attrs = prop.GetCustomAttributes (typeof (LocalizationKeyAttribute), true);
				var textBlock = new TextBlock
				{
					Text = Strings.Instance[(attrs[0] as LocalizationKeyAttribute).LocalizationKey],
					VerticalAlignment = VerticalAlignment.Center
				};
				Grid.SetRow (textBlock, propDict.IndexOf (propPair));
				Grid.SetColumn (textBlock, 0);
				contentGrid.Children.Add (textBlock);

				FrameworkElement control = null;

				if (prop.PropertyType == typeof (string) || prop.PropertyType == typeof (Regex))
				{
					control = new TextBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					var binding = new Binding ()
					{
						Source = command,
						Path = new PropertyPath (prop.Name),
						Converter = prop.PropertyType == typeof (Regex) ? new RegexConverter () : null,
					};
					control.SetBinding (TextBox.TextProperty, binding);
				}
				if (prop.PropertyType == typeof (bool))
				{
					control = new CheckBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					var binding = new Binding ()
					{
						Source = command,
						Path = new PropertyPath (prop.Name)
					};
					control.SetBinding (ToggleButton.IsCheckedProperty, binding);
				}
				if (prop.PropertyType == typeof (bool?))
				{
					control = new CheckBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					var checkBox = control as CheckBox;
					checkBox.IsThreeState = true;
					checkBox.Checked += (sender, e) =>
					{
						checkBox.IsChecked = checkBox.IsChecked switch
						{
							null => false,
							true => true,
							false => null
						};
						e.Handled = true;
					};
				}
				if (prop.PropertyType == typeof (uint) || prop.PropertyType == typeof (uint?))
				{
					var container = new Grid ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					container.RowDefinitions.Add (new RowDefinition () { Height = new GridLength (9, GridUnitType.Pixel) });
					container.RowDefinitions.Add (new RowDefinition () { Height = new GridLength (9, GridUnitType.Pixel) });
					container.ColumnDefinitions.Add (new ColumnDefinition ());
					container.ColumnDefinitions.Add (new ColumnDefinition () { Width = new GridLength (24) });
					var text = new TextBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					text.PreviewTextInput += (sender, e) => { e.Handled = Regex.IsMatch (e.Text, "[^0-9]"); };
					text.TextChanged += (sender, e) => { if (text.Text.Trim () == "") text.Text = "0"; };
					var textBinding = new Binding ()
					{
						Source = command,
						Path = new PropertyPath (prop.Name)
					};
					text.SetBinding (TextBox.TextProperty, textBinding);
					Grid.SetRowSpan (text, 2);
					container.Children.Add (text);

					var upButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Bottom,
						Content = "▲",
						FontSize = 5.5
					};
					upButton.Click += (sender, e) =>
					{
						if (text.Text == "") text.Text = "0";
						var num = uint.Parse (text.Text) + 1;
						if (num == uint.MinValue) num = uint.MaxValue;
						prop.SetValue (command, num);
						text.GetBindingExpression (TextBox.TextProperty)?.UpdateTarget ();
					};
					Grid.SetRow (upButton, 0);
					Grid.SetColumn (upButton, 1);
					container.Children.Add (upButton);

					var downButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Top,
						Content = "▼",
						FontSize = 5.5
					};
					downButton.Click += (sender, e) =>
					{
						if (text.Text == "") text.Text = "0";
						var num = uint.Parse (text.Text) - 1;
						if (num == uint.MaxValue) num = uint.MinValue;
						//text.Text = num.ToString ();
						prop.SetValue (command, num);
						text.GetBindingExpression (TextBox.TextProperty)?.UpdateTarget ();
					};
					Grid.SetRow (downButton, 1);
					Grid.SetColumn (downButton, 1);
					container.Children.Add (downButton);

					control = container;
				}
				if (prop.PropertyType == typeof (uint?))
				{
					(control as Grid).ColumnDefinitions.Insert (0, new ColumnDefinition () { Width = new GridLength (20) });

					var check = new CheckBox
					{
						IsChecked = (prop.GetValue(command) as uint?).HasValue,
						VerticalAlignment = VerticalAlignment.Center
					};
					check.Checked += (sender, e) =>
					{
						if (check.IsChecked != null && check.IsChecked.Value) prop.SetValue (command, (uint?) 0);
						else prop.SetValue (command, null);
					};
					Grid.SetRowSpan (check, 2);
					(control as Grid).Children.Add (check);

					var binding = new Binding ()
					{
						Source = check,
						Path = new PropertyPath ("IsChecked")
					};
					((control as Grid).Children [0] as TextBox).SetBinding (TextBox.IsEnabledProperty, binding);
					((control as Grid).Children [1] as Button).SetBinding (Button.IsEnabledProperty, binding);
					((control as Grid).Children [2] as Button).SetBinding (Button.IsEnabledProperty, binding);
					Grid.SetColumn (((control as Grid).Children [0] as TextBox), 1);
					Grid.SetColumn (((control as Grid).Children [1] as Button), 2);
					Grid.SetColumn (((control as Grid).Children [2] as Button), 2);
				}

				if (prop.PropertyType == typeof (int))
				{
					var container = new Grid ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					container.RowDefinitions.Add (new RowDefinition () { Height = new GridLength (9, GridUnitType.Pixel) });
					container.RowDefinitions.Add (new RowDefinition () { Height = new GridLength (9, GridUnitType.Pixel) });
					container.ColumnDefinitions.Add (new ColumnDefinition ());
					container.ColumnDefinitions.Add (new ColumnDefinition () { Width = new GridLength (24) });
					var text = new TextBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					text.PreviewTextInput += (sender, e) => { e.Handled = !Regex.IsMatch (e.Text, "[\\-0-9][0-9]*"); };
					text.TextChanged += (sender, e) => { if (text.Text.Trim () == "") text.Text = "0"; };
					var textBinding = new Binding ()
					{
						Source = command,
						Path = new PropertyPath (prop.Name)
					};
					text.SetBinding (TextBox.TextProperty, textBinding);
					Grid.SetRowSpan (text, 2);
					container.Children.Add (text);

					var upButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Bottom,
						Content = "▲",
						FontSize = 5.5
					};
					upButton.Click += (sender, e) =>
					{
						if (text.Text == "") text.Text = "0";
						var num = int.Parse (text.Text) + 1;
						if (num == int.MinValue) num = int.MaxValue;
						//text.Text = num.ToString ();
						prop.SetValue (command, num);
						text.GetBindingExpression (TextBox.TextProperty)?.UpdateTarget ();
					};
					Grid.SetRow (upButton, 0);
					Grid.SetColumn (upButton, 1);
					container.Children.Add (upButton);

					var downButton = new Button
					{
						Style = this.Resources["ButtonStyle"] as Style,
						VerticalAlignment = VerticalAlignment.Top,
						Content = "▼",
						FontSize = 5.5
					};
					downButton.Click += (sender, e) =>
					{
						if (text.Text == "") text.Text = "0";
						var num = int.Parse (text.Text) - 1;
						if (num == int.MaxValue) num = int.MinValue;
						prop.SetValue (command, num);
						text.GetBindingExpression (TextBox.TextProperty)?.UpdateTarget ();
					};
					Grid.SetRow (downButton, 1);
					Grid.SetColumn (downButton, 1);
					container.Children.Add (downButton);

					control = container;
				}

				if (prop.PropertyType == typeof (Position1) || prop.PropertyType == typeof (Position2))
				{
					control = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						VerticalAlignment = VerticalAlignment.Center
					};
					var radioPre = new RadioButton ()
					{
						Content = Strings.Instance [Position2.StartPoint.ToString()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioPre.Checked += (sender, e) => { prop.SetValue (command, Position1.StartPoint); };
					radioPre.IsChecked = ((Position1) (int) prop?.GetValue(command)) == Position1.StartPoint;
					(control as StackPanel).Children.Add (radioPre);
					var radioPost = new RadioButton ()
					{
						Content = Strings.Instance [Position2.EndPoint.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioPost.Checked += (sender, e) => { prop.SetValue (command, Position1.EndPoint); };
					radioPost.IsChecked = ((Position1) (int) prop?.GetValue(command)) == Position1.EndPoint;
					(control as StackPanel).Children.Add (radioPost);
				}
				if (prop.PropertyType == typeof (Position2))
				{
					var radioBoth = new RadioButton ()
					{
						Content = Strings.Instance [Position2.BothPoint.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioBoth.Checked += (sender, e) => { prop.SetValue (command, Position2.BothPoint); };
					radioBoth.IsChecked = ((Position2) prop?.GetValue (command)) == Position2.BothPoint;
					(control as StackPanel).Children.Add (radioBoth);
				}

				if (prop.PropertyType == typeof (Casecast2) || prop.PropertyType == typeof (Casecast1))
				{
					control = new StackPanel
					{
						Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center
					};
					var radioLower = new RadioButton ()
					{
						Content = Strings.Instance [Casecast2.LowercaseAll.ToString()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioLower.Checked += (sender, e) => { prop.SetValue (command, Casecast1.LowercaseAll); };
					radioLower.IsChecked = ((Casecast1) prop?.GetValue (command)) == Casecast1.LowercaseAll;
					(control as StackPanel).Children.Add (radioLower);
					var radioUpper = new RadioButton ()
					{
						Content = Strings.Instance [Casecast2.UppercaseAll.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioUpper.Checked += (sender, e) => { prop.SetValue (command, Casecast1.UppercaseAll); };
					radioUpper.IsChecked = ((Casecast1) prop?.GetValue (command)) == Casecast1.UppercaseAll;
					(control as StackPanel).Children.Add (radioUpper);
				}
				if (prop.PropertyType == typeof (Casecast2))
				{
					var radioUpperFirstLetter = new RadioButton ()
					{
						Content = Strings.Instance [Casecast2.UppercaseFirstLetter.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioUpperFirstLetter.Checked += (sender, e) => { prop.SetValue (command, Casecast2.UppercaseFirstLetter); };
					radioUpperFirstLetter.IsChecked = ((Casecast2) prop?.GetValue (command)) == Casecast2.UppercaseFirstLetter;
					(control as StackPanel).Children.Add (radioUpperFirstLetter);
				}

				if (prop.PropertyType == typeof (DirectoryInfo))
				{
					var grid = new Grid ();
					grid.ColumnDefinitions.Add (new ColumnDefinition ());
					grid.ColumnDefinitions.Add (new ColumnDefinition () { Width = new GridLength (32) });
					var textBox = new TextBox ()
					{
						IsReadOnly = true,
						Text = (prop.GetValue (command) as DirectoryInfo).FullName,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness (0, 0, 5, 0)
					};
					grid.Children.Add (textBox);
					var button = new Button ()
					{
						Style = this.Resources ["ButtonStyle"] as Style,
						Content = "...",
						VerticalAlignment = VerticalAlignment.Center
					};
					button.Click += (sender, e) =>
					{
						FileDialog fbd = new OpenFolderDialog ()
						{
							InitialDirectory = textBox.Text
						};
						if (fbd.ShowDialog () == true)
						{
							textBox.Text = fbd.FileName;
							prop.SetValue (command, new DirectoryInfo (textBox.Text));
						}
					};
					Grid.SetColumn (button, 1);
					grid.Children.Add (button);
					control = grid;
				}

				if (prop.PropertyType == typeof (FileDateKind))
				{
					control = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						VerticalAlignment = VerticalAlignment.Center
					};
					var radioCreated = new RadioButton ()
					{
						Content = Strings.Instance [FileDateKind.Creation.ToString()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioCreated.Checked += (sender, e) => { prop.SetValue (command, FileDateKind.Creation); };
					radioCreated.IsChecked = ((FileDateKind) prop?.GetValue (command)) == FileDateKind.Creation;
					(control as StackPanel).Children.Add (radioCreated);
					var radioModified = new RadioButton ()
					{
						Content = Strings.Instance [FileDateKind.LastModify.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioModified.Checked += (sender, e) => { prop.SetValue (command, FileDateKind.LastModify); };
					radioModified.IsChecked = ((FileDateKind) prop?.GetValue (command)) == FileDateKind.LastModify;
					(control as StackPanel).Children.Add (radioModified);
					var radioAccessed = new RadioButton ()
					{
						Content = Strings.Instance [FileDateKind.LastAccess.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioAccessed.Checked += (sender, e) => { prop.SetValue (command, FileDateKind.LastAccess); };
					radioAccessed.IsChecked = ((FileDateKind) prop?.GetValue (command)) == FileDateKind.LastAccess;
					(control as StackPanel).Children.Add (radioAccessed);
					var radioNow = new RadioButton ()
					{
						Content = Strings.Instance [FileDateKind.Now.ToString ()],
						Margin = new Thickness (0, 0, 5, 0),
						GroupName = prop.Name
					};
					radioNow.Checked += (sender, e) => { prop.SetValue (command, FileDateKind.Now); };
					radioNow.IsChecked = ((FileDateKind) prop?.GetValue (command)) == FileDateKind.Now;
					(control as StackPanel).Children.Add (radioNow);
				}

				if (prop.PropertyType == typeof (MediaTag))
				{
					var comboBox = new ComboBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach (var item in new [] {
						"media_tag_audio_bitrate", "media_tag_audio_samplerate", "media_tag_audio_channels", "media_tag_audio_bitspersample",
						"media_tag_audio_codec", "media_tag_audio_album", "media_tag_audio_album_artists", "media_tag_audio_composers",
						"media_tag_audio_performers", "media_tag_audio_copyright", "media_tag_audio_disc", "media_tag_audio_disc_count",
						"media_tag_audio_genres", "media_tag_audio_title", "media_tag_audio_track", "media_tag_audio_track_count",
						"media_tag_audio_conductor", "media_tag_audio_year", "media_tag_audio_duration",

						"media_tag_video_title", "media_tag_video_duration", "media_tag_video_width", "media_tag_video_height",
						"media_tag_video_codec", "media_tag_video_genres", "media_tag_video_year", "media_tag_video_copyright",

						"media_tag_image_width", "media_tag_image_height", "media_tag_image_codec", "media_tag_image_quality",
					})
						comboBox.Items.Add (Strings.Instance [item]);
					comboBox.SelectedIndex = (int) (MediaTag) (prop.GetValue(command) ?? 0);
					comboBox.SelectionChanged += (sender, e) => { prop.SetValue (command, (MediaTag) comboBox.SelectedIndex); };

					control = comboBox;
				}

				if (prop.PropertyType == typeof (DocumentTag))
				{
					var comboBox = new ComboBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach (var item in new [] { "document_tag_title", "document_tag_author" })
						comboBox.Items.Add (Strings.Instance [item]);
					comboBox.SelectedIndex = (int) (DocumentTag) (prop.GetValue(command) ?? 0);
					comboBox.SelectionChanged += (sender, e) => { prop.SetValue (command, (DocumentTag) comboBox.SelectedIndex); };

					control = comboBox;
				}

				if (prop.PropertyType == typeof (HashType))
				{
					var comboBox = new ComboBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach (var item in new [] { "hash_md5", "hash_sha1", "hash_sha256", "hash_sha384", "hash_sha512" })
						comboBox.Items.Add (Strings.Instance [item]);
					comboBox.SelectedIndex = (int) (HashType) (prop.GetValue(command) ?? 0);
					comboBox.SelectionChanged += (sender, e) => { prop.SetValue (command, (HashType) comboBox.SelectedIndex); };

					control = comboBox;
				}

				if (control == null)
					continue;

				Grid.SetRow (control, propDict.IndexOf (propPair));
				Grid.SetColumn (control, 1);

				contentGrid.Children.Add (control);
			}
		}

		private void ButtonOK_Click (object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void ButtonCancel_Click (object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
