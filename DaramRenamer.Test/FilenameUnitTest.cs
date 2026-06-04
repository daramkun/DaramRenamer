using System.Diagnostics;
using DaramRenamer.Commands;

namespace DaramRenamer.Test;

[TestClass]
public class FilenameUnitTest
{
    [TestMethod]
    public void ReplacePlainCommandTest()
    {
        var testFile1 = TestUtil.MakeFileInfo(@"C:\PathA.txt");
        
        var command1 = new ReplacePlainCommand
        {
            Find = "a",
            Replace = "b",
            IncludeExtension = false
        };
        command1.DoCommand(testFile1);
        
        Assert.AreEqual("PbthA.txt", testFile1.ChangedFilename);

        testFile1.Reset();
        
        var command2 = new ReplacePlainCommand
        {
            Find = "t",
            Replace = "r",
            IncludeExtension = true
        };
        command2.DoCommand(testFile1);
        
        Assert.AreEqual("ParhA.rxr", testFile1.ChangedFilename);
    }

    [TestMethod]
    public void SubstringCommand()
    {
        var testFile1 = TestUtil.MakeFileInfo(@"C:\Path.txt");
        var testFile2 = TestUtil.MakeFileInfo(@"C:\Finder.txt");

        var command1 = new SubstringCommand
        {
            StartIndex = 2,
            Length = 3,
            IncludeExtension = false
        };

        command1.DoCommand(testFile1, testFile2);
        
        Debug.Write(testFile1.ChangedFilename);
        Assert.AreEqual("th.txt", testFile1.ChangedFilename);
        
        Debug.Write(testFile2.ChangedFilename);
        Assert.AreEqual("nde.txt", testFile2.ChangedFilename);
    }
}
