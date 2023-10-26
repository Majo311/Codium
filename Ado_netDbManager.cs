using Codium.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Xml;

namespace Codium
{
    public class Ado_netDbManager
    {
        private static object _locker = new object();
        private static Ado_netDbManager? instance = null;
        public string? ConnectionString { get; private set; } = null;
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

        public bool CreateTablesIfNotExist()
        {
            bool isTablesCreated = false;
            String checkTableCommandMessages = "SELECT OBJECT_ID('[Codium_Data].[dbo].[Messages]', 'U') AS Result";
            String checkTableCommandEvents = "SELECT OBJECT_ID('[Codium_Data].[dbo].[Events]', 'U') AS Result";
            String checkTableCommandOds = "SELECT OBJECT_ID('[Codium_Data].[dbo].[Odds]', 'U') AS Result";

            String strQueryMessages = "create table [Codium_Data].[dbo].[Messages](" +
                                                            "MessageID nvarchar(40) Primary key," +
                                                            "GeneratedDate  datetime)";

            String strQueryEvents = "create table [Codium_Data].[dbo].[Events](" +
                                                "MessageID nvarchar(40) Primary key," +
                                                "ProviderEventID int," +
                                                "EventName nvarchar(MAX)," +
                                                "EventDate datetime2)";

            String strQuerytblOdds = "create table [Codium_Data].[dbo].[Odds](" +
                                                            "ProviderOddsID  int NOT NULL Primary key," +
                                                            "MessageID nvarchar(40) NOT NULL FOREIGN KEY REFERENCES dbo.Events(MessageID)," +
                                                            "OddsName nvarchar(MAX)," +
                                                            "OddsRate float,"+
                                                            "Status nvarchar(10))";

            if (CreateTableIfNotExist(this.ConnectionString, checkTableCommandMessages, strQueryMessages) &&
                CreateTableIfNotExist(this.ConnectionString, checkTableCommandEvents, strQueryEvents) &&
                CreateTableIfNotExist(this.ConnectionString, checkTableCommandOds, strQuerytblOdds))
            {
                isTablesCreated = true;
            }
            return isTablesCreated;
        }
        private bool CreateTableIfNotExist(string ConectionString, string queryToCheck, string queryToCreate)
        {
            bool isTableCreated = false;
            using (SqlConnection myConn = new SqlConnection(ConectionString))
            {
                try
                {
                    string OBJECT_ID = String.Empty;
                    myConn.Open();
                    using (SqlCommand checkDbCommand = new SqlCommand(queryToCheck, myConn))
                    {
                        using (SqlDataReader reader = checkDbCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OBJECT_ID = reader[0].ToString();
                            }   
                        }
                    }
                    if (String.IsNullOrEmpty(OBJECT_ID))// if table don't exist
                    {
                        using (SqlCommand myCommand = new SqlCommand(queryToCreate, myConn))
                        {
                            myCommand.ExecuteNonQuery();
                        }
                        isTableCreated = true;
                    }
                    else
                        isTableCreated = true; 

                }
                catch (Exception ex)
                {
                    isTableCreated = false;
                }
                return isTableCreated;
            }

        }

        public void InsertMessages(IList<Message> messages) 
        {
            using (SqlConnection myConn = new SqlConnection(this.ConnectionString))
            {
                myConn.Open();
                SqlTransaction sqlTran = myConn.BeginTransaction();
                SqlCommand command = myConn.CreateCommand();
                command.Transaction= sqlTran;
                string x = "";
                try
                {
                    DateTime eventDateTime;
                    foreach (Message message in messages)
                    {
                        command.CommandText = "Insert into [Codium_Data].[dbo].[Messages](MessageID,GeneratedDate)" +
                                              "Values('"+ message.MessageID+"','"+DateTime.Parse(message.GeneratedDate)+"')";
                        command.ExecuteNonQuery();
                        eventDateTime = new DateTime();
                        command.CommandText = "Insert into [Codium_Data].[dbo].[Events](MessageID,ProviderEventID,EventName,EventDate)" +
                                              "Values('"+ message.MessageID+"','"+message.Event.ProviderEventID+"','"+ message.Event.EventName+"','"+ eventDateTime + "')";
                        command.ExecuteNonQuery();
                        string oddInsertQuery = "Insert into [Codium_Data].[dbo].[Odds](ProviderOddsID,MessageID,OddsName,OddsRate,Status) Values";
                        int i= 0;
                        foreach(Odd o in message.Event.OddsList)
                        {
                            oddInsertQuery+= "('"+o.ProviderOddsID.ToString()+"', '"+
                                                            message.MessageID+","+
                                                            o.OddsName+"','"+
                                                            string.Format(new System.Globalization.CultureInfo("en-GB"),"{0:F}", o.OddsRate)+"','"+
                                                            o.Status+"')";
                            if(i!=message.Event.OddsList.Count-1)
                            oddInsertQuery += ",";
                            i++;
                        }
                        command.CommandText = oddInsertQuery;
                        command.ExecuteNonQuery();
                        sqlTran.Commit();
                    }

                }
                catch (Exception ex)
                {
                    try
                    {
                        // Attempt to roll back the transaction.
                        sqlTran.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        // Throws an InvalidOperationException if the connection
                        // is closed or the transaction has already been rolled
                        // back on the server.
                        Console.WriteLine(exRollback.Message);
                    }
                }
                
               
            }
        }
    }
}
