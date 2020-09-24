using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;

namespace cairoCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       /*this program gets the calculatiton input the users 
        * 
        * 
        */
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnONe_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 1.ToString();
        }

        private void BtnTwo1_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 2.ToString();
        }

        private void BtnThre1_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 3.ToString();
        }

        private void BtnFour_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 4.ToString();
        }

        private void BtnFive_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 5.ToString();
        }

        private void BtnSix_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 6.ToString();
        }

        private void BtnSeven_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 7.ToString();
        }

        private void BtnEight_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 8.ToString();
        }

        private void BtnNine_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 9.ToString();
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += "+";
        }

        private void BtntEquals_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                DataTable td = new DataTable();
                var result = td.Compute(lblresult.Content.ToString(), "");
                lblresult.Content = result.ToString();
            }
        }

        private void BtntEquals_Click(object sender, RoutedEventArgs e) { 
            DataTable td = new DataTable();
        var result = td.Compute(lblresult.Content.ToString(), "");
        
            try
            {
                
                
            }
            catch(Exception)
            {
                lblresult.Content = "not a valid computation becuase" + e.ToString(); 
            }
            finally
            {
                lblresult.Content = result.ToString();
            }
            }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += "-";
        }

        private void Btntimes_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += "*";
        }

        private void BtnDecimal_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += ".";
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content = "";
        }

        private void BtnZero_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += 0.ToString();
        }

        private void BtnDivide_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += "/";
        }

        private void BtnLeftPartheses_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += "(";
        }

        private void BtnrightPartheses_Click(object sender, RoutedEventArgs e)
        {
            lblresult.Content += ")";
        }

        private void BtnExponent_Click(object sender, RoutedEventArgs e)
        {

            DataTable td = new DataTable();
            var result = td.Compute("",lblresult.Content.ToString());
            lblresult.Content = Math.Pow(Convert.ToDouble(result),2).ToString();

        }

        private void BtntEquals_KeyDown_1(object sender, KeyEventArgs e) { 
       
        }

        private void BtntEquals_KeyDown_2(object sender, KeyEventArgs e)
        {
          
        }

        private void BtnSwtich_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("this is a feature under developement");
        }
    }
}
