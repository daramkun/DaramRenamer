using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DaramRenamer;

[Serializable]
public class UndoManager
{
    private Stack<byte[]> _redoStack = new();
    private Stack<byte[]> _undoStack = new();

    public bool IsUndoStackEmpty => _undoStack.Count == 0;
    public bool IsRedoStackEmpty => _redoStack.Count == 0;

    public event EventHandler UpdateUndo, UpdateRedo;

    public void SaveToUndoStack(ObservableCollection<FileInfo> fileInfoCollection, bool clearRedoStack = true)
    {
        _undoStack.Push(FileInfoSerializer.SerializeCollection(fileInfoCollection));

        if (clearRedoStack)
            ClearRedoStack();

        UpdateUndo?.Invoke(this, EventArgs.Empty);
    }

    public void SaveToRedoStack(ObservableCollection<FileInfo> fileInfoCollection)
    {
        _redoStack.Push(FileInfoSerializer.SerializeCollection(fileInfoCollection));

        UpdateRedo?.Invoke(this, EventArgs.Empty);
    }

    public byte[] SaveTemporary(ObservableCollection<FileInfo> fileInfoCollection)
    {
        return FileInfoSerializer.SerializeCollection(fileInfoCollection);
    }

    public ObservableCollection<FileInfo> LoadFromUndoStack()
    {
        if (IsUndoStackEmpty) return null;
        var ret = FileInfoSerializer.DeserializeCollection(_undoStack.Pop());
        UpdateUndo?.Invoke(this, EventArgs.Empty);
        return ret;
    }

    public ObservableCollection<FileInfo> LoadFromRedoStack()
    {
        if (IsRedoStackEmpty) return null;
        var ret = FileInfoSerializer.DeserializeCollection(_redoStack.Pop());
        UpdateRedo?.Invoke(this, EventArgs.Empty);
        return ret;
    }

    public ObservableCollection<FileInfo> LoadTemporary(byte[] temporary)
    {
        return FileInfoSerializer.DeserializeCollection(temporary);
    }

    public void ClearUndoStack()
    {
        _undoStack.Clear();
    }

    public void ClearRedoStack()
    {
        _redoStack.Clear();
    }

    public void ClearAll()
    {
        ClearUndoStack();
        ClearRedoStack();
    }
}