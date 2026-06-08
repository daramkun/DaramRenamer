using System.Collections.ObjectModel;
using System.Text;
using DaramRenamer.Commands;
using DaramRenamer.Conditions;
using DaramRenamer.Helpers;

namespace DaramRenamer.Test;

[TestClass]
public class BaseBehaviorTest
{
    [TestMethod]
    public void FileItemSerializerRoundTripsSingleItem()
    {
        var item = new FileItem(@"C:\source\sample.txt", "renamed.md", @"D:\target", false);

        var restored = FileItemSerializer.Deserialize(FileItemSerializer.Serialize(item));

        Assert.AreEqual(item.SourceFullPath, restored.SourceFullPath);
        Assert.AreEqual(item.ChangedName, restored.ChangedName);
        Assert.AreEqual(item.ChangedPath, restored.ChangedPath);
        Assert.AreEqual(item.IsDirectory, restored.IsDirectory);
        Assert.AreEqual(@"D:\target/renamed.md".Replace('/', Path.DirectorySeparatorChar), restored.ChangedFullPath);
    }

    [TestMethod]
    public void FileItemSerializerRoundTripsCollectionAsIndependentCopy()
    {
        var items = new ObservableCollection<FileItem>
        {
            new(@"C:\source\a.txt", "a-renamed.txt", @"C:\out", false),
            new(@"C:\source\b", "b-renamed", @"C:\out", true)
        };

        var restored = FileItemSerializer.DeserializeCollection(FileItemSerializer.SerializeCollection(items));
        items[0].ChangedName = "changed-after-serialize.txt";

        Assert.HasCount(2, restored);
        Assert.AreEqual("a-renamed.txt", restored[0].ChangedName);
        Assert.AreEqual("b-renamed", restored[1].ChangedName);
        Assert.IsTrue(restored[1].IsDirectory);
    }

    [TestMethod]
    public void UndoManagerRestoresUndoAndRedoSnapshots()
    {
        var undoManager = new UndoManager();
        var items = new ObservableCollection<FileItem>
        {
            new(@"C:\source\a.txt", "a.txt", @"C:\source", false)
        };

        undoManager.SaveToUndoStack(items);
        items[0].ChangedName = "a-renamed.txt";

        var undoSnapshot = undoManager.LoadFromUndoStack();
        undoManager.SaveToRedoStack(items);
        items[0].ChangedName = "a-final.txt";
        var redoSnapshot = undoManager.LoadFromRedoStack();

        Assert.AreEqual("a.txt", undoSnapshot[0].ChangedName);
        Assert.AreEqual("a-renamed.txt", redoSnapshot[0].ChangedName);
        Assert.IsTrue(undoManager.IsUndoStackEmpty);
        Assert.IsTrue(undoManager.IsRedoStackEmpty);
    }

    [TestMethod]
    public void BatchNodeSerializesCommandsConditionsAndChildren()
    {
        var root = new RootBatchNode();
        root.Children.Add(new BatchNode
        {
            Condition = new ExtensionCondition { Extension = ".txt" },
            Command = new ConcatCommand { Text = "root-", Position = Position3.Begin },
            Children =
            {
                new BatchNode
                {
                    Command = new ReplacePlainCommand
                    {
                        Find = "root",
                        Replace = "child",
                        IncludeExtension = false
                    }
                }
            }
        });

        var builder = new StringBuilder();
        using (var writer = new StringWriter(builder))
            root.Serialize(writer);

        var restored = new RootBatchNode();
        using (var reader = new StringReader(builder.ToString()))
            restored.Deserializer(reader);

        var matching = new FileItem(@"C:\source\name.txt", "name.txt", @"C:\source", false);
        var skipped = new FileItem(@"C:\source\name.jpg", "name.jpg", @"C:\source", false);
        restored.Execute(matching);
        restored.Execute(skipped);

        Assert.AreEqual("child-name.txt", matching.ChangedName);
        Assert.AreEqual("root-name.jpg", skipped.ChangedName);
    }
}
