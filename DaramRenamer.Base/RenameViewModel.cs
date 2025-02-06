using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DaramRenamer;

public class RenameViewModel : INotifyPropertyChanged
{
    private readonly ObservableCollection<BaseFileInfo> _files = [];

    private readonly Stack<BaseFileInfo[]> _undoStack = [];
    private readonly Stack<BaseFileInfo[]> _redoStack = [];

    private BaseCondition? _condition;

    public IReadOnlyList<BaseFileInfo> Files => _files;
    public bool IsUndoable => _undoStack.Count > 0;
    public bool IsRedoable => _redoStack.Count > 0;

    public RenameViewModel()
    {
        
    }

    public void ClearFiles()
    {
        StoreUndo();
        _files.Clear();
    }

    public void AddFiles(IEnumerable<BaseFileInfo> files)
    {
        StoreUndo();

        foreach (var file in files)
            _files.Add(file);
    }

    public void RemoveFiles(IEnumerable<BaseFileInfo> files)
    {
        StoreUndo();

        foreach (var file in files)
            _files.Remove(file);
    }

    public void ApplyCondition(BaseCondition condition)
    {
        _condition = condition;
    }

    public void ApplyCommand(BaseCommand command)
    {
        StoreUndo();

        foreach (var file in _files)
        {
            if (_condition != null && !_condition.IsSatisfied(file))
                continue;
            
            command.DoCommand(file);
        }
    }

    public void Undo()
    {
        if (!IsUndoable)
            return;
        
        StoreRedo();
        RestoreUndo();
    }

    public void Redo()
    {
        if (!IsRedoable)
            return;

        StoreUndo();
        RestoreRedo();
    }

    private void StoreUndo()
    {
        var recent = _files.Select(f => (BaseFileInfo)f.Clone()).ToArray();
        _undoStack.Push(recent);
        _redoStack.Clear();
        
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    private void RestoreUndo()
    {
        if (!_undoStack.TryPop(out var recent))
            return;

        _files.Clear();
        foreach (var r in recent)
            _files.Add(r);
        
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    private void StoreRedo()
    {
        var recent = _files.Select(f => (BaseFileInfo)f.Clone()).ToArray();
        _redoStack.Push(recent);
        
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    private void RestoreRedo()
    {
        if (!_redoStack.TryPop(out var recent))
            return;

        _files.Clear();
        foreach (var r in recent)
            _files.Add(r);
        
        OnPropertyChanged(nameof(IsUndoable));
        OnPropertyChanged(nameof(IsRedoable));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}