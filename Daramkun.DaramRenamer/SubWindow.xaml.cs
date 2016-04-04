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

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// SubWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SubWindow : UserControl
	{
		public event RoutedEventHandler OKButtonClicked;
		public event RoutedEventHandler CancelButtonClicked;

		public IProcessor Processor { get; private set; }

		public SubWindow ( IProcessor processor )
		{
			InitializeComponent ();

			Processor = processor;

			overlayWindowTitle.Text = Globalizer.Strings [ processor.Name ];

			Type type = processor.GetType ();
			var props = type.GetProperties ();
			Dictionary<uint, PropertyInfo> propDict = new Dictionary<uint, PropertyInfo> ();
			foreach ( var prop in props )
			{
				object [] attrs = prop.GetCustomAttributes ( typeof ( GlobalizedAttribute ), true );
				if ( attrs.Length > 0)
					propDict.Add ( ( attrs [ 0 ] as GlobalizedAttribute ).Order, prop );
			}

			for ( int i = 0; i < propDict.Count; ++i )
				contentGrid.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 24 ) } );

			FrameworkElement firstControl = null;
			foreach ( var propPair in from p in propDict orderby p.Key select p )
			{
				var prop = propPair.Value;
				object [] attrs = prop.GetCustomAttributes ( typeof ( GlobalizedAttribute ), true );
				TextBlock textBlock = new TextBlock () { Text = Globalizer.Strings [ ( attrs [ 0 ] as GlobalizedAttribute ).Field ] };
				textBlock.VerticalAlignment = VerticalAlignment.Center;
				Grid.SetRow ( textBlock, ( int ) propPair.Key );
				Grid.SetColumn ( textBlock, 0 );
				contentGrid.Children.Add ( textBlock );

				FrameworkElement control = null;

				if ( prop.PropertyType == typeof ( string ) || prop.PropertyType == typeof ( Regex ) )
				{
					control = new TextBox ();
					control.VerticalAlignment = VerticalAlignment.Center;
					var binding = new Binding ();
					binding.Source = Processor;
					binding.Path = new PropertyPath ( prop.Name );
					control.SetBinding ( TextBox.TextProperty, binding );
				}
				if ( prop.PropertyType == typeof ( bool ) )
				{
					control = new CheckBox ();
					control.VerticalAlignment = VerticalAlignment.Center;
					var binding = new Binding ();
					binding.Source = Processor;
					binding.Path = new PropertyPath ( prop.Name );
					control.SetBinding ( CheckBox.IsCheckedProperty, binding );
				}
				if ( prop.PropertyType == typeof ( bool? ) )
				{
					control = new CheckBox ();
					control.VerticalAlignment = VerticalAlignment.Center;
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
					var container = new Grid ();
					container.VerticalAlignment = VerticalAlignment.Center;
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 0.5, GridUnitType.Star ) } );
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 0.5, GridUnitType.Star ) } );
					container.ColumnDefinitions.Add ( new ColumnDefinition () );
					container.ColumnDefinitions.Add ( new ColumnDefinition () { Width = new GridLength ( 24 ) } );
					var text = new TextBox ();
					text.VerticalAlignment = VerticalAlignment.Center;
					text.PreviewTextInput += ( sender, e ) => { e.Handled = Regex.IsMatch ( e.Text, "[^0-9]" ); };
					text.TextChanged += ( sender, e ) => { if ( text.Text.Trim () == "" ) text.Text = "0"; };
					var textBinding = new Binding ();
					textBinding.Source = Processor;
					textBinding.Path = new PropertyPath ( prop.Name );
					text.SetBinding ( TextBox.TextProperty, textBinding );
					Grid.SetRowSpan ( text, 2 );
					container.Children.Add ( text );

					var upButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					upButton.VerticalAlignment = VerticalAlignment.Bottom;
					upButton.Content = "▲";
					upButton.FontSize = 7;
					upButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = uint.Parse ( text.Text ) + 1;
						if ( num == uint.MinValue ) num = uint.MaxValue;
						text.Text = num.ToString ();
					};
					Grid.SetRow ( upButton, 0 );
					Grid.SetColumn ( upButton, 1 );
					container.Children.Add ( upButton );

					var downButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					downButton.VerticalAlignment = VerticalAlignment.Top;
					downButton.Content = "▼";
					downButton.FontSize = 7;
					downButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = uint.Parse ( text.Text ) - 1;
						if ( num == uint.MaxValue ) num = uint.MinValue;
						text.Text = num.ToString ();
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

					var binding = new Binding ();
					binding.Source = check;
					binding.Path = new PropertyPath ( "IsChecked" );
					( ( control as Grid ).Children [ 0 ] as TextBox ).SetBinding ( TextBox.IsEnabledProperty, binding );
					( ( control as Grid ).Children [ 1 ] as Button ).SetBinding ( Button.IsEnabledProperty, binding );
					( ( control as Grid ).Children [ 2 ] as Button ).SetBinding ( Button.IsEnabledProperty, binding );
					Grid.SetColumn ( ( ( control as Grid ).Children [ 0 ] as TextBox ), 1 );
					Grid.SetColumn ( ( ( control as Grid ).Children [ 1 ] as Button ), 2 );
					Grid.SetColumn ( ( ( control as Grid ).Children [ 2 ] as Button ), 2 );
				}

				if ( prop.PropertyType == typeof ( int ) )
				{
					var container = new Grid ();
					container.VerticalAlignment = VerticalAlignment.Center;
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 0.5, GridUnitType.Star ) } );
					container.RowDefinitions.Add ( new RowDefinition () { Height = new GridLength ( 0.5, GridUnitType.Star ) } );
					container.ColumnDefinitions.Add ( new ColumnDefinition () );
					container.ColumnDefinitions.Add ( new ColumnDefinition () { Width = new GridLength ( 24 ) } );
					var text = new TextBox ();
					text.VerticalAlignment = VerticalAlignment.Center;
					text.PreviewTextInput += ( sender, e ) => { e.Handled = !Regex.IsMatch ( e.Text, "[\\-0-9][0-9]*" ); };
					text.TextChanged += ( sender, e ) => { if ( text.Text.Trim () == "" ) text.Text = "0"; };
					var textBinding = new Binding ();
					textBinding.Source = Processor;
					textBinding.Path = new PropertyPath ( prop.Name );
					text.SetBinding ( TextBox.TextProperty, textBinding );
					Grid.SetRowSpan ( text, 2 );
					container.Children.Add ( text );

					var upButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					upButton.VerticalAlignment = VerticalAlignment.Bottom;
					upButton.Content = "▲";
					upButton.FontSize = 7;
					upButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = int.Parse ( text.Text ) + 1;
						if ( num == int.MinValue ) num = int.MaxValue;
						text.Text = num.ToString ();
					};
					Grid.SetRow ( upButton, 0 );
					Grid.SetColumn ( upButton, 1 );
					container.Children.Add ( upButton );

					var downButton = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					downButton.VerticalAlignment = VerticalAlignment.Top;
					downButton.Content = "▼";
					downButton.FontSize = 7;
					downButton.Click += ( sender, e ) =>
					{
						if ( text.Text == "" ) text.Text = "0";
						var num = int.Parse ( text.Text ) - 1;
						if ( num == int.MaxValue ) num = int.MinValue;
						text.Text = num.ToString ();
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
					var radioPre = new RadioButton ();
					radioPre.Content = Globalizer.Strings [ "position_front" ];
					radioPre.Margin = new Thickness ( 0, 0, 5, 0 );
					radioPre.GroupName = prop.Name;
					radioPre.Checked += ( sender, e ) => { prop.SetValue ( Processor, OnePointPosition.StartPoint ); };
					radioPre.IsChecked = ( ( OnePointPosition ) prop.GetValue ( Processor ) ) == OnePointPosition.StartPoint;
					( control as StackPanel ).Children.Add ( radioPre );
					var radioPost = new RadioButton ();
					radioPost.Content = Globalizer.Strings [ "position_end" ];
					radioPost.Margin = new Thickness ( 0, 0, 5, 0 );
					radioPost.GroupName = prop.Name;
					radioPost.Checked += ( sender, e ) => { prop.SetValue ( Processor, OnePointPosition.EndPoint ); };
					radioPost.IsChecked = ( ( OnePointPosition ) prop.GetValue ( Processor ) ) == OnePointPosition.EndPoint;
					( control as StackPanel ).Children.Add ( radioPost );
				}
				if ( prop.PropertyType == typeof ( Position ) )
				{
					var radioBoth = new RadioButton ();
					radioBoth.Content = Globalizer.Strings [ "position_both" ];
					radioBoth.Margin = new Thickness ( 0, 0, 5, 0 );
					radioBoth.GroupName = prop.Name;
					radioBoth.Checked += ( sender, e ) => { prop.SetValue ( Processor, Position.BothPoint ); };
					radioBoth.IsChecked = ( ( Position ) prop.GetValue ( Processor ) ) == Position.BothPoint;
					( control as StackPanel ).Children.Add ( radioBoth );
				}

				if ( prop.PropertyType == typeof ( Casecast ) || prop.PropertyType == typeof ( CasecastBW ) )
				{
					control = new StackPanel () { Orientation = Orientation.Horizontal };
					control.VerticalAlignment = VerticalAlignment.Center;
					var radioLower = new RadioButton ();
					radioLower.Content = Globalizer.Strings [ "casecast_lower" ];
					radioLower.Margin = new Thickness ( 0, 0, 5, 0 );
					radioLower.GroupName = prop.Name;
					radioLower.Checked += ( sender, e ) => { prop.SetValue ( Processor, CasecastBW.AllToLowercase ); };
					radioLower.IsChecked = ( ( CasecastBW ) prop.GetValue ( Processor ) ) == CasecastBW.AllToLowercase;
					( control as StackPanel ).Children.Add ( radioLower );
					var radioUpper = new RadioButton ();
					radioUpper.Content = Globalizer.Strings [ "casecast_upper" ];
					radioUpper.Margin = new Thickness ( 0, 0, 5, 0 );
					radioUpper.GroupName = prop.Name;
					radioUpper.Checked += ( sender, e ) => { prop.SetValue ( Processor, CasecastBW.AllToUppercase ); };
					radioUpper.IsChecked = ( ( CasecastBW ) prop.GetValue ( Processor ) ) == CasecastBW.AllToUppercase;
					( control as StackPanel ).Children.Add ( radioUpper );
				}
				if ( prop.PropertyType == typeof ( Casecast ) )
				{
					var radioUpperFirstLetter = new RadioButton ();
					radioUpperFirstLetter.Content = Globalizer.Strings [ "casecast_upper_first_letter" ];
					radioUpperFirstLetter.Margin = new Thickness ( 0, 0, 5, 0 );
					radioUpperFirstLetter.GroupName = prop.Name;
					radioUpperFirstLetter.Checked += ( sender, e ) => { prop.SetValue ( Processor, Casecast.UppercaseFirstLetter ); };
					radioUpperFirstLetter.IsChecked = ( ( Casecast ) prop.GetValue ( Processor ) ) == Casecast.UppercaseFirstLetter;
					( control as StackPanel ).Children.Add ( radioUpperFirstLetter );
				}

				if ( prop.PropertyType == typeof ( DirectoryInfo ) )
				{
					var grid = new Grid ();
					grid.ColumnDefinitions.Add ( new ColumnDefinition () );
					grid.ColumnDefinitions.Add ( new ColumnDefinition () { Width = new GridLength ( 32 ) } );
					var textBox = new TextBox ();
					textBox.IsReadOnly = true;
					textBox.Text = ( prop.GetValue ( Processor ) as DirectoryInfo ).FullName;
					textBox.VerticalAlignment = VerticalAlignment.Center;
					textBox.Margin = new Thickness ( 0, 0, 5, 0 );
					grid.Children.Add ( textBox );
					var button = new Button () { Style = this.Resources [ "ButtonStyle" ] as Style };
					button.Content = "...";
					button.VerticalAlignment = VerticalAlignment.Center;
					button.Click += ( sender, e ) =>
					{
						WPFFolderBrowser.WPFFolderBrowserDialog fbd = new WPFFolderBrowser.WPFFolderBrowserDialog ();
						fbd.InitialDirectory = textBox.Text;
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
					var radioCreated = new RadioButton ();
					radioCreated.Content = Globalizer.Strings [ "add_date_created" ];
					radioCreated.Margin = new Thickness ( 0, 0, 5, 0 );
					radioCreated.GroupName = prop.Name;
					radioCreated.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.CreationDate ); };
					radioCreated.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.CreationDate;
					( control as StackPanel ).Children.Add ( radioCreated );
					var radioModified = new RadioButton ();
					radioModified.Content = Globalizer.Strings [ "add_date_modified" ];
					radioModified.Margin = new Thickness ( 0, 0, 5, 0 );
					radioModified.GroupName = prop.Name;
					radioModified.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.ModifiedDate ); };
					radioModified.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.ModifiedDate;
					( control as StackPanel ).Children.Add ( radioModified );
					var radioAccessed = new RadioButton ();
					radioAccessed.Content = Globalizer.Strings [ "add_date_accessed" ];
					radioAccessed.Margin = new Thickness ( 0, 0, 5, 0 );
					radioAccessed.GroupName = prop.Name;
					radioAccessed.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.AccessedDate ); };
					radioAccessed.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.AccessedDate;
					( control as StackPanel ).Children.Add ( radioAccessed );
					var radioNow = new RadioButton ();
					radioNow.Content = Globalizer.Strings [ "add_date_now" ];
					radioNow.Margin = new Thickness ( 0, 0, 5, 0 );
					radioNow.GroupName = prop.Name;
					radioNow.Checked += ( sender, e ) => { prop.SetValue ( Processor, DateType.Now ); };
					radioNow.IsChecked = ( ( DateType ) prop.GetValue ( Processor ) ) == DateType.Now;
					( control as StackPanel ).Children.Add ( radioNow );
				}

				if ( prop.PropertyType == typeof ( MediaTag ) )
				{
					var comboBox = new ComboBox ();
					comboBox.VerticalAlignment = VerticalAlignment.Center;
					foreach ( var item in new [] { "media_tag_audio_album", "media_tag_audio_album_artists", "media_tag_audio_composers",
						"media_tag_audio_copyright", "media_tag_audio_disc", "media_tag_audio_disc_count", "media_tag_audio_genres",
						"media_tag_audio_performers", "media_tag_audio_title", "media_tag_audio_track", "media_tag_audio_track_count",
						"media_tag_audio_year", "media_tag_audio_duration", "media_tag_audio_codec", "media_tag_audio_samplerate",
						"media_tag_audio_bitrate", "media_tag_audio_bitspersample", "media_tag_audio_channels", "media_tag_image_width",
						"media_tag_image_height", "media_tag_image_quality", "media_tag_image_codec", "media_tag_video_genres",
						"media_tag_video_title", "media_tag_video_year", "media_tag_video_duration", "media_tag_video_width",
						"media_tag_video_height", "media_tag_video_codec" } )
						comboBox.Items.Add ( Globalizer.Strings [ item ] );
					comboBox.SelectedIndex = ( int ) ( MediaTag ) prop.GetValue ( Processor );
					comboBox.SelectionChanged += ( sender, e ) => { prop.SetValue ( Processor, ( MediaTag ) comboBox.SelectedIndex ); };

					control = comboBox;
				}

				if ( prop.PropertyType == typeof ( DocumentTag ) )
				{
					var comboBox = new ComboBox ();
					comboBox.VerticalAlignment = VerticalAlignment.Center;
					foreach ( var item in new [] { "document_tag_title", "document_tag_author" } )
						comboBox.Items.Add ( Globalizer.Strings [ item ] );
					comboBox.SelectedIndex = ( int ) ( DocumentTag ) prop.GetValue ( Processor );
					comboBox.SelectionChanged += ( sender, e ) => { prop.SetValue ( Processor, ( DocumentTag ) comboBox.SelectedIndex ); };

					control = comboBox;
				}

				if ( prop.PropertyType == typeof ( HashType ) )
				{
					var comboBox = new ComboBox ();
					comboBox.VerticalAlignment = VerticalAlignment.Center;
					foreach ( var item in new [] { "hash_md5", "hash_sha1", "hash_sha256", "hash_sha384", "hash_sha512" } )
						comboBox.Items.Add ( Globalizer.Strings [ item ] );
					comboBox.SelectedIndex = ( int ) ( HashType ) prop.GetValue ( Processor );
					comboBox.SelectionChanged += ( sender, e ) => { prop.SetValue ( Processor, ( HashType ) comboBox.SelectedIndex ); };

					control = comboBox;
				}

				if ( control != null )
				{
					Grid.SetRow ( control, ( int ) propPair.Key );
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
