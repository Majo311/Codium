using Codium.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Windows;
using System.ComponentModel;

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
        BackgroundWorker worker=new BackgroundWorker();

      
        public MainWindow()
        {
            InitializeComponent();
            this.worker.ProgressChanged += Worker_ProgressChanged;
            this.worker.DoWork += Worker_InsertToDb;
        }
        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Json File (*.json*)|*.json*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamReader streamReader = new StreamReader(openFileDialog.FileName))
                    {
                        JsonFile = streamReader.ReadToEnd();
                        JArray array = JsonConvert.DeserializeObject<JArray>(JsonFile)!;
                        this.messages = (array).Select(x =>
                        new Message(x["MessageID"].ToString(), x["GeneratedDate"].ToString(), Event: x["Event"].GetEvent())
                        ).ToList();
                        if (this.messages.Count > 0)
                        {
                            tb_output.Text += Environment.NewLine+"Json file was readed and deserialized successfuly!";
                        }
                        else
                            tb_output.Text += Environment.NewLine+"Json file is empty!";

                    }
                }
                catch(Exception ex) 
                {
                    tb_output.Text+= Environment.NewLine + ex.Message;
                } 
            }
        }
        private void Worker_InsertToDb(object? sender, DoWorkEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Ado_netDbManager Ado_netDbManager = Ado_netDbManager.GetInstance(this.connectionString);
            if (Ado_netDbManager.CreateDatabaseIfNotExist(database) && Ado_netDbManager.CreateTablesIfNotExist())
            {
                if (!Ado_netDbManager.WasDataInserted)
                {
                    _ = Ado_netDbManager.InsertMessagesAsync(this.messages);
                    this.Dispatcher.Invoke(() =>
                    {
                        stopwatch.Stop();
                        TimeSpan ts = stopwatch.Elapsed;
                        MessageBox.Show("Data was inserted to DB. It taked " + ts.Minutes.ToString() + ":" + ts.Seconds + ":" + ts.Milliseconds.ToString());
                        this.tb_output.Text += Environment.NewLine + "Data was inserted to DB. It taked " + ts.Minutes.ToString() + ":" + ts.Seconds + ":" + ts.Milliseconds.ToString();
                    });     
                }
                else
                {
                    MessageBox.Show("Data wasn't inserted to Db");
                }
            }
        }
        private void btn_SaveToDb_Click(object sender, RoutedEventArgs e)
        {
           this.worker.RunWorkerAsync();
        }
    }
}
