using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DaramRenamer
{
	[Serializable]
	public class UndoManager<T> where T : class
	{
		[NonSerialized]
		private BinaryFormatter bf = new BinaryFormatter();
		private Stack<byte[]> undoStack = new Stack<byte[]>();
		private Stack<byte[]> redoStack = new Stack<byte[]>();

		public event EventHandler UpdateUndo, UpdateRedo;

		public bool IsUndoStackEmpty => undoStack.Count == 0;
		public bool IsRedoStackEmpty => redoStack.Count == 0;

		public void SaveToUndoStack(T fileInfoCollection, bool clearRedoStack = true)
		{
			using (var memStream = new MemoryStream())
			{
				bf.Serialize(memStream, fileInfoCollection ?? throw new ArgumentNullException());
				undoStack.Push(memStream.ToArray());
			}

			if (clearRedoStack)
				ClearRedoStack();

			UpdateUndo?.Invoke(this, EventArgs.Empty);
		}

		public void SaveToRedoStack(T fileInfoCollection)
		{
			using (var memStream = new MemoryStream())
			{
				bf.Serialize(memStream, fileInfoCollection ?? throw new ArgumentNullException());
				redoStack.Push(memStream.ToArray());
			}

			UpdateRedo?.Invoke(this, EventArgs.Empty);
		}

		public T LoadFromUndoStack()
		{
			if (IsUndoStackEmpty) return null;
			using var memStream = new MemoryStream(undoStack.Pop());
			var ret = bf.Deserialize(memStream) as T ?? throw new InvalidCastException();
			UpdateUndo?.Invoke(this, EventArgs.Empty);
			return ret;
		}

		public T LoadFromRedoStack()
		{
			if (IsRedoStackEmpty) return null;
			using var memStream = new MemoryStream(redoStack.Pop());
			var ret = bf.Deserialize(memStream) as T ?? throw new InvalidCastException();
			UpdateRedo?.Invoke(this, EventArgs.Empty);
			return ret;
		}

		public void ClearUndoStack()
		{
			undoStack.Clear();
		}

		public void ClearRedoStack()
		{
			redoStack.Clear();
		}

		public void ClearAll()
		{
			ClearUndoStack();
			ClearRedoStack();
		}
	}
}
