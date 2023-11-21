using Codium.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Codium
{
    public class Ado_netDbManager
    {
        private static object _locker = new object();
        private static Ado_netDbManager? instance = null;
        public string? ConnectionString { get; private set; } = null;

        public bool WasDataInserted {  get; private set; }
        private Ado_netDbManager(string connectionToServerString)
        {
            this.ConnectionString = connectionToServerString;
            this.WasDataInserted = false;
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
                                                            "Id int NOT NULL IDENTITY(1,1) PRIMARY KEY," +
                                                            "MessageID nvarchar(40) NOT NULL," +
                                                            "GeneratedDate  datetime)";

            String strQueryEvents = "create table [Codium_Data].[dbo].[Events](" +
                                                "Id int NOT NULL IDENTITY(1,1) PRIMARY KEY," +
                                                "Message_Id int NOT NULL," +
                                                "ProviderEventID int," +
                                                "EventName nvarchar(MAX)," +
                                                "EventDate datetime2)";

            String strQuerytblOdds = "create table [Codium_Data].[dbo].[Odds](" +
                                                            "Id int NOT NULL IDENTITY(1,1) PRIMARY KEY," +
                                                            "ProviderOddsID  int NOT NULL," +
                                                            "Event_Id int NOT NULL FOREIGN KEY REFERENCES dbo.Events(Id) ON DELETE CASCADE ON UPDATE CASCADE," +
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
                    if (String.IsNullOrEmpty(OBJECT_ID))// if table doesn't exist
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

        public async Task<TimeSpan> InsertMessages(IList<Message> messages) 
        {
            return await Task<TimeSpan>.Run(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                using (SqlConnection myConn = new SqlConnection(this.ConnectionString))
                {
                    myConn.Open();
                    SqlCommand command = myConn.CreateCommand();
                    try
                    {
                        DateTime eventDateTime;
                        foreach (Message message in messages)
                        {
                            command.CommandText = "Insert into [Codium_Data].[dbo].[Messages](MessageID,GeneratedDate)" +
                                                  "Values('" + message.MessageID + "','" + DateTime.Parse(message.GeneratedDate) + "')";
                            command.ExecuteNonQuery();
                            command.CommandText = "Select max(Id) from [Codium_Data].[dbo].[Messages]";
                            int message_Id = (int)(command.ExecuteScalar());
                            eventDateTime = new DateTime();
                            command.CommandText = "Select Count(ProviderEventID) from [Codium_Data].[dbo].[Events]";
                            int eventId= (int)(command.ExecuteScalar());
                            if (eventId == 0)
                            {
                                command.CommandText = ("Insert into [Codium_Data].[dbo].[Events](Message_Id,ProviderEventID,EventName,EventDate)" +
                                                      "Values('" + message_Id.ToString() + "','" + message.Event.ProviderEventID + "','" + message.Event.EventName + "','" + eventDateTime + "')");
                            }
                            else
                            {
                                command.CommandText = "Update [Codium_Data].[dbo].[Events] set EventDate='"+ eventDateTime+"' where [Id]='" + message_Id.ToString()+"'";
                            }
                            Random random = new Random();
                            Thread.Sleep(random.Next(10));//simulation of extern API 
                            command.ExecuteNonQuery();
                            command.CommandText = "Select max(Id) from [Codium_Data].[dbo].[Events]";
                            int event_Id = (int)(command.ExecuteScalar());
                            await Parallel.ForEachAsync(message.Event.OddsList,async (o, cancellationToken) =>
                            {
                                lock (_locker)
                                {
                                    string oddInsertQuery = "Insert into [Codium_Data].[dbo].[Odds](ProviderOddsID,Event_Id,OddsName,OddsRate,Status) Values";
                                    oddInsertQuery += "('" + o.ProviderOddsID + "','" +
                                                             event_Id.ToString() + "','" +
                                                             o.OddsName + "','" +
                                                             string.Format(new System.Globalization.CultureInfo("en-GB"), "{0:F}", o.OddsRate) + "','" +
                                                             o.Status + "')";
                                    command.CommandText = oddInsertQuery;
                                    command.ExecuteNonQuery();
                                }
                            });
                        }
                        this.WasDataInserted = true;
                    }
                    catch (Exception ex)
                    {
                        this.WasDataInserted = false;
                    }
                    finally
                    {
                        myConn.Close();
                    }
                }
                stopwatch.Stop();
                return stopwatch.Elapsed;
            });
        }
    }
}
