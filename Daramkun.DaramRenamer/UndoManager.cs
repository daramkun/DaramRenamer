using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public class UndoManager<T> where T : class
	{
		BinaryFormatter bf = new BinaryFormatter ();
		Stack<byte []> undoStack = new Stack<byte []> ();
		Stack<byte []> redoStack = new Stack<byte []> ();

		public bool IsUndoStackEmpty { get { return undoStack.Count == 0; } }
		public bool IsRedoStackEmpty { get { return redoStack.Count == 0; } }

		public void SaveToUndoStack ( T fileInfoCollection )
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection );
				undoStack.Push ( memStream.ToArray () );
			}
			ClearRedoStack ();
		}

		public void SaveToRedoStack ( T fileInfoCollection )
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection );
				redoStack.Push ( memStream.ToArray () );
			}
		}

		public T LoadFromUndoStack ()
		{
			if ( IsUndoStackEmpty ) return null;
			using ( MemoryStream memStream = new MemoryStream ( undoStack.Pop () ) )
				return bf.Deserialize ( memStream ) as T;
		}

		public T LoadFromRedoStack ()
		{
			if ( IsUndoStackEmpty ) return null;
			using ( MemoryStream memStream = new MemoryStream ( redoStack.Pop () ) )
				return bf.Deserialize ( memStream ) as T;
		}

		public void ClearUndoStack () { undoStack.Clear (); }
		public void ClearRedoStack () { redoStack.Clear (); }
		public void ClearAll () { ClearUndoStack ();ClearRedoStack (); }
	}
}
