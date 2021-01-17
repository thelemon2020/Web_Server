/*
* FILE : Logger.cs
* PROJECT : PROG2001 - Assignment #6
* PROGRAMMER : Chris Lemon
* FIRST VERSION : 2020 - 11 - 16
* REVISED ON : 2020 - 12 - 01
* DESCRIPTION : This file holds the logic for the Logger class
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a06
{

   /*
    * NAME : Logger
    * PURPOSE : This defines the Logger class.  It creates a log file and writes log entries to it when events happen on the server
    */
    static class Logger
    {
        const string filePath = "myOwnWebServer.log"; //the default filepath
      
      /*
       * METHOD : CreateLogFile()
       *
       * DESCRIPTION :  This method attempts to create or overwrite the file located at the default filepath
       *
       * PARAMETERS : None
       *
       * RETURNS : Nothing
       */
        static public bool CreateLogFile()
        {
            try
            {
                FileStream createFile = File.Create(filePath);//create the file
                createFile.Close();//close it so it can be used later
                return true;//return that everything worked
            }
            catch //if the file create fails
            {
                return false; //return that something went wrong
            }
        }

       /*
        * METHOD : WriteLog()
        *
        * DESCRIPTION :  This method takes two strings and writes them to a log file
        *
        * PARAMETERS : eventToWrite - the event that triggered the log write
        *              message - the description of the event
        *
        * RETURNS : Nothing
        */
        static public void WriteLog(string eventToWrite, string message)
        {
            StringBuilder logEntry = new StringBuilder(); // string builder to create the log entry
            logEntry.AppendFormat("{0} [{1}] - {2}", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), eventToWrite, message.Replace("\r\n", "")); //log entry created
            StreamWriter sw = new StreamWriter(filePath,true); //create a new stream writer to do the write operation
            sw.WriteLine(logEntry.ToString()); //write the log entry to log file
            sw.Close(); //close stream
        }

    }
}
