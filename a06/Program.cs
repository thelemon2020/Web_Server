/*
* FILE : Program.cs
* PROJECT : PROG2001 - Assignment #6
* PROGRAMMER : Chris Lemon
* FIRST VERSION : 2020 - 11 - 16
* REVISED ON : 2020 - 12 - 01
* DESCRIPTION : This file acts as a simple web server that takes in HTTP get requests and sends back an appropriate HTTP response.  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace a06
{
   /*
    * NAME : Program
    * PURPOSE : This defines the Program class.  It's sole purpose is to create a server class and hand off responsibility to that class.  
    *           Captures any exceptions or errors that may be associated with starting the server
    */
    class Program
    {
      /*
       * METHOD : Main()
       *
       * DESCRIPTION :  The entry point into the program.  Responsible for creating the log file and starting server
       *
       * PARAMETERS : args - command line arguments to give properties to server
       *
       * RETURNS : Nothing
       */
        public static void Main(string[] args)
        {
            if (Logger.CreateLogFile()) //check that the log file was properly created.  End program if it fails
            {
                StringBuilder argsString = new StringBuilder(); // create stringbuilder object to quickly build string from loop
                argsString.Append("Command Line Arguments: "); //start of string
                foreach (string arg in args) //each command line arg
                {
                    argsString.AppendFormat("{0} ", arg); //add it to the string
                }
                Logger.WriteLog("APPLICATION STARTS", argsString.ToString()); //log the start parameters of the program
                if (args.Count() == 3) // if the mandatory number of args is not met
                {
                    Server webServer = null;
                    try
                    {
                        webServer = new Server(args); //create a new instance of Server
                        if (webServer.run == false) //if one of the command line args is invalid or missing
                        {
                            Logger.WriteLog("SERVER START FAILED", "1 Or More Command Line Arguments Are Invalid Or Missing");//write a message to log if server fails
                            Console.WriteLine("SERVER START FAILED 1 Or More Command Line Arguments Are Invalid Or Missing");//write to console to give user feedback as to why program failed to start
                        }
                        else
                        {
                            webServer.createListener(); //start the server's listener
                        }
                       
                    }
                    catch (Exception e) // if something goes wrong with either the listener or the server itself
                    {
                        Console.WriteLine("Server Error: {0}", e.Message); //write to console to give user feedback as to why program failed to start
                        Logger.WriteLog("SERVER START FAILED", e.Message); //write a message to log if server fails
                    }
                }
                else
                {
                    Console.WriteLine("SERVER START FAILED 1 Or More Command Line Arguments Missing");//write to console to give user feedback as to why program failed to start
                    Logger.WriteLog("SERVER START FAILED", "1 Or More Command Line Arguments Missing");//write a message to log if server fails
                }
               
            }
            else //logger failed to create file
            {
                Console.WriteLine("Logger failed to create. Application Terminating"); //show message on console since logger failed to start
            }
        }
     
    }

}


