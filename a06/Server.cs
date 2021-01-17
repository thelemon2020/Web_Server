/*
* FILE : Server.cs
* PROJECT : PROG2001 - Assignment #6
* PROGRAMMER : Chris Lemon
* FIRST VERSION : 2020 - 11 - 16
* REVISED ON : 2020 - 12 - 01
* DESCRIPTION : This file defines the Server class.  
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace a06
{
   /*
    * NAME : Server
    * PURPOSE : This defines the Server class.  It holds the main logic for the program, which takes command line arguments
    *           parses them appropriatetly and then starts a TCP listener to loop and connect with and handle clients.  
    *           It ends the loop with a response to the server, before looping back for another connection
    */
    class Server
    {
        //properties
        public string webRoot { get; set; } //< the start directory of the server
        public IPAddress webIP { get; set; } //< the IP address of the server
        public int webPort { get; set; } //< the port of the server
        public bool run { get; set; } //< should the server start and continue to listen for connections

        /*
         * METHOD : Server()
         *
         * DESCRIPTION :  The constructor for the server class.  Sets run to be true and calls a method to 
         *                fill some of its properties
         *
         * PARAMETERS : args - a string array holding command line arguments
         *
         * RETURNS : Nothing
         */
        public Server(string[] args)
        {
            run = true;  //set so listener loop will run until shut down
            getCommandLineArgs(args); // parse the command args into properties
            if ((webIP == null) || (webRoot == null) || (webIP == null)) //if one of the command line args is invalid
            {
                run = false;//don't run server
            }
        }
       /*
        * METHOD : getCommandLineArgs()
        *
        * DESCRIPTION :  This method takes in the string array that holds the command line args and parses them into 
        *                their appropriate properties
        *
        * PARAMETERS : toParse - an array of strings that are taken from the command line
        *
        * RETURNS : Nothing
        */
        public void getCommandLineArgs(string[] toParse)
        {
            foreach (string argument in toParse) //iterate through each element of toParse
            {
                if (argument.Contains("webRoot")) //check if this element is the webRoot
                {
                    string[] splitTemp = argument.Split('='); //break up string grab appropriate information
                    webRoot = splitTemp[1]; //remove extra bits
                }
                else if (argument.Contains("webIP"))//check if this element is the webIP
                {
                    string[] splitTemp = argument.Split('=');//break up string grab appropriate information
                    string tempIP = splitTemp[1]; //remove extra bits
                    IPAddress temp2; // create extra temp to parse out ip into
                    IPAddress.TryParse(tempIP, out temp2);  //parse out the ip
                    webIP = temp2; //set property
                }
                else if (argument.Contains("webPort")) //check if this element is the webPort
                {
                    string[] splitTemp = argument.Split('=');//break up string grab appropriate information
                    string tempPort = splitTemp[1];//remove extra bits
                    int tempIntPort; // set a temporary variable to parse the int out into
                    int.TryParse(tempPort, out tempIntPort);//parse out int
                    webPort = tempIntPort; // set property
                }
            }
        }
        /*
         * METHOD : createListener()
         *
         * DESCRIPTION : This method creates a listener and returns it.  Probably not neccessary, but makes it more organized
         * 
         * PARAMETERS : None
         *
         * RETURNS : listener - the created TcpListener
         */
        public void createListener()
        {
            TcpListener listener = new TcpListener(webIP, webPort); //create listener
            listenForClients(listener); //return listener
        }
        /*
        * METHOD : listenForClients()
        *
        * DESCRIPTION : This method starts the listener and loops, waiting for a connection.  
        *               if it gets one, it passes the connection to another method for processing
        * 
        * PARAMETERS : None
        *
        * RETURNS : void
        */
        public void listenForClients(TcpListener listener)
        {
            listener.Start();//start listener
            while (run == true) //run until it's time for the server to shutdown.  in this case, never
            {
                TcpClient clientConnection = listener.AcceptTcpClient(); //create client if connection is made
                HandleConnection(clientConnection); //process connection 
                clientConnection.Close();//close connection
            }
            listener.Stop();//stop listener when it's time to shut down server
        }
        /*
         * METHOD : HandleConnection()
         *
         * DESCRIPTION : Holds the calls to other methods to properly complete a request/response http process
         * 
         * PARAMETERS : client - the TcpClient connection to send/receive to/from
         *
         * RETURNS : void
         */
        public void HandleConnection(TcpClient client)
        {
            NetworkStream toClient = client.GetStream(); //open stream with client
            string request = receiveFromClient(toClient);//get the request string
            ResponseMessage msg = parseRequest(request);//parse the string
            sendToClient(toClient, msg);//send a response back
        }

       /*
        * METHOD : receiveFromClients()
        *
        * DESCRIPTION : This method reads and stores the http stream from the client
        * 
        * PARAMETERS : client - the connected client
        *
        * RETURNS : userRequest - the request string provided by the connected client
        */
        public string receiveFromClient(NetworkStream client)
        {
            byte[] incomingData = new byte[1024]; //create array to receive incoming data
            int bytesRec = 0; //variable to get the length of the data being received
            string messageAsString = ""; //string to hold the ascii conversion of the data
            try
            {
                bytesRec = client.Read(incomingData, 0, incomingData.Length); // read from the stream
                messageAsString += Encoding.ASCII.GetString(incomingData, 0, bytesRec); // convert to string               
            }
            catch(Exception e)
            {
                Logger.WriteLog("FAILED TO READ FROM CLIENT", e.Message); //log a failed to read message
            }
            return messageAsString; //return the ascii encoded string
        }
       /*
        * METHOD : parseRequest()
        *
        * DESCRIPTION : This method takes the request string and parses it out, then decides what to do with it
        * 
        * PARAMETERS : request - the request string to be parsed
        *
        * RETURNS : void
        */
        public ResponseMessage parseRequest(string request)
        {
            string[] requestArray = request.Split('\n'); //split up the request elements
            string[] getMethod = requestArray[0].Split(' '); //split up the line holding method and filepath
            string logMessage = "";
            if ((getMethod.Length>1) && (!getMethod[1].Contains("HTTP"))) //if it appears that the request was properly formatted
            {
                logMessage = getMethod[0] + " " + getMethod[1]; //create string that shows the method and filepath
            }
            else //if it looks like it wasn't formatted properly
            {
                logMessage = getMethod[0]; //create string to show just method
            }
            Logger.WriteLog("REQUEST", logMessage); //write log              
            ResponseMessage toSend = null;
            bool isValid = isHTTPValid(requestArray); //check that request was properly formatted
            if (isValid == true) //if the request was properly formatted
            {
                 if (webRoot==null) //if webroot was missing from command line args
                 {
                    toSend = new ResponseMessage(); //creat new instance of ResponseMessage
                    toSend.serverCode = "500 Server Error"; // create error code 500 because the server is not set up properly - could have been a 404 error, but this seemed more appropriate
                    toSend.FileRequest("500.html"); //try to open 500 error page
                 }
                 else if (getMethod[0] == "GET") //if method is "GET"
                 {
                     string filePath = webRoot + getMethod[1]; //create filepath string
                     toSend = new ResponseMessage(); //create new instance of ResponseMessage
                     toSend.FileRequest(filePath); //try to open file requested by client
                 }
                 else //if method is "POST"
                 {
                     toSend = new ResponseMessage();  // create new instance of ResponseMessage
                     toSend.serverCode = "405 METHOD NOT ALLOWED"; // Create an error code because "POST" is not supported
                     toSend.FileRequest("405.html"); //try to open 405 error page
                }
            }
            else
            {
                 toSend = new ResponseMessage();  // create new instance of ResponseMessage
                 toSend.serverCode = "400 BAD REQUEST"; // Create an error code because the Request was not formatted properly
                 toSend.FileRequest("400.html"); //try to open 400 error page
            }    
            return toSend; //return response message
        }

        /*
        * METHOD : sendToClient()
        *
        * DESCRIPTION : This method sends a response string back to the client
        * 
        * PARAMETERS : client - the stream to write to
        *              msg - the message class that holds the byte array to send to the client
        *
        * RETURNS : void
        */
        public void sendToClient(NetworkStream client, ResponseMessage msg)
        {
            try
            {
                client.Write(msg.encodedString, 0, msg.encodedString.Length); //send to client
            }
            catch(Exception e)
            {
                Logger.WriteLog("FAILED TO SEND TO CLIENT", e.Message); //log if something fails with the send
            }
           
        }
        
        /*
        * METHOD : isHTTPValid()
        *
        * DESCRIPTION : This method uses regex to check the validity of the HTTP header received from the client
        * 
        * PARAMETERS : HttpRequest - the string array of the HTTP request
        *
        * RETURNS : true - if request is in valid format
        *           false - if request is not in valid format
        */
        public bool isHTTPValid(string[] HttpRequest)
        {
            Regex line1 = new Regex("^\\b(?i)(POST|GET)\\b\\s[^\\s]+\\s\\b(HTTP/1.1)\\b\r?$");//set regex to make sure the user includes the method, a requested resource and the http info
            Regex hostInfo = new Regex("^\\b(?i)(host)\\b[:]\\s[^\\s]+\r?$"); //make sure that header has the host information
            if (!line1.IsMatch(HttpRequest[0])) //check that line 1 of header is correct
            {
                return false; //return false if not
            }
            int i = 0;
            foreach (string entry in HttpRequest) //since we can't guarantee the host info will be in the second line (thansk IE) we must check all the lines
            {
                if (hostInfo.IsMatch(entry)) //check that host information is somewhere in http request
                {
                    break; //break out of loop if it is
                }
                i++; // 
            }
            if (i >= HttpRequest.Length -1)
            {
                return false;
            }
            if (HttpRequest[0].Split(' ')[0].Contains("POST")) //if header is not standard GET method request
            {
                Regex line3 = new Regex("^\\b(?i)(Content-Type)\\b[:]\\s[^\\s]+\r?$"); //set regex to check that there is a content type listed
                Regex line4 = new Regex("^\\b(?i)(Content-Length)\\b[:]\\s[0-9]+\r?$"); // set regex to make sure a length is provided
                List<bool> isMatch = new List<bool>();
                bool isHeader = true;
                string getLength = "";
                int contentLength = 0;
                foreach (string headerEntry in HttpRequest) //check that the rest of the header contains at least the two other mandatory lines, content-type and content-length
                {
                    if (isHeader==true) //if the rest of the lines are part of the header
                    {
                        if (line3.IsMatch(headerEntry)) // check if Content-Type header is present and correct
                        {
                            isMatch.Add(true); //set true flag
                        }
                        if (line4.IsMatch(headerEntry)) // check if Content-Length header is present and correct
                        {
                            int.TryParse(headerEntry.Split(' ')[1],out contentLength);
                            isMatch.Add(true); //set true flag
                        }
                        if ((headerEntry == "\r") || (headerEntry == "")) //if end of header
                        {
                            isHeader=false; // get out of loop
                        }
                    }
                    else
                    {
                        getLength += headerEntry;
                    }                   
                }
                if (getLength.Length == contentLength) //if the length of the content matches length of the body
                {
                    isMatch.Add(true); //add final true
                }
                if (isMatch.Count != 3) // header did not have two mandatory fields or content length did not match body length
                {
                    return false;  //return false to show request is invalid
                }
            }
            return true; //return true if header has passed all tests
        }
    }
}
