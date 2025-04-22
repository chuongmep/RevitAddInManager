using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Algorithm;

namespace Test;

[TestClass]
public class TestSearch
{
    string str = "core.plugin.revit.aboutcommand.cmdcheckversion.core.plugin.revit";
    string str2 = "xx.acbxdsd.lgf;glf.gdhfghjfgjgj.dfgfhjuyklkijfdsfs.core.plugin.revit";
    string pattern = "core.plugin.revit";

    [TestMethod]
    public void TestKmpAlgorithm()
    {
        bool flag1 = KMPAlgorithm.KmpSearch(pattern, str);
        bool flag2 = KMPAlgorithm.KmpSearch(pattern, str);
        bool flag3 = KMPAlgorithm.KmpSearch(str2, str);
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
        Assert.IsFalse(flag3);
    }

    [TestMethod]
    public void TestContains()
    {
        bool flag1 = str.Contains(pattern);
        bool flag2 = str2.Contains(pattern);
        bool flag3 = str2.Contains(str);
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
        Assert.IsFalse(flag3);
    }

    [TestMethod]
    public void TestIndexOf()
    {
        bool flag1 = str.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        bool flag2 = str2.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        bool flag3 = str.IndexOf(str2, StringComparison.OrdinalIgnoreCase) >= 0;
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
        Assert.IsFalse(flag3);
    }

    [TestMethod]
    public void TestZAlgorithmShort()
    {
        bool flag1 = ZAlgorithm.Search(str, pattern);
        bool flag2 = ZAlgorithm.Search(str2, pattern);
        bool flag3 = ZAlgorithm.Search(str, str2);
        Assert.IsTrue(flag1);
        Assert.IsTrue(flag2);
        Assert.IsFalse(flag3);
    }

   
}