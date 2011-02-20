using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Midgard.WPFUndoManager
{
    class StackList<T> : IList<T>
    {

        #region Stack

        public T Pop()
        {
            var first = list.First.Value;
            RemoveAt(0);
            return first;
        }

        public void Push(T item)
        {
            Add(item);
        }

        public T Peek()
        {
            return list.First.Value;
        }

        #endregion

        LinkedList<T> list= new LinkedList<T>();

        int currentIndex;

        LinkedListNode<T> currentNode;

        #region IList<T> Members

        public int IndexOf(T item)
        {
            if (list.Count == 0)
                return -1;
            currentIndex = 0;
            currentNode = list.First;
            while (!currentNode.Value.Equals(item))
            {
                ++currentIndex;
                currentNode = currentNode.Next;
                if (currentNode == null)
                    return -1;
            }
            return currentIndex;
        }

        public void Insert(int index, T item)
        {
            if (index > list.Count || index < 0)
                throw new IndexOutOfRangeException(index + " Ausserhalb dem Breich von " + list.Count);
            if (index == list.Count)
            {
                currentIndex = index;
                list.AddLast(item);
                currentNode = list.Last;
                return;
            }
            goToIndex(index);
            currentNode = list.AddBefore(currentNode, item);
        }

        private void goToIndex(int index)
        {
            int distanceToCurrent = Math.Abs(index - currentIndex);
            int distanceToFirst = index;
            int distanceToLast = list.Count - index;

            if (distanceToLast < distanceToCurrent && distanceToLast < distanceToFirst)
            {
                //Jump To last
                currentIndex = list.Count - 1;
                currentNode = list.Last;
            }
            else if (distanceToFirst < distanceToCurrent)
            {
                //Jump to First
                currentIndex = 0;
                currentNode = list.First;
            }
            while (this.currentIndex < index)
            {
                ++currentIndex;
                currentNode = currentNode.Next;
            }
            while (currentIndex > index)
            {
                --currentIndex;
                currentNode = currentNode.Previous;
            }
        }

        public void RemoveAt(int index)
        {
            if (index >= list.Count || index < 0)
                throw new IndexOutOfRangeException(index + " Ausserhalb dem Breich von " + Count);
            goToIndex(index);
            if (index == list.Count - 1)
            {
                currentNode = currentNode.Previous;
                --index;
                list.RemoveLast();
            }

        }

        public T this[int index]
        {
            get
            {
                if (index >= list.Count || index < 0)
                    throw new IndexOutOfRangeException(index + " Ausserhalb dem Breich von " + list.Count);
                goToIndex(index);
                return currentNode.Value;
            }
            set
            {
                if (index > list.Count || index < 0)
                    throw new IndexOutOfRangeException(index + " Ausserhalb dem Breich von " + list.Count);
                if (index == list.Count)
                {
                    currentIndex = index;
                    list.AddLast(value);
                    currentNode = list.Last;
                    return;
                }
  
                goToIndex(index);
                currentNode.Value = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            Insert(0, item);
        }

        public void Clear()
        {

            list.Clear();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int i = IndexOf(item);
            if (i == -1)
                return false;
            RemoveAt(i);
            return true;
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return (list as IEnumerable<T>).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (list as System.Collections.IEnumerable).GetEnumerator();
        }

        #endregion
    }
}
