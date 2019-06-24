using Daramee.Nargs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public static class ProcessorExtensions
	{
		public static Type GetMethodType ( this IProcessor processor )
		{
			return GenerateGenericDelegate ( processor.GetType () );
		}
		public static Type GetMethodType ( this ICondition condition )
		{
			return GenerateGenericDelegate ( condition.GetType () );
		}
		private static Type GenerateGenericDelegate ( Type type )
		{
			List<Type> paramTypes = new List<Type> ( 4 )
			{
				typeof ( FileInfo )
			};
			foreach ( PropertyInfo prop in type.GetProperties () )
			{
				var localized = prop.GetCustomAttribute<ArgumentAttribute> ();
				if ( localized == null ) continue;
				paramTypes.Add ( prop.PropertyType );
			}

			Type funcType;
			switch ( paramTypes.Count )
			{
				case 1: return typeof ( Func<FileInfo, bool> );
				case 2: funcType = typeof ( Func<,,> ); break;
				case 3: funcType = typeof ( Func<,,,> ); break;
				case 4: funcType = typeof ( Func<,,,,> ); break;
				case 5: funcType = typeof ( Func<,,,,,> ); break;
				case 6: funcType = typeof ( Func<,,,,,,> ); break;
				case 7: funcType = typeof ( Func<,,,,,,,> ); break;
				case 8: funcType = typeof ( Func<,,,,,,,,> ); break;
				default: return null;
			}
			paramTypes.Add ( typeof ( bool ) );
			return funcType.MakeGenericType ( paramTypes.ToArray () );
		}

		public static MethodInfo CreateMethod ( this IProcessor processor )
		{
			return GenerateDynamicMethod ( processor.GetType (), processor.Name, "Process" );
		}
		public static MethodInfo CreateMethod ( this ICondition condition )
		{
			return GenerateDynamicMethod ( condition.GetType (), condition.Name, "IsValid" );
		}
		private static MethodInfo GenerateDynamicMethod ( Type type, string methodName, string callMethod )
		{
			List<PropertyInfo> propInfos = new List<PropertyInfo> ();
			List<Type> paramTypes = new List<Type> ( 8 )
			{
				typeof ( FileInfo )
			};
			foreach ( PropertyInfo prop in type.GetProperties () )
			{
				var localized = prop.GetCustomAttribute<ArgumentAttribute> ();
				if ( localized == null ) continue;
				paramTypes.Add ( prop.PropertyType );
				propInfos.Add ( prop );
			}

			DynamicMethod method = new DynamicMethod ( methodName,
				MethodAttributes.Static | MethodAttributes.Public,
				CallingConventions.Standard,
				typeof ( bool ), paramTypes.ToArray (), typeof ( ProcessorExtensions ), false );

			ILGenerator gen = method.GetILGenerator ();

			var processorVar = gen.DeclareLocal ( type );
			gen.Emit ( OpCodes.Newobj, type.GetConstructor ( new Type [] { } ) );
			gen.Emit ( OpCodes.Stloc, processorVar );

			int index = 1;
			foreach ( PropertyInfo propInfo in propInfos )
			{
				gen.Emit ( OpCodes.Ldloc, processorVar );
				gen.Emit ( OpCodes.Ldarg, index++ );
				gen.Emit ( OpCodes.Callvirt, propInfo.GetSetMethod () );
			}

			gen.Emit ( OpCodes.Ldloc, processorVar );
			gen.Emit ( OpCodes.Ldarg, 0 );
			gen.Emit ( OpCodes.Callvirt, type.GetMethod ( callMethod ) );

			gen.Emit ( OpCodes.Ret );

			return method;
		}

		static List<Delegate> delegates = new List<Delegate> ();
		public static IReadOnlyList<Delegate> Delegates
		{
			get
			{
				lock ( delegates )
				{
					if ( /*delegates == null*/delegates.Count == 0 )
					{
						//delegates = new List<Delegate> ();

						Assembly assembly = Assembly.Load ( "DaramRenamer" );
						foreach ( Type type in assembly.GetTypes () )
						{
							if ( type.GetInterface ( typeof ( IProcessor ).FullName ) != null )
							{
								if ( type.IsAbstract )
									continue;
								IProcessor processor = Activator.CreateInstance ( type ) as IProcessor;
								MethodInfo methodInfo = processor.CreateMethod ();
								Type methodType = processor.GetMethodType ();
								delegates.Add ( methodInfo.CreateDelegate ( methodType ) );
							}
							else if ( type.GetInterface ( typeof ( ICondition ).FullName ) != null )
							{
								ICondition condition = Activator.CreateInstance ( type ) as ICondition;
								MethodInfo methodInfo = condition.CreateMethod ();
								Type methodType = condition.GetMethodType ();
								delegates.Add ( methodInfo.CreateDelegate ( methodType ) );
							}
						}
						delegates.Add ( new Func<string> ( get_renamer_version ) );
						delegates.Add ( new Func<string, string> ( get_file_content ) );
					}
					return delegates;
				}
			}
		}

		private static string get_renamer_version ()
		{
			Version version = Assembly.GetExecutingAssembly ().GetName ().Version;
			return $"{version.Major}.{version.Minor}.{version.Build}";
		}
		private static string get_file_content ( string filename )
		{
			try { return File.ReadAllText ( filename, Encoding.UTF8 ); } catch { return null; }
		}
		private static FileInfo create_file_info ( string filename )
		{
			return new FileInfo ( filename );
		}
	}
}
