using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Daramee.DaramCommonLib;
using Daramee.Nargs;
using Daramee.Winston.Dialogs;
using Daramkun.DaramRenamer.Converters;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// SubWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SubWindow : UserControl, ISubWindow
	{
		public event RoutedEventHandler OKButtonClicked;
		public event RoutedEventHandler CancelButtonClicked;

		public IProcessor Processor { get; private set; }

		public SubWindow ( IProcessor processor, bool titleBarVisible = true )
		{
			InitializeComponent ();

			if ( !titleBarVisible )
			{
				titleBar.Visibility = Visibility.Hidden;
				titleBar.Height = 0;
			}

			Processor = processor;

			overlayWindowTitle.Text = StringTable.SharedStrings [ processor.Name ];

			Type type = processor.GetType ();
			var props = type.GetProperties ();
			List<PropertyInfo> propDict = new List<PropertyInfo> ();
			foreach ( var prop in props )
			{
				object [] attrs = prop.GetCustomAttributes ( typeof ( ArgumentAttribute ), true );
				if ( attrs.Length > 0)
					propDict.Add ( prop );
			}

			for ( int i = 0; i < propDict.Count; ++i )
				contentGrid.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 24 ) } );

			FrameworkElement firstControl = null;
			foreach ( var propPair in propDict )
			{
				var prop = propPair;
				object [] attrs = prop.GetCustomAttributes ( typeof ( ArgumentAttribute ), true );
				TextBlock textBlock = new TextBlock () { Text = StringTable.SharedStrings [ ( attrs [ 0 ] as ArgumentAttribute ).Name ] };
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				Grid.SetRow ( textBlock, propDict.IndexOf ( propPair ) );
				Grid.SetColumn ( textBlock, 0 );
				contentGrid.Children.Add ( textBlock );

				FrameworkElement control = null;

				if ( prop.PropertyType == typeof ( string ) || prop.PropertyType == typeof ( Regex ) )
				{
					control = new TextBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					var binding = new Binding ()
					{
						Source = Processor,
						Path = new PropertyPath ( prop.Name ),
						Converter = prop.PropertyType == typeof ( Regex ) ? new RegexConverter () : null,
					};
					control.SetBinding ( TextBox.TextProperty, binding );
				}
				if ( prop.PropertyType == typeof ( bool ) )
				{
					control = new CheckBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					var binding = new Binding ()
					{
						Source = Processor,
						Path = new PropertyPath ( prop.Name )
					};
					control.SetBinding ( CheckBox.IsCheckedProperty, binding );
				}
				if ( prop.PropertyType == typeof ( bool? ) )
				{
					control = new CheckBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					( control as CheckBox ).IsThreeState = true;
					( control as CheckBox ).Checked += ( sender, e ) =>
					{
						switch ( (control as CheckBox).IsChecked)
						{
							case null: ( control as CheckBox ).IsChecked = false; break;
							case true: ( control as CheckBox ).IsChecked = true; break;
							case false: ( control as CheckBox ).IsChecked = null; break;
						}
						e.Handled = true;
					};
				}
				if ( prop.PropertyType == typeof ( uint ) || prop.PropertyType == typeof ( uint? ) )
				{
					var container = new Grid ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 9, GridUnitType.Pixel ) } );
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 9, GridUnitType.Pixel ) } );
					container.ColumnDefinitions.Add ( new ColumnDefinition () );
					container.ColumnDefinitions.Add ( new ColumnDefinition () { Width = new GridLength ( 24 ) } );
					var text = new TextBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					text.PreviewTextInput += ( sender, e ) => { e.Handled = Regex.IsMatch ( e.Text, "[^0-9]" ); };
					text.TextChanged += ( sender, e ) => { if ( text.Text.Trim () == "" ) text.Text = "0"; };
					var textBinding = new Binding ()
					{
						Source = Processor,
						Path = new PropertyPath ( prop.Name )
					};
					text.SetBinding ( TextBox.TextProperty, textBinding );
					Grid.SetRowSpan ( text, 2 );
					container.Children.Add ( text );

					var upButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					upButton.VerticalAlignment = VerticalAlignment.Bottom;
					upButton.Content = "▲";
					upButton.FontSize = 5.5;
					upButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = uint.Parse ( text.Text ) + 1;
						if ( num == uint.MinValue ) num = uint.MaxValue;
						//text.Text = num.ToString ();
						prop.SetValue ( Processor, num );
						text.GetBindingExpression ( TextBox.TextProperty ).UpdateTarget ();
					};
					Grid.SetRow ( upButton, 0 );
					Grid.SetColumn ( upButton, 1 );
					container.Children.Add ( upButton );

					var downButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					downButton.VerticalAlignment = VerticalAlignment.Top;
					downButton.Content = "▼";
					downButton.FontSize = 5.5;
					downButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = uint.Parse ( text.Text ) - 1;
						if ( num == uint.MaxValue ) num = uint.MinValue;
						//text.Text = num.ToString ();
						prop.SetValue ( Processor, num );
						text.GetBindingExpression ( TextBox.TextProperty ).UpdateTarget ();
					};
					Grid.SetRow ( downButton, 1 );
					Grid.SetColumn ( downButton, 1 );
					container.Children.Add ( downButton );

					control = container;
				}
				if ( prop.PropertyType == typeof ( uint? ) )
				{
					( control as Grid ).ColumnDefinitions.Insert ( 0, new ColumnDefinition () { Width = new GridLength ( 20 ) } );

					var check = new CheckBox () { IsChecked = ( prop.GetValue ( Processor ) as uint? ).HasValue };
					check.VerticalAlignment = VerticalAlignment.Center;
					check.Checked += ( sender, e ) =>
					{
						if ( check.IsChecked.Value ) prop.SetValue ( Processor, ( uint? ) 0 );
						else prop.SetValue ( Processor, null );
					};
					Grid.SetRowSpan ( check, 2 );
					( control as Grid ).Children.Add ( check );

					var binding = new Binding ()
					{
						Source = check,
						Path = new PropertyPath ( "IsChecked" )
					};
					( ( control as Grid ).Children [ 0 ] as TextBox ).SetBinding ( TextBox.IsEnabledProperty, binding );
					( ( control as Grid ).Children [ 1 ] as Button ).SetBinding ( Button.IsEnabledProperty, binding );
					( ( control as Grid ).Children [ 2 ] as Button ).SetBinding ( Button.IsEnabledProperty, binding );
					Grid.SetColumn ( ( ( control as Grid ).Children [ 0 ] as TextBox ), 1 );
					Grid.SetColumn ( ( ( control as Grid ).Children [ 1 ] as Button ), 2 );
					Grid.SetColumn ( ( ( control as Grid ).Children [ 2 ] as Button ), 2 );
				}

				if ( prop.PropertyType == typeof ( int ) )
				{
					var container = new Grid ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 9, GridUnitType.Pixel ) } );
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 9, GridUnitType.Pixel ) } );
					container.ColumnDefinitions.Add ( new ColumnDefinition () );
					container.ColumnDefinitions.Add ( new ColumnDefinition () { Width = new GridLength ( 24 ) } );
					var text = new TextBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					text.PreviewTextInput += ( sender, e ) => { e.Handled = !Regex.IsMatch ( e.Text, "[\\-0-9][0-9]*" ); };
					text.TextChanged += ( sender, e ) => { if ( text.Text.Trim () == "" ) text.Text = "0"; };
					var textBinding = new Binding ()
					{
						Source = Processor,
						Path = new PropertyPath ( prop.Name )
					};
					text.SetBinding ( TextBox.TextProperty, textBinding );
					Grid.SetRowSpan ( text, 2 );
					container.Children.Add ( text );

					var upButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					upButton.VerticalAlignment = VerticalAlignment.Bottom;
					upButton.Content = "▲";
					upButton.FontSize = 5.5;
					upButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = int.Parse ( text.Text ) + 1;
						if ( num == int.MinValue ) num = int.MaxValue;
						//text.Text = num.ToString ();
						prop.SetValue ( Processor, num );
						text.GetBindingExpression ( TextBox.TextProperty ).UpdateTarget ();
					};
					Grid.SetRow ( upButton, 0 );
					Grid.SetColumn ( upButton, 1 );
					container.Children.Add ( upButton );

					var downButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					downButton.VerticalAlignment = VerticalAlignment.Top;
					downButton.Content = "▼";
					downButton.FontSize = 5.5;
					downButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = int.Parse ( text.Text ) - 1;
						if ( num == int.MaxValue ) num = int.MinValue;
						//text.Text = num.ToString ();
						prop.SetValue ( Processor, num );
						text.GetBindingExpression ( TextBox.TextProperty ).UpdateTarget ();
					};
					Grid.SetRow ( downButton, 1 );
					Grid.SetColumn ( downButton, 1 );
					container.Children.Add ( downButton );

					control = container;
				}

				if ( prop.PropertyType == typeof ( OnePointPosition ) || prop.PropertyType == typeof ( Position ) )
				{
					control = new StackPanel () { Orientation = Orientation.Horizontal };
					control.VerticalAlignment = VerticalAlignment.Center;
					var radioPre = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "position_front" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioPre.Checked += ( sender, e ) => { prop.SetValue ( Processor, OnePointPosition.StartPoint ); };
					radioPre.IsChecked = ( ( OnePointPosition ) prop.GetValue ( Processor ) ) == OnePointPosition.StartPoint;
					( control as StackPanel ).Children.Add ( radioPre );
					var radioPost = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "position_end" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioPost.Checked += ( sender, e ) => { prop.SetValue ( Processor, OnePointPosition.EndPoint ); };
					radioPost.IsChecked = ( ( OnePointPosition ) prop.GetValue ( Processor ) ) == OnePointPosition.EndPoint;
					( control as StackPanel ).Children.Add ( radioPost );
				}
				if ( prop.PropertyType == typeof ( Position ) )
				{
					var radioBoth = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "position_both" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioBoth.Checked += ( sender, e ) => { prop.SetValue ( Processor, Position.BothPoint ); };
					radioBoth.IsChecked = ( ( Position ) prop.GetValue ( Processor ) ) == Position.BothPoint;
					( control as StackPanel ).Children.Add ( radioBoth );
				}

				if ( prop.PropertyType == typeof ( Casecast ) || prop.PropertyType == typeof ( CasecastBW ) )
				{
					control = new StackPanel () { Orientation = Orientation.Horizontal };
					control.VerticalAlignment = VerticalAlignment.Center;
					var radioLower = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "casecast_lower" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioLower.Checked += ( sender, e ) => { prop.SetValue ( Processor, CasecastBW.AllToLowercase ); };
					radioLower.IsChecked = ( ( CasecastBW ) prop.GetValue ( Processor ) ) == CasecastBW.AllToLowercase;
					( control as StackPanel ).Children.Add ( radioLower );
					var radioUpper = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "casecast_upper" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioUpper.Checked += ( sender, e ) => { prop.SetValue ( Processor, CasecastBW.AllToUppercase ); };
					radioUpper.IsChecked = ( ( CasecastBW ) prop.GetValue ( Processor ) ) == CasecastBW.AllToUppercase;
					( control as StackPanel ).Children.Add ( radioUpper );
				}
				if ( prop.PropertyType == typeof ( Casecast ) )
				{
					var radioUpperFirstLetter = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "casecast_upper_first_letter" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioUpperFirstLetter.Checked += ( sender, e ) => { prop.SetValue ( Processor, Casecast.UppercaseFirstLetter ); };
					radioUpperFirstLetter.IsChecked = ( ( Casecast ) prop.GetValue ( Processor ) ) == Casecast.UppercaseFirstLetter;
					( control as StackPanel ).Children.Add ( radioUpperFirstLetter );
				}

				if ( prop.PropertyType == typeof ( DirectoryInfo ) )
				{
					var grid = new Grid ();
					grid.ColumnDefinitions.Add ( new ColumnDefinition () );
					grid.ColumnDefinitions.Add ( new ColumnDefinition () { Width = new GridLength ( 32 ) } );
					var textBox = new TextBox ()
					{
						IsReadOnly = true,
						Text = ( prop.GetValue ( Processor ) as DirectoryInfo ).FullName,
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness ( 0, 0, 5, 0 )
					};
					grid.Children.Add ( textBox );
					var button = new Button ()
					{
						Style = this.Resources [ "ButtonStyle" ] as Style,
						Content = "...",
						VerticalAlignment = VerticalAlignment.Center
					};
					button.Click += ( sender, e ) =>
					{
						FileDialog fbd = new OpenFolderDialog ()
						{
							InitialDirectory = textBox.Text
						};
						if ( fbd.ShowDialog () == true )
						{
							textBox.Text = fbd.FileName;
							prop.SetValue ( Processor, new DirectoryInfo ( textBox.Text ) );
						}
					};
					Grid.SetColumn ( button, 1 );
					grid.Children.Add ( button );
					control = grid;
				}

				if ( prop.PropertyType == typeof ( DateType ) )
				{
					control = new StackPanel () { Orientation = Orientation.Horizontal };
					control.VerticalAlignment = VerticalAlignment.Center;
					var radioCreated = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "add_date_created" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioCreated.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.CreationDate ); };
					radioCreated.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.CreationDate;
					( control as StackPanel ).Children.Add ( radioCreated );
					var radioModified = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "add_date_modified" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioModified.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.ModifiedDate ); };
					radioModified.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.ModifiedDate;
					( control as StackPanel ).Children.Add ( radioModified );
					var radioAccessed = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "add_date_accessed" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioAccessed.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.AccessedDate ); };
					radioAccessed.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.AccessedDate;
					( control as StackPanel ).Children.Add ( radioAccessed );
					var radioNow = new RadioButton ()
					{
						Content = StringTable.SharedStrings [ "add_date_now" ],
						Margin = new Thickness ( 0, 0, 5, 0 ),
						GroupName = prop.Name
					};
					radioNow.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.Now ); };
					radioNow.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.Now;
					( control as StackPanel ).Children.Add ( radioNow );
				}

				if ( prop.PropertyType == typeof ( MediaTag ) )
				{
					var comboBox = new ComboBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach ( var item in new [] {
						"media_tag_audio_bitrate", "media_tag_audio_samplerate", "media_tag_audio_channels", "media_tag_audio_bitspersample",
						"media_tag_audio_codec", "media_tag_audio_album", "media_tag_audio_album_artists", "media_tag_audio_composers",
						"media_tag_audio_performers", "media_tag_audio_copyright", "media_tag_audio_disc", "media_tag_audio_disc_count",
						"media_tag_audio_genres", "media_tag_audio_title", "media_tag_audio_track", "media_tag_audio_track_count",
						"media_tag_audio_conductor", "media_tag_audio_year", "media_tag_audio_duration",

						"media_tag_video_title", "media_tag_video_duration", "media_tag_video_width", "media_tag_video_height",
						"media_tag_video_codec", "media_tag_video_genres", "media_tag_video_year", "media_tag_video_copyright",

						"media_tag_image_width", "media_tag_image_height", "media_tag_image_codec", "media_tag_image_quality",
					} )
						comboBox.Items.Add ( StringTable.SharedStrings [ item ] );
					comboBox.SelectedIndex = ( int ) ( MediaTag ) prop.GetValue ( Processor );
					comboBox.SelectionChanged += ( sender, e ) => { prop.SetValue ( Processor, ( MediaTag ) comboBox.SelectedIndex ); };

					control = comboBox;
				}

				if ( prop.PropertyType == typeof ( DocumentTag ) )
				{
					var comboBox = new ComboBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach ( var item in new [] { "document_tag_title", "document_tag_author" } )
						comboBox.Items.Add ( StringTable.SharedStrings [ item ] );
					comboBox.SelectedIndex = ( int ) ( DocumentTag ) prop.GetValue ( Processor );
					comboBox.SelectionChanged += ( sender, e ) => { prop.SetValue ( Processor, ( DocumentTag ) comboBox.SelectedIndex ); };

					control = comboBox;
				}

				if ( prop.PropertyType == typeof ( HashType ) )
				{
					var comboBox = new ComboBox ()
					{
						VerticalAlignment = VerticalAlignment.Center
					};
					foreach ( var item in new [] { "hash_md5", "hash_sha1", "hash_sha256", "hash_sha384", "hash_sha512" } )
						comboBox.Items.Add ( StringTable.SharedStrings [ item ] );
					comboBox.SelectedIndex = ( int ) ( HashType ) prop.GetValue ( Processor );
					comboBox.SelectionChanged += ( sender, e ) => { prop.SetValue ( Processor, ( HashType ) comboBox.SelectedIndex ); };

					control = comboBox;
				}

				if ( control != null )
				{
					Grid.SetRow ( control, propDict.IndexOf ( propPair ) );
					Grid.SetColumn ( control, 1 );

					contentGrid.Children.Add ( control );

					if ( firstControl == null )
						firstControl = control;
				}
			}

			Dispatcher.BeginInvoke ( DispatcherPriority.Loaded, new Action ( () =>
			{
				btnOKButton?.Focus ();
				firstControl?.Focus ();
			} ) );
		}

		private void OK_Button ( object sender, RoutedEventArgs e )
		{
			btnOKButton?.Focus ();
			OKButtonClicked?.Invoke ( this, e );
		}

		private void Cancel_Button ( object sender, RoutedEventArgs e )
		{
			btnCancelButton?.Focus ();
			CancelButtonClicked?.Invoke ( this, e );
		}
	}
}
