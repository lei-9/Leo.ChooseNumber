using System;
using System.Collections.Generic;
using System.Text;
using IOS.ConsoleApp.Core.CUCC;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Leo.ChooseNumber.Test
{
    [TestClass]
    public class CUCCUnitTest
    {

        [TestMethod]
        public void TestInitData()
        {
            CUCC_Search.Init_Data();
        }
    }
}
