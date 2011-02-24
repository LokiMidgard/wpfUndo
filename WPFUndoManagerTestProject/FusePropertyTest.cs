using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WPFUndoManagerTestProject
{
    /// <summary>
    /// Summary description for FusePropertyTest
    /// </summary>
    [TestClass]
    public class FusePropertyTest
    {
        public FusePropertyTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        /// <summary>
        /// Use TestInitialize to run code before running each test 
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            ViewModel = new FusePropertyTestViewModel();
        }

        FusePropertyTestViewModel ViewModel { get; set; }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            var originalFuse = ViewModel.FuseInt;
            ViewModel.FuseInt++;
            ViewModel.FuseInt++;
            Assert.AreEqual(originalFuse + 2, ViewModel.FuseInt);
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(originalFuse, ViewModel.FuseInt);




        }
    }
}
