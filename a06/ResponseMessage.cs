/*
* FILE : ResponseMessage.cs
* PROJECT : PROG2001 - Assignment #6
* PROGRAMMER : Chris Lemon
* FIRST VERSION : 2020 - 11 - 16
* REVISED ON : 2020 - 12 - 01
* DESCRIPTION : This file defines the ResponseMessage class.  
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace a06
{
    /*
    * NAME : ResponseMessage
    * PURPOSE : This defines the ResponseMessage class.  It's purpose is to generate a HTTP Response string to send back to the client
    *           It has logic for checking for and reading a file to generate a body and properties that are filled to generate a header
    */
    class ResponseMessage
    {
        //properties
        public const string serverInfo = "Chris' Awesome Web Server"; //server name
        public const string serverType = "HTTP/1.1 "; //first part of header.  Always the same
        public static readonly string[] okCode = { "200 OK", "404 Not Found", "500 Server Error", "415 Unsupported Error Type", "403 Forbidden" }; //Possible error codes
        public string serverCode { get; set; } //error code
        public DateTime sendTime { get; set; } //time the response is sent
        public string contentType { get; set; } // mime type of file
        public string contentLength { get; set; } //length of body
        StringBuilder responseString { get; set; } //place to build response string
        public byte[] header { get; set; } // header as bytes
        public byte[] body { get; set; } //body as bytes
        public bool badContent {get; set;} //flag to see if user has requested an invalid file type
        public byte[] encodedString { get; set; } // full response on byte
        public bool errorFour {get;set;} //marks if 404 has already been called once
        public bool errorFive { get; set; } //marks if 500 has already been called once
       
        /*
        * METHOD : FileRequest()
        *
        * DESCRIPTION :  This method checks that a file exists and copies its contents into a byte array if it exists.
        *                If it doesn't exist, it generates an error response instead
        *
        * PARAMETERS : filePath - the file path of the file to be checked and potentially opened
        *
        * RETURNS : Nothing
        */
        public void FileRequest(string filePath)
        { 
            try
            {
                if (filePath.Contains("?")) //check to see if there is a query string
                {          
                    filePath = filePath.Split('?')[0]; //remove query string from file path
                }
                if(filePath.Contains(".."))//check if the user is trying to access files outside of the root directory
                {
                    serverCode = okCode[4];//set error to forbidden
                    FileRequest("403.html");//change file being opened to 403 error page
                }
                else if (File.Exists(filePath)) //does file exist
                {
                    contentType = MimeMapping.GetMimeMapping(filePath); // get mimemapping value of extension
                    string[] checkType = contentType.Split('/');//split it up to check if the type is text or not
                    if ((checkType[1] =="plain") || (checkType[1] == "jpeg") || (checkType[1] == "html") || (checkType[1] == "gif")) //if it's a supported file type
                    {                        
                        body = File.ReadAllBytes(filePath); //get all the text from file
                        contentLength = body.Count().ToString(); //figure out length of file
                    }
                    else // if it's not a supported file type
                    {
                        badContent = true; //set flag
                    }
                    if (serverCode == null) //if this is the first call to open file
                    {
                        serverCode = okCode[0]; //set response code to 200
                    }                  
                    if (badContent == false) // file cannot be sent as it's an unsupported type
                    {
                        CreateString(); //create a response string
                    }
                   
                }
                else //file not found
                {
                    serverCode = okCode[1];//set error code to 404
                    if (errorFour == true) // error 404 has already been caught once
                    {
                        Exception e = new Exception(); //create exception to trigger 500 error code
                        throw e; //throw exception
                    }
                    else
                    {
                        errorFour = true; //404 has already been accessed once so if it comes around again, the 404 file can't be accessed
                        FileRequest("404.html"); //open 404 page to send                        
                    }
                }
                if (badContent == true) //if a flag has been posted that the user has tried to access an unsupported code
                {
                    serverCode = okCode[3]; //set to error code 415
                    badContent = false; // turn off flag so we can send the error file
                    FileRequest("415.html"); // send the error page to be processed
                }
            }
            catch //something went wrong
            {
                serverCode = okCode[2]; //set error code to 500
                if (errorFive == true) //500 error already called, meaning the 500 page is missing
                {
                    Logger.WriteLog("SERVER ERROR", "Error Pages Missing"); //log to to file
                }
                else
                {
                    errorFive = true; //500 already called once
                    body = null; //set body to null in case this happened during file i/o
                    contentLength = "0"; //set contentlength to zero in case this happened during file i/o
                    FileRequest("500.html"); //open 500 page to send
                }
               
            }
        }
        /*
        * METHOD : CreateString()
        *
        * DESCRIPTION :  This method takes all the properties and creates a response string out of them
        *
        * PARAMETERS : None
        *
        * RETURNS : Nothing
        */
        public void CreateString()
        {
            sendTime = DateTime.Now; // get current date/time
            CreateLogString(); //create a log file
            StringBuilder responseString = new StringBuilder(); //stringbuilder to easily create header
            responseString.AppendFormat("{0}{1}\r\nServer: {2}\r\nDate: {3}\r\nContent-Type: {4}\r\n" +
                "Content-Length: {5}\r\n\r\n", serverType, serverCode, serverInfo, sendTime.ToString("yyyy-MM-dd hh:mm:ss"),contentType, contentLength); //create header
            header = Encoding.ASCII.GetBytes(responseString.ToString()); //encode the header into bytes
            encodedString = new byte[header.Length + body.Length]; //set encodedString to the right size
            Buffer.BlockCopy(header, 0, encodedString, 0, header.Length); //copy header into encodedString
            Buffer.BlockCopy(body, 0, encodedString, header.Length, body.Length); //copy body into encodedString
        }

        /*
        * METHOD : CreateLogString()
        *
        * DESCRIPTION :  This method builds the log string to be inserted into the log file
        *
        * PARAMETERS : None
        *
        * RETURNS : Nothing
        */
        public void CreateLogString()
        {
            StringBuilder logMessage = new StringBuilder();  //string builder to easily build log string
            if (serverCode == "200 OK") //if everything has worked properly
            {
                logMessage.AppendFormat("content-type: {0}, content-length: {1}, server: {2}, date: {3}",
                    contentType, contentLength, serverInfo, sendTime.ToString("yyyy-MM-dd hh:mm:ss")); //create string with full header info
            }
            else //there has been some kind of error
            {
                logMessage.AppendFormat("status: {0}", serverCode.Split(' ')[0]); //create string with just error code
            }
            Logger.WriteLog("RESPONSE", logMessage.ToString()); //write to log file
        }
        
    }
}
