using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace Codium
{
    public class Ado_netDbManager
    {
        private static object _locker = new object();
        private static Ado_netDbManager? instance=null;
        public string? ConnectionString { get; private set; }=null;
        private Ado_netDbManager(string connectionToServerString) 
        { 
            this.ConnectionString = connectionToServerString;
        }
        public static Ado_netDbManager GetInstance(string ConnnectionString)
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
        public bool CreateDatabaseIfNotExist(string pathToDatabase)
        {
            bool dbExists = false;
            string cmdText = String.Format("SELECT * FROM sys.databases where Name='Codium_Data'");
            String strQuery = "Create Database Codium_Data On ( NAME = Codium_Data, FileName='" + pathToDatabase + "' )";
            using (SqlConnection myConn = new SqlConnection(this.ConnectionString))
            {
                try
                {
                    myConn.Open();
                    using (SqlCommand checkDbCommand = new SqlCommand(cmdText, myConn))
                    {
                        using (SqlDataReader reader = checkDbCommand.ExecuteReader())
                        {
                            dbExists = reader.HasRows;
                        }
                    }
                    if (!dbExists)
                    {
                        using (SqlCommand myCommand = new SqlCommand(strQuery, myConn))
                        {
                            myCommand.ExecuteNonQuery();
                            dbExists = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    dbExists = false;
                }
               
            }
            return dbExists;
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
