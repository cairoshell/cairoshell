using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using SHAppBarMessage1.Common;
using CairoDesktop.Interop;
using System.Resources;
using System.Windows.Input;

namespace CairoDesktop
{
    public static class CustomCommands
    {
        private static Dispatcher _dispatcher;
        private static RoutedUICommand openSearchResultCommand = new RoutedUICommand("OpenSearchResult", "OpenSearchResult", typeof(CustomCommands));


        static CustomCommands()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public static RoutedUICommand OpenSearchResult
        {
            get
            {
                return openSearchResultCommand;
            }
        }


    }
}
