using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Th145_palette_editor
{
    class UndoStack<T>
    {
        List<T> stack;
        int i; // Index to the next element to add
        public bool ignoreAdd; // It true, calls to Add will be ignored.

        public UndoStack()
        {
            stack = new List<T>();
            i = 0;
            ignoreAdd = false;
        }

        public void Add(T elem)
        {
            if (ignoreAdd)
                return;
            if (stack.Count > i)
                stack.RemoveRange(i, stack.Count - i);
            stack.Add(elem);
            i++;
        }

        public bool CanUndo { get { return i > 1; } }
        public T Undo()
        {
            i--;
            return stack[i];
        }

        public bool CanRedo { get { return i < stack.Count; } }
        public T Redo()
        {
            i++;
            return stack[i - 1];
        }
    }
}
