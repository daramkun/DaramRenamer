using System.Diagnostics;
using DaramRenamer.Commands.Filename;

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
        
        Assert.IsTrue(testFile1.ChangedFilename == "PbthA.txt");

        testFile1.Reset();
        
        var command2 = new ReplacePlainCommand
        {
            Find = "t",
            Replace = "r",
            IncludeExtension = true
        };
        command2.DoCommand(testFile1);
        
        Assert.IsTrue(testFile1.ChangedFilename == "ParhA.rxr");
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
        Assert.IsTrue(testFile1.ChangedFilename == "th.txt");
        
        Debug.Write(testFile2.ChangedFilename);
        Assert.IsTrue(testFile2.ChangedFilename == "nde.txt");
    }
}