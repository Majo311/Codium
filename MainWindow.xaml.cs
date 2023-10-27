using Codium.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Windows;

namespace Codium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? JsonFile = null;
        private string connectionString = "Server=.\\SQLEXPRESS;Integrated security=SSPI;database=master";
        private string database = "C:\\Codium_data.mdf";
        private List<Message> messages = new List<Message>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json File (*.json*)|*.json*";
            if (openFileDialog.ShowDialog() == true)
            {
                using (StreamReader streamReader = new StreamReader(openFileDialog.FileName))
                {
                    JsonFile=streamReader.ReadToEnd();
                    JArray array = JsonConvert.DeserializeObject<JArray>(JsonFile)!;
                    this.messages = (array).Select(x =>
                    new Message(x["MessageID"].ToString(), x["GeneratedDate"].ToString(), Event: x["Event"].GetEvent())

                    ).ToList();
                }
            }
        }

        private void btn_SaveToDb_Click(object sender, RoutedEventArgs e)
        {
            Ado_netDbManager Ado_netDbManager= Ado_netDbManager.GetInstance(this.connectionString);
            if(Ado_netDbManager.CreateDatabaseIfNotExist(database)&&Ado_netDbManager.CreateTablesIfNotExist())
            {
                if(!Ado_netDbManager.WasDataInserted)
                {
                    Ado_netDbManager.InsertMessages(this.messages);
                    MessageBox.Show("Data was inserted in Db or already there are to Db");
                }
                else
                {
                    MessageBox.Show("Data wasn't inserted to Db");
                }
                
            }

        }
    }
}
