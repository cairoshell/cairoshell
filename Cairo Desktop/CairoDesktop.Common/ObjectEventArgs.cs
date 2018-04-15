using System;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Defines an EventArgs class containing an ObjectBase instance as the context.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough()]
    public class ObjectEventArgs : EventArgs
    {
        private object _context;
        private ObjectActions _action;

        /// <summary>
        /// Initializes a new instance of the ObjectEventArgs class.
        /// </summary>
        /// <param name="context">The context of the event.</param>
        /// <param name="action">The action being taken on the object.</param>
        public ObjectEventArgs(object context, ObjectActions action)
        {
            //if (context == null)
            //{
            //    throw new ArgumentNullException("context");
            //}

            _context = context;
            _action = action;
        }

        /// <summary>
        /// Returns the context of the event.
        /// </summary>
        public object Context
        {
            get
            {
                return _context;
            }
            set
            {
                _context = value;
            }
        }

        /// <summary>
        /// Returns the action being taken on the context.
        /// </summary>
        public ObjectActions Action
        {
            get
            {
                return _action;
            }
        }
    }
}