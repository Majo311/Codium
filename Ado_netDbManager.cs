﻿using Codium.Models;
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
            String checkTableCommandOds = "SELECT OBJECT_ID('[Codium_Data].[dbo].[Odds]', 'U') AS Result";                                                                    
            String checkTableCommandEvents = "SELECT OBJECT_ID('[Codium_Data].[dbo].[Events]', 'U') AS Result";
            String checkTableCommandMessages = "SELECT OBJECT_ID('[Codium_Data].[dbo].[Messages]', 'U') AS Result";

            String strQuerytblOdds = "create table [Codium_Data].[dbo].[Odds](" +
                                                            "ProviderOddsID  int NOT NULL Primary key," +
                                                            "OddsName nvarchar(MAX)," +
                                                            "OddsRate float," +
                                                            "Status nvarchar(10)," +
                                                            "ProviderEventID int)";
            String strQueryEvents = "create table [Codium_Data].[dbo].[Events](" +
                                                            "ProviderEventID int Primary key," +
                                                            "EventName nvarchar(MAX)," +
                                                            "EventDate datetime2)";
            String strQueryMessages = "create table [Codium_Data].[dbo].[Messages](" +
                                                            "MessageID nvarchar(40) Primary key," +
                                                            "GeneratedDate  datetime," +
                                                            "ProviderEventID int)";


            if (CreateTableIfNotExist(this.ConnectionString, checkTableCommandOds, strQuerytblOdds) &&
                CreateTableIfNotExist(this.ConnectionString, checkTableCommandEvents, strQueryEvents) &&
                CreateTableIfNotExist(this.ConnectionString, checkTableCommandMessages, strQueryMessages))
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
            string insertQuery="";
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
                        command.CommandText = "Insert into [Codium_Data].[dbo].[Messages](MessageID,GeneratedDate,ProviderEventID)" +
                                              "Values('"+ message.MessageID+"','"+DateTime.Parse(message.GeneratedDate)+"','"+ message.Event.ProviderEventID+"')";
                        command.ExecuteNonQuery();
                        eventDateTime = new DateTime();
                        command.CommandText = "Insert into [Codium_Data].[dbo].[Events](ProviderEventID,EventName,EventDate)" +
                                              "Values('"+message.Event.ProviderEventID+"','"+ message.Event.EventName+"','"+ eventDateTime + "')";
                        command.ExecuteNonQuery();
                        
                        foreach(Odd o in message.Event.OddsList)
                        {
                            command.CommandText = "Insert into [Codium_Data].[dbo].[Odds](ProviderOddsID,OddsName,OddsRate,Status,ProviderEventID)" +
                                                  "Values('"+o.ProviderOddsID.ToString()+"','"+o.OddsName+"','"+
                                                  string.Format(new System.Globalization.CultureInfo("en-GB"), "{0:F}", o.OddsRate) + "','"+o.Status+"','"+message.Event.ProviderEventID.ToString()+"')";
                            command.ExecuteNonQuery();
                        }  
                    }
                    sqlTran.Commit();
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
