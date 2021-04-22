using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DaramRenamer
{
	[Serializable]
	public class UndoManager
	{
		private Stack<byte[]> undoStack = new();
		private Stack<byte[]> redoStack = new();

		public event EventHandler UpdateUndo, UpdateRedo;

		public bool IsUndoStackEmpty => undoStack.Count == 0;
		public bool IsRedoStackEmpty => redoStack.Count == 0;

		public void SaveToUndoStack(ObservableCollection<FileInfo> fileInfoCollection, bool clearRedoStack = true)
		{
			undoStack.Push(FileInfoSerializer.SerializeCollection(fileInfoCollection));

			if (clearRedoStack)
				ClearRedoStack();

			UpdateUndo?.Invoke(this, EventArgs.Empty);
		}

		public void SaveToRedoStack(ObservableCollection<FileInfo> fileInfoCollection)
		{
			redoStack.Push(FileInfoSerializer.SerializeCollection(fileInfoCollection));

			UpdateRedo?.Invoke(this, EventArgs.Empty);
		}

		public byte[] SaveTemporary(ObservableCollection<FileInfo> fileInfoCollection)
		{
			return FileInfoSerializer.SerializeCollection(fileInfoCollection);
		}

		public ObservableCollection<FileInfo> LoadFromUndoStack()
		{
			if (IsUndoStackEmpty) return null;
			var ret = FileInfoSerializer.DeserializeCollection(undoStack.Pop());
			UpdateUndo?.Invoke(this, EventArgs.Empty);
			return ret;
		}

		public ObservableCollection<FileInfo> LoadFromRedoStack()
		{
			if (IsRedoStackEmpty) return null;
			var ret = FileInfoSerializer.DeserializeCollection(redoStack.Pop());
			UpdateRedo?.Invoke(this, EventArgs.Empty);
			return ret;
		}

		public ObservableCollection<FileInfo> LoadTemporary(byte[] temporary)
		{
			return FileInfoSerializer.DeserializeCollection(temporary);
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
