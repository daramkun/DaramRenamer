using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

				if ( control != null )
				{
					Grid.SetRow ( control, ( int ) propPair.Key );
					Grid.SetColumn ( control, 1 );

					contentGrid.Children.Add ( control );
				}
			}
		}

		private void OK_Button ( object sender, RoutedEventArgs e )
		{
			if ( OKButtonClicked != null ) OKButtonClicked ( this, e );
		}

		private void Cancel_Button ( object sender, RoutedEventArgs e )
		{
			if ( CancelButtonClicked != null ) CancelButtonClicked ( this, e );
		}
	}
}
