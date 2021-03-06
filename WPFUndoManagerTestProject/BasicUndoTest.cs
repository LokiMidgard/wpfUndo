﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

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
        }

        [TestMethod]
        public void RedoListIsDefaultEmpty()
        {
            Assert.AreEqual(0, ViewModel.UndoManager.RedoList.Count, "RedoListe muss von anfang an Leer sein.");
        }

        [TestMethod]
        public void PropertyCanUndo()
        {
            Assert.AreEqual(false, ViewModel.UndoManager.Undo.CanExecute(null), "Wenn noch keine Änderungen gemacht wurden darf man kein Undo machen.");
            ViewModel.FirstName = "test";
            Assert.AreEqual(true, ViewModel.UndoManager.Undo.CanExecute(null), "Nach einer Änderung muss man dies Rückgänig machen können.");
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(false, ViewModel.UndoManager.Undo.CanExecute(null), "Wenn alle Änderungen rückgänig gemacht wurden darf man kein Undo machen.");
        }
        [TestMethod]
        public void PropertyCanRedo()
        {
            Assert.AreEqual(false, ViewModel.UndoManager.Redo.CanExecute(null));
            ViewModel.FirstName = "test";
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(true, ViewModel.UndoManager.Redo.CanExecute(null));
            ViewModel.UndoManager.Redo.Execute(null);
            Assert.AreEqual(false, ViewModel.UndoManager.Redo.CanExecute(null));
        }

        [TestMethod]
        public void PropertyUndo()
        {
            var oldFirstName = ViewModel.FirstName;
            var oldLastName = ViewModel.SurName;
            var newFirstName = "My new First Name";
            ViewModel.FirstName = newFirstName;
            Assert.AreEqual(oldLastName, ViewModel.SurName, "Surname darf nciht geändert sein.");
            Assert.AreEqual(newFirstName, ViewModel.FirstName, "FirstName muss den Neuen wert haben");

            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(oldLastName, ViewModel.SurName, "Surname darf nicht geändert sein.");
            Assert.AreEqual(oldFirstName, ViewModel.FirstName, "Firstname muss wieder den Anfangswert haben.");
        }

        [TestMethod]
        public void PropertyRedo()
        {
            var firstFirstName = ViewModel.FirstName;
            var seccondFirstName = "Hans";
            var thirdFirstName = "Klaus";

            ViewModel.FirstName = seccondFirstName;
            ViewModel.FirstName = thirdFirstName;
            Assert.AreEqual(thirdFirstName, ViewModel.FirstName);
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(seccondFirstName, ViewModel.FirstName);
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(firstFirstName, ViewModel.FirstName);
            ViewModel.UndoManager.Redo.Execute(null);
            Assert.AreEqual(seccondFirstName, ViewModel.FirstName);
        }

        [TestMethod]
        public void ChangeCollectionTest()
        {
            String[] toAdd = new string[10];
            for (int i = 0; i < toAdd.Length; i++)
            {
                toAdd[i] = i.ToString();
            }

            var col= ViewModel.StringList;
            Assert.AreEqual(0,col.Count);
            col.Add(toAdd[0]);
            Assert.AreEqual(1, col.Count);
            Assert.AreEqual(toAdd[0], col[0]);
            ViewModel.UndoManager.Undo.Execute(null);
            Assert.AreEqual(0, col.Count);
            ViewModel.UndoManager.Redo.Execute(null);
            Assert.AreEqual(1, col.Count);
            Assert.AreEqual(toAdd[0], col[0]);

            for (int i = 1; i < toAdd.Length; i++)
            {
                col.Add(toAdd[i]);
            }

            col.RemoveAt(5);

            Assert.AreEqual(toAdd.Length - 1, col.Count);
            Assert.AreEqual(toAdd[6], col[5]);

            ViewModel.UndoManager.Undo.Execute(null);


            Assert.AreEqual(toAdd.Length , col.Count);
            Assert.AreEqual(toAdd[5], col[5]);
        }

        [TestMethod]
        public void CollectionMoveTest()
        {
            for (int i = 0; i < 10; i++)
            {
                ViewModel.StringList.Add(i.ToString());
            }
            ViewModel.StringList.Move(3, 7);

            Assert.AreEqual(4.ToString(), ViewModel.StringList[3]);
            Assert.AreEqual(3.ToString(), ViewModel.StringList[7]);

            ViewModel.UndoManager.Undo.Execute(null);

            Assert.AreEqual(3.ToString(), ViewModel.StringList[3]);
            Assert.AreEqual(7.ToString(), ViewModel.StringList[7]);

            ViewModel.UndoManager.Redo.Execute(null);

            Assert.AreEqual(4.ToString(), ViewModel.StringList[3]);
            Assert.AreEqual(3.ToString(), ViewModel.StringList[7]);

        }

        [TestMethod]
        public void CollectionReplace()
        {
            for (int i = 0; i < 10; i++)
            {
                ViewModel.StringList.Add(i.ToString());
            }

            Assert.AreEqual(3.ToString(), ViewModel.StringList[3]);
            
            var replacment = "replaced";
            ViewModel.StringList[3]=replacment;

            Assert.AreEqual(replacment, ViewModel.StringList[3]);

            ViewModel.UndoManager.Undo.Execute(null);

            Assert.AreEqual(3.ToString(), ViewModel.StringList[3]);

            ViewModel.UndoManager.Redo.Execute(null);

            Assert.AreEqual(replacment, ViewModel.StringList[3]);

        }
    }
}
