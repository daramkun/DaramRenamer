using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.Processors
{
	public class BatchProcessor : IProcessor
	{
		public string Name { get { return "process_batch_process"; } }
		public bool CannotMultithreadProcess { get { return false; } }

		public ObservableCollection<IProcessor> Processors { get; } = new ObservableCollection<IProcessor> ();

		enum ScriptState
		{
			Blank,
			Comment,
			Checker,
			Statement,
			EndScript,
			Call,
			String,
			Number,
			Boolean,
			Callee,
			Argument,

		}

		public BatchProcessor () { }
		public BatchProcessor ( string batchScript )
		{
			ScriptState scriptState = ScriptState.Blank;
			int pc = 0;
			while ( pc != batchScript.Length )
			{
				switch ( scriptState )
				{
					case ScriptState.Blank:
						if ( batchScript [ pc ] == ' ' || batchScript [ pc ] == '\t' ||
							batchScript [ pc ] == '\n' || batchScript [ pc ] == '\r' ||
							batchScript [ pc ] == '　' )
							scriptState = ScriptState.Blank;
						else if ( batchScript [ pc ] == '#' )
							scriptState = ScriptState.Comment;
						else if ( batchScript [ pc ] == 'c' || batchScript [ pc ] == 's' || batchScript [ pc ] == 'e' )
							scriptState = ScriptState.Checker;
						break;
					case ScriptState.Comment:
						if ( batchScript [ pc ] == '\n' )
							scriptState = ScriptState.Blank;
						break;
					case ScriptState.Checker:
						switch ( batchScript [ pc - 1 ])
						{
							case 'c':
								if ( batchScript [ pc ] == 'a' && batchScript [ pc + 1 ] == 'l' && batchScript [ pc + 2 ] == 'l' )
									scriptState = ScriptState.Call;
								break;
							case 's':
								if ( batchScript [ pc ] == 't' && batchScript [ pc + 1 ] == 'm' && batchScript [ pc + 2 ] == 't' )
									scriptState = ScriptState.Statement;
								break;
							case 'e':
								if ( batchScript [ pc ] == 'n' && batchScript [ pc + 1 ] == 'd' && batchScript [ pc + 2 ] == 's' )
									scriptState = ScriptState.EndScript;
								break;
						}
						break;
					case ScriptState.Call:

						break;
					case ScriptState.Statement:

						break;
				}
				++pc;
			}
		}

		public bool Process ( FileInfo file )
		{
			foreach ( var p in Processors )
				if ( !p.Process ( file ) )
					return false;
			return true;
		}
	}
}
