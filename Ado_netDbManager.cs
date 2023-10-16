using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Codium
{
    public class Ado_netDbManager
    {
        private object _locker = new object();
        private static Ado_netDbManager? instance=null;
        public string? ConnectionString { get; private set; }=null;
        private Ado_netDbManager(string connectionToServerString) 
        { 
            this.ConnectionString = connectionToServerString;
        }
        public Ado_netDbManager GetInstance(string ConnnectionString)
        {
            lock (_locker)
            {
                if (instance == null)
                {
                    instance = new Ado_netDbManager(ConnnectionString);
                }
            }
            return instance;
        }
        public bool CreateDatabase(string pathToDatabase)
        {
            bool isDbCreated = false;
            String strQuery = "Create Database Codium_Data On ( NAME = Codium_Data, FileName='" + pathToDatabase + "' )";
            using (SqlConnection myConn = new SqlConnection(this.ConnectionString))
            {
                using (SqlCommand myCommand = new SqlCommand(strQuery, myConn))
                {
                    try
                    {
                        myConn.Open();
                        myCommand.ExecuteNonQuery();
                        isDbCreated = true;
                    }
                    catch (Exception ex)
                    {
                        isDbCreated = false;
                    }
                }
            }
          
            return isDbCreated;
        }

        public bool CreateDatabaseTables(string pathToDatabase)
        {
            bool isDbCreated = false;
            String strQuery = "";
            using (SqlConnection myConn = new SqlConnection("Server=localhost;Integrated security=SSPI"))
            {
                using (SqlCommand myCommand = new SqlCommand(strQuery, myConn))
                {
                    using (SqlDataAdapter myDataAdapter = new SqlDataAdapter())
                    {
                        try
                        {
                            myConn.Open();

                            isDbCreated = true;
                        }
                        catch (Exception ex)
                        {
                            isDbCreated = false;
                        }
                    }
                }
            }
           
            return isDbCreated;
        }
          
    }
}
