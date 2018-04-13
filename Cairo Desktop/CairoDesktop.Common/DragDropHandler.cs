using System;
using System.Windows;
using System.Windows.Input;

namespace CairoDesktop.Common
{
    public class DragDropHandler
    {
        private FrameworkElement dragElement = null;
        private bool dragging = false;
        //private bool inDragDrop = false;
        private Point dragStart;
        private DataObject dataObject = null;

        public DragDropHandler(FrameworkElement dragElement, DataObject dataObject)
        {
            this.dragElement = dragElement;
            this.dataObject = dataObject;

            dragElement.MouseLeftButtonDown += new MouseButtonEventHandler(dragElement_MouseLeftButtonDown);
            dragElement.MouseMove += new MouseEventHandler(dragElement_MouseMove);
        }

        public void dragElement_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!dragElement.IsMouseCaptured)
            {
                dragging = true;
                dragStart = e.GetPosition(dragElement);
            }
        }

        public void dragElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point currentPos = e.GetPosition(dragElement);

                if ((Math.Abs(currentPos.X - dragStart.X) > 5) || (Math.Abs(currentPos.Y - dragStart.Y) > 5))
                {
                    dragElement.CaptureMouse();

                    //inDragDrop = true;
                    DragDropEffects de = DragDrop.DoDragDrop(dragElement, dataObject, DragDropEffects.Move);
                    //inDragDrop = false;
                    dragging = false;
                    dragElement.ReleaseMouseCapture();
                }
            }
        }
    }
}
