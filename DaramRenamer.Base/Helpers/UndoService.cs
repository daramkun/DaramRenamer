using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DaramRenamer.Helpers;

public class UndoService : INotifyPropertyChanged
{
    private readonly Stack<byte[]> _redoStack = new();
    private readonly Stack<byte[]> _undoStack = new();

    public bool IsUndoStackEmpty => _undoStack.Count == 0;
    public bool IsRedoStackEmpty => _redoStack.Count == 0;

    public event EventHandler? UndoUpdated, RedoUpdated;

    public void SaveToUndoStack(ObservableCollection<FileItem> collection, bool clearRedoStack = true)
    {
        _undoStack.Push(FileItemSerializer.SerializeCollection(collection));

        if (clearRedoStack)
            ClearRedoStack();

        UndoUpdated?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(IsUndoStackEmpty));
    }

    public void SaveToRedoStack(ObservableCollection<FileItem> collection)
    {
        _redoStack.Push(FileItemSerializer.SerializeCollection(collection));

        RedoUpdated?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(IsRedoStackEmpty));
    }

    public byte[] SaveTemporary(ObservableCollection<FileItem> collection)
    {
        return FileItemSerializer.SerializeCollection(collection);
    }

    public ObservableCollection<FileItem> LoadFromUndoStack()
    {
        if (IsUndoStackEmpty) return [];
        var ret = FileItemSerializer.DeserializeCollection(_undoStack.Pop());
        UndoUpdated?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(IsUndoStackEmpty));
        return ret;
    }

    public ObservableCollection<FileItem> LoadFromRedoStack()
    {
        if (IsRedoStackEmpty) return [];
        var ret = FileItemSerializer.DeserializeCollection(_redoStack.Pop());
        RedoUpdated?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(IsRedoStackEmpty));
        return ret;
    }

    public ObservableCollection<FileItem> LoadTemporary(byte[] temporary)
    {
        return FileItemSerializer.DeserializeCollection(temporary);
    }

    public void ClearUndoStack()
    {
        _undoStack.Clear();
        OnPropertyChanged(nameof(IsUndoStackEmpty));
    }

    public void ClearRedoStack()
    {
        _redoStack.Clear();
        OnPropertyChanged(nameof(IsRedoStackEmpty));
    }

    public void ClearAll()
    {
        ClearUndoStack();
        ClearRedoStack();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}