using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfTestApplication
{
    /// <summary>
    /// Interaktionslogik für View1.xaml
    /// </summary>
    public partial class View1 : UserControl
    {
        private Point listBox1_startPoint;
        private Point listBox2_startPoint;

        internal const String con = "exchangeListboxStringFormat";


        public View1()
        {
            this.InitializeComponent();



            // Fügen Sie Code, der bei der Objekterstellung erforderlich ist, unter diesem Punkt ein.
        }


        private void listBox1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            listBox1_startPoint = e.GetPosition(null);

        }

        private void listBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = listBox1_startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listView == null || listViewItem == null)
                    return;
                // Find the data behind the ListViewItem
                String contact = (String)listView.ItemContainerGenerator.
                    ItemFromContainer(listViewItem);

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject(con, contact);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
            }

        }

        private void listBox2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            listBox2_startPoint = e.GetPosition(null);

        }

        private void listBox2_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = listBox2_startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem =
                    FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listView == null || listViewItem == null)
                    return;

                // Find the data behind the ListViewItem
                String contact = (String)listView.ItemContainerGenerator.
                    ItemFromContainer(listViewItem);

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject(con, contact);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Move);
            }

        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

    }

    public static class ListDrop
    {
        public static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
        "DropCommand",
        typeof(ICommand),
        typeof(ListDrop),
        new PropertyMetadata(null, OnDropCommandChange));

        private static readonly IDictionary<DependencyObject, DragEventHandler> oldHandlersDrop;
        private static readonly IDictionary<DependencyObject, DragEventHandler> oldHandlersDragEnter;

        static ListDrop()
        {
            oldHandlersDrop = new Dictionary<DependencyObject, DragEventHandler>();
            oldHandlersDragEnter = new Dictionary<DependencyObject, DragEventHandler>();
        }

        public static void SetDropCommand(DependencyObject source, ICommand value)
        {
            source.SetValue(DropCommandProperty, value);
        }

        public static ICommand GetDropCommand(DependencyObject source)
        {
            return (ICommand)source.GetValue(DropCommandProperty);
        }

        private static void OnDropCommandChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ICommand command = e.NewValue as ICommand;
            UIElement uiElement = d as UIElement;

            if (oldHandlersDrop.ContainsKey(uiElement))
            {
                uiElement.Drop -= oldHandlersDrop[uiElement];
                oldHandlersDrop.Remove(uiElement);
            }
            if (oldHandlersDragEnter.ContainsKey(uiElement))
            {
                uiElement.DragOver -= oldHandlersDrop[uiElement];
                uiElement.DragEnter -= oldHandlersDrop[uiElement];
                oldHandlersDragEnter.Remove(uiElement);
            }

            if (command != null && uiElement != null)
            {
                DragEventHandler newDropHandler = (sender, args) => command.Execute(args.Data.GetData(View1.con));
                uiElement.Drop += newDropHandler;
                oldHandlersDrop.Add(uiElement, newDropHandler);

                DragEventHandler newDragEnterHandler = (sender, ev) =>
                {
                    
                    if (!command.CanExecute(ev.Data.GetData(View1.con)))
                    {
                        ev.Effects = DragDropEffects.None;
                    }
                    ev.Handled = true;
                };
                uiElement.DragOver += newDragEnterHandler;
                uiElement.DragEnter += newDragEnterHandler;
                oldHandlersDragEnter.Add(uiElement, newDragEnterHandler);

            }
        }
    }



}