////////////////////////////////////////////////////////////////////////////////////////
// Repository.cs : This package is used for storing files.                            //
// ver 1.0                                                                            //
//                                                                                    //
//Language:     Visual C#                                                             //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-681 SMA Project 4                                                //
// Author      : Adarsh Venkatesh Bodineni,Syracuse University                        //
// Source      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////

/*
* Module Operations:
*===================
* This package is used to store build requests and log files.
* It responds to child builder request for files by sending the required test files.
* It responds to Client requests for displaying the test files, build requests and log files.
*
* public Interfaces:
* =================
* 
* rcvThreadProc(): Continuously monitors the receive queue and assigns tasks to other components.
* based on the message type it have received.
* swap(): Swaps the to and from addresses of the servers.
* save():Repository saves the logs and build requests in its storage
* processGUIRequest(): Repository sends requested files to GUI 
*
* Required Files:
* ===============
* MPCommService.cs,IService.cs
*
* Maintainance History:
* =====================
* ver 1.0
*
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FederationComm;
using System.Threading;
using System.IO;
using System.Xml.Linq;

namespace FederationServers
{
    class Repository
    {
        Comm comm { get; set; } = null;
        Thread rcvThread = null;
        string Storagepath = @"../../../RepositoryFolder";
        public Repository()
        {
            comm = new Comm("http://localhost", 9070);
           Message _msg = new Message(Message.MessageType.BuildRequest);
            _msg.toAddress = "http://localhost:9080/IService";
            _msg.fromAddress = "http://localhost:9070/IService";
            string[] buildRequests = Directory.GetFiles(Path.Combine(Path.GetFullPath(Storagepath), "BuildRequests"), "*BuildRequest*.xml");
            foreach (string bRequest in buildRequests)
            {
                Message m1 = _msg.Clone();
                m1.body = File.ReadAllText(bRequest);
                m1.fileName = Path.GetFileName(bRequest);
                comm.postMessage(m1);
            }
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }


        /*----< Monitors the receive queue and assigns tasks to other components >----------------*/

        void rcvThreadProc()
        {
            Console.Write("\n  starting Repository's receive thread");
            while (true)
            {
                Message msg1 = comm.getMessage();
                msg1.show();
                switch (msg1.type)
                {
                    case Message.MessageType.FileTransferStatus:
                        Console.Write("\n Child builder has requested for build files\n");
                        msg1 = swap(msg1);
                        bool output1 = comm.postFile(msg1);
                        if (output1)
                            Console.WriteLine("\nBuild Files Transfered successfully");
                        break;
                    case Message.MessageType.BuildLog:
                        Save(msg1);
                        break;
                    case Message.MessageType.TestLog:
                        Save(msg1);
                        break;
                    case Message.MessageType.Drivers:
                        processGUIRequest(msg1);
                        break;
                    case Message.MessageType.TestCodes:
                        processGUIRequest(msg1);
                        break;
                    case Message.MessageType.SaveBuildRequest:
                        Save(msg1);
                        break;
                    case Message.MessageType.BuildRequests:
                        processGUIRequest(msg1);
                        break;
                    case Message.MessageType.BuildLogs:
                        processGUIRequest(msg1);
                        break;
                    case Message.MessageType.TestLogs:
                        processGUIRequest(msg1);
                        break;
                }
            }
        }

        /*----< Repository saves the logs and build requests in its storage >----------------*/

        public void Save(Message msg1)
        {
            switch (msg1.type)
            {
                case Message.MessageType.BuildLog:
                    string buildLog = Path.Combine(Path.GetFullPath(Storagepath), "BuildLog") + "/BuildLog" + DateTime.Now.ToFileTime() + ".xml";
                    Console.Write("\nChild builder requested to store build log is:\n" + msg1.body);
                    XDocument doc = XDocument.Parse(msg1.body);
                    Console.Write("\n\nStoring the build log at:" + buildLog + "\n");
                    doc.Save(buildLog);
                    break;
                case Message.MessageType.TestLog:
                    string testLog = Path.Combine(Path.GetFullPath(Storagepath), "TestLog") + "/TestLog" + DateTime.Now.ToFileTime() + ".xml";
                    Console.Write("\nTest harness requested to store test log is:\n" + msg1.body);
                    XDocument doc2 = XDocument.Parse(msg1.body);
                    Console.Write("\n\nStoring the test log at:" + testLog + "\n");
                    doc2.Save(testLog);
                    break;
                case Message.MessageType.SaveBuildRequest:
                    Console.Write("\n Client requested to save build request");
                    string buildRequest = Path.Combine(Path.GetFullPath(Storagepath), "BuildRequests") + "/BuildRequest" + DateTime.Now.ToFileTime() + ".xml";
                    XDocument doc1 = XDocument.Parse(msg1.body);
                    Console.Write("\n saving build request at:" + buildRequest);
                    doc1.Save(buildRequest);
                    break;
            }
        }

        /*----< Repository sends requested files to GUI >----------------*/
        public void processGUIRequest(Message msg1)
        {
            switch (msg1.type)
            {
                case Message.MessageType.Drivers:
                    Console.Write("\n Client requested for drivers");
                    string[] drivers = Directory.GetFiles(Storagepath, "*d*.cs");
                    msg1 = swap(msg1);
                    foreach (string file in drivers)
                        msg1.fileNames.Add(file);
                    Console.Write("\nsending drivers to client\n");
                    comm.postMessage(msg1);
                    break;
                case Message.MessageType.TestCodes:
                    Console.Write("\n Client requested for test codes");
                    string[] testCodes = Directory.GetFiles(Storagepath, "*c*.cs");
                    msg1 = swap(msg1);
                    foreach (string file in testCodes)
                        msg1.fileNames.Add(file);
                    Console.Write("\nsending test codes to client\n");
                    comm.postMessage(msg1);
                    break;
                case Message.MessageType.BuildRequests:
                    Console.Write("\n Client requested for build requests");
                    string[] buildRequests = Directory.GetFiles(Storagepath + "/BuildRequests", "*.xml");
                    msg1 = swap(msg1);
                    foreach (string file in buildRequests)
                        msg1.fileNames.Add(file);
                    Console.Write("\nsending build requests to client\n");
                    comm.postMessage(msg1);
                    break;
                case Message.MessageType.BuildLogs:
                    Console.Write("\n Client requested for build logs");
                    string[] buildLogs = Directory.GetFiles(Storagepath + "/BuildLog", "*.xml");
                    msg1 = swap(msg1);
                    foreach (string file in buildLogs)
                        msg1.fileNames.Add(file);
                    Console.Write("\nsending build logs to client\n");
                    comm.postMessage(msg1);
                    break;
                case Message.MessageType.TestLogs:
                    Console.Write("\n Client requested for test logs");
                    string[] testLogs = Directory.GetFiles(Storagepath + "/TestLog", "*.xml");
                    msg1 = swap(msg1);
                    foreach (string file in testLogs)
                        msg1.fileNames.Add(file);
                    Console.Write("\nsending test logs to client\n");
                    comm.postMessage(msg1);
                    break;
            }
            }


        //----------< Swaps the toAddress and fromAddress content of Message>-----------------------

        Message swap(Message msg)
        {
            string temp = msg.toAddress;
            msg.toAddress = msg.fromAddress;
            msg.fromAddress = temp;
            return msg;
        }
#if(REPOSITORY)
        static void Main(string[] args)
        {
            Console.Write("****** This is Repository server*********\n");
            Repository rs = new Repository();
            Message msg = new Message(Message.MessageType.connect);
            msg.toAddress = "http://localhost:9070/IService";
            msg.fromAddress = "http://localhost:9070/IService";
            rs.comm.postMessage(msg);
        }
#endif
    }
}
