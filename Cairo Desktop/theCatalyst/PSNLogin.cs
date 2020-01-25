using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace theCatalyst
{
    public partial class PSNLogin : Form
    {
        public string CalculateMD5Hash(string input)

        {

            // step 1, calculate MD5 hash from input

            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {

                sb.Append(hash[i].ToString("X2"));

            }

            return sb.ToString();

        }

        public string Username { get; set; }
        public string Password { get; set; }
        public PSNLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((Properties.Settings.Default.LastAccounts.Split(',').Contains(comboBox1.Text) == false) & checkBox1.Checked){
                Properties.Settings.Default.LastAccounts = Properties.Settings.Default.LastAccounts + comboBox1.Text + ",";
                Properties.Settings.Default.Save();
            }
            Username = comboBox1.Text;
            Password = CalculateMD5Hash(textBox1.Text);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Username = "";
            Password = "";
            this.Close();
        }

        private void PSNLogin_Load(object sender, EventArgs e)
        {
            foreach (string x in Properties.Settings.Default.LastAccounts.Split(','))
            {
                comboBox1.Items.Add(x);
            }
        }
    }
}
