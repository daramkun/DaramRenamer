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
				else if ( prop.PropertyType == typeof ( bool ) )
				{
					control = new CheckBox ();
					control.VerticalAlignment = VerticalAlignment.Center;
					var binding = new Binding ();
					binding.Source = Processor;
					binding.Path = new PropertyPath ( prop.Name );
					control.SetBinding ( CheckBox.IsCheckedProperty, binding );
				}
				else if ( prop.PropertyType == typeof ( Position ) )
				{
					control = new StackPanel () { Orientation = Orientation.Horizontal };
					control.VerticalAlignment = VerticalAlignment.Center;
					var radioPre = new RadioButton ();
					radioPre.Content = Globalizer.Strings [ "position_front" ];
					radioPre.Margin = new Thickness ( 0, 0, 5, 0 );
					radioPre.GroupName = prop.Name;
					radioPre.Checked += ( sender, e ) => { prop.SetValue ( Processor, Position.StartPoint ); };
					radioPre.IsChecked = ( ( Position ) prop.GetValue ( Processor ) ) == Position.StartPoint;
					( control as StackPanel ).Children.Add ( radioPre );
					var radioPost = new RadioButton ();
					radioPost.Content = Globalizer.Strings [ "position_end" ];
					radioPost.Margin = new Thickness ( 0, 0, 5, 0 );
					radioPost.GroupName = prop.Name;
					radioPost.Checked += ( sender, e ) => { prop.SetValue ( Processor, Position.EndPoint ); };
					radioPost.IsChecked = ( ( Position ) prop.GetValue ( Processor ) ) == Position.EndPoint;
					( control as StackPanel ).Children.Add ( radioPost );
					var radioBoth = new RadioButton ();
					radioBoth.Content = Globalizer.Strings [ "position_both" ];
					radioBoth.Margin = new Thickness ( 0, 0, 5, 0 );
					radioBoth.GroupName = prop.Name;
					radioBoth.Checked += ( sender, e ) => { prop.SetValue ( Processor, Position.BothPoint ); };
					radioBoth.IsChecked = ( ( Position ) prop.GetValue ( Processor ) ) == Position.BothPoint;
					( control as StackPanel ).Children.Add ( radioBoth );
				}
				else if ( prop.PropertyType == typeof ( OnePointPosition ) )
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
				else if ( prop.PropertyType == typeof ( Casecast ) )
				{
					control = new StackPanel () { Orientation = Orientation.Horizontal };
					control.VerticalAlignment = VerticalAlignment.Center;
					var radioLower = new RadioButton ();
					radioLower.Content = Globalizer.Strings [ "casecast_lower" ];
					radioLower.Margin = new Thickness ( 0, 0, 5, 0 );
					radioLower.GroupName = prop.Name;
					radioLower.Checked += ( sender, e ) => { prop.SetValue ( Processor, Casecast.AllToLowercase ); };
					radioLower.IsChecked = ( ( Casecast ) prop.GetValue ( Processor ) ) == Casecast.AllToLowercase;
					( control as StackPanel ).Children.Add ( radioLower );
					var radioUpper = new RadioButton ();
					radioUpper.Content = Globalizer.Strings [ "casecast_upper" ];
					radioUpper.Margin = new Thickness ( 0, 0, 5, 0 );
					radioUpper.GroupName = prop.Name;
					radioUpper.Checked += ( sender, e ) => { prop.SetValue ( Processor, Casecast.AllToUppercase ); };
					radioUpper.IsChecked = ( ( Casecast ) prop.GetValue ( Processor ) ) == Casecast.AllToUppercase;
					( control as StackPanel ).Children.Add ( radioUpper );
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
