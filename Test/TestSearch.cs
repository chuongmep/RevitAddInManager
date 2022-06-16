using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RevitAddinManager.Model;

namespace Test;

[TestClass]
public class TestSearch
{
    string str = "core.plugin.revit.aboutcommand.cmdcheckversion.core.plugin.revit";
    string str2 = "xx.acbxdsd.lgf;glf.gdhfghjfgjgj.dfgfhjuyklkijfdsfs.core.plugin.revit";
    string pattern = "core";
    [TestMethod]
    public void TestKMPAlgorithmShort()
    {
        bool flag1 = KMPAlgorithm.KmpSearch(pattern, str);
        bool flag2 = KMPAlgorithm.KmpSearch(pattern, str);
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
    }

    [TestMethod]
    public void TestContainsShort()
    {
        bool flag1 = str.Contains(pattern);
        bool flag2 = str2.Contains(pattern);
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
    }

    [TestMethod]
    public void TestIndexOfShort()
    {
        bool flag1 = str.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        bool flag2 = str2.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
    }
}