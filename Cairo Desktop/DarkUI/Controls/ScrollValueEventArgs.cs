using System;

namespace DarkUI.Controls
{
    public class ScrollValueEventArgs : EventArgs
    {
        public int Value { get; private set; }

        public ScrollValueEventArgs(int value)
        {
            Value = value;
        }
    }
}
