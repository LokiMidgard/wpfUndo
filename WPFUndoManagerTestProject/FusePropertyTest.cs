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
            SecondViewModel = new FusePropertyTestViewModel();
        }

        FusePropertyTestViewModel ViewModel { get; set; }
        FusePropertyTestViewModel SecondViewModel { get; set; }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TryToFuse()
        {
            var originalFuse = ViewModel.FuseInt;
            ViewModel.FuseInt++;
            ViewModel.FuseInt++;
            Assert.AreEqual(originalFuse + 2, ViewModel.FuseInt);
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(originalFuse, ViewModel.FuseInt);
        }

        [TestMethod]
        public void MayNotFuseTwoDifferentViewmodels()
        {
            var origionaFuse1 = ViewModel.FuseInt;
            var origionalFuse2 = SecondViewModel.FuseInt;

            Assert.AreEqual(origionaFuse1,origionalFuse2);
            Assert.AreSame(ViewModel.UndoManager, SecondViewModel.UndoManager);

            var new1 = origionaFuse1 + 1;
            var new2 = origionalFuse2+2;

            ViewModel.FuseInt = new1;
            SecondViewModel.FuseInt = new2;

            Assert.AreEqual(new1, ViewModel.FuseInt);
            Assert.AreEqual(new2, SecondViewModel.FuseInt);

            ViewModel.UndoManager.Undo.Execute(null);

            Assert.AreEqual(new1, ViewModel.FuseInt);
            Assert.AreEqual(origionalFuse2, SecondViewModel.FuseInt);

            ViewModel.UndoManager.Undo.Execute(null);

            Assert.AreEqual(origionaFuse1, ViewModel.FuseInt);
            Assert.AreEqual(origionalFuse2, SecondViewModel.FuseInt);


        }
    }
}
