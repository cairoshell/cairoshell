using System;

namespace CairoDesktop.Common
{
    /// <summary>
    /// Defines an EventArgs 
    /// </summary>
    public sealed class TypeCollectionEventArgs : ObjectEventArgs
    {
        public TypeCollectionEventArgs(Type context, ObjectActions action) :
            base(context, action)
        {

        }

        public new Type Context
        {
            get
            {
                return (Type)base.Context;
            }
        }
    }
}