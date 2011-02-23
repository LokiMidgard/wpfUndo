using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WPFUndoManagerTestProject
{
    /// <summary>
    /// Summary description for BasicUndoTest
    /// </summary>
    [TestClass]
    public class BasicUndoTest
    {
        public BasicUndoTest()
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
            ViewModel = new BasicTestViewModel();
        }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        public BasicTestViewModel ViewModel { get; set; }
        #endregion

        [TestMethod]
        public void UndoListIsDefaultEmpty()
        {
            Assert.AreEqual(0, ViewModel.UndoManager.UndoList.Count, "UndoListe muss von anfang an Leer sein.");
            //
            // TODO: Add test logic here
            //
        }
    }
}
