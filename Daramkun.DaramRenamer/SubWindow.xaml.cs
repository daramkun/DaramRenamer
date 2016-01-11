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

				Control control = null;

				if ( prop.PropertyType == typeof ( string ) )
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
