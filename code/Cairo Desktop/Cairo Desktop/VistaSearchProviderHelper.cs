using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using VistaSearchProvider;
using System.Collections.ObjectModel;
using System.Threading;
using SearchAPILib;
using System.Data.OleDb;
using System.Data;

namespace VistaSearchProvider
{
    class VistaSearchProviderHelper:DependencyObject
    {
        static CSearchManager cManager;
        static ISearchQueryHelper cHelper;
        static OleDbConnection cConnection;
        static WaitCallback workerCallback;
        static bool ToAbortFlag = false;
        static bool IsWorkingFlag = false;

        class SearchObjectState
        {
            public string SearchString;
            public ManualResetEvent Reset;

            public SearchObjectState(string searchStr)
            {
                SearchString = searchStr;
                Reset = new ManualResetEvent(false);
            }

            public SearchObjectState()
            {
                SearchString = string.Empty;
                Reset = new ManualResetEvent(false);
            }
        }
        static SearchObjectState searchObjState = new SearchObjectState();

        static VistaSearchProviderHelper()
        {
            m_results = new ThreadSafeObservableCollection<SearchResult>();
            cManager = new CSearchManagerClass();
        }

        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register("SearchText", typeof(string), typeof(VistaSearchProviderHelper), new UIPropertyMetadata(default(string),
            new PropertyChangedCallback(OnSearchTextChanged)));

        static void OnSearchTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //change
            ToAbortFlag = IsWorkingFlag;

            if (workerCallback == null)
                workerCallback = new WaitCallback(doSearch);

            searchObjState.SearchString = e.NewValue.ToString();
            //if (searchObjState.SearchString.Length > 2)
            {
                workerCallback.BeginInvoke(searchObjState, null, null);
            }

        }

        static void doSearch(Object state)
        {
            SearchObjectState sos = (SearchObjectState)state;
            if (sos.SearchString == string.Empty)
            {
                sos.Reset.Set();
                return;
            }
            cHelper = cManager.GetCatalog("SYSTEMINDEX").GetQueryHelper();
            cHelper.QuerySelectColumns = "\"System.ItemNameDisplay\",\"System.ItemPathDisplay\"";

            try
            {
                using (cConnection = new OleDbConnection(
                            cHelper.ConnectionString))
                {
                    cConnection.Open();
                    using (OleDbCommand cmd = new OleDbCommand(
                            cHelper.GenerateSQLFromUserQuery(
                                sos.SearchString
                            ),
                            cConnection))
                    {
                        if (cConnection.State == ConnectionState.Open)
                        {
                            using (OleDbDataReader reader = cmd.ExecuteReader())
                            {
                                m_results.Clear();
                                IsWorkingFlag = true;
            
                                while (!reader.IsClosed && reader.Read())
                                {
                                    m_results.Add(new SearchResult() { Name = reader[0].ToString(), Path = reader[1].ToString() });
                                    if (ToAbortFlag)
                                        break;
                                }
                                reader.Close();

                                IsWorkingFlag = false;
                            }
                        }

                    }
                    // TODO: Investigate possible RaceOnRCWCleanup exception.
                    cConnection.Close();    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            sos.Reset.Set();
            ToAbortFlag = false;

        }
        public string SearchText
        {
            get { return GetValue(SearchTextProperty).ToString(); }
            set { SetValue(SearchTextProperty, value); }
        }

        static ThreadSafeObservableCollection<SearchResult> m_results;
        public ReadOnlyObservableCollection<SearchResult> Results
        {
            get { return new ReadOnlyObservableCollection<SearchResult>(m_results); }
        }
    }

    public class SearchResult
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
