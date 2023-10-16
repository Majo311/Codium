using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace Codium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? JsonFile = null;
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
                }
            }
        }
    }
}
