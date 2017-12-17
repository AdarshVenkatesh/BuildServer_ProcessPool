////////////////////////////////////////////////////////////////////////////////////////
// ChildBuilder.cs : This package is the child build server controller.               //
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
* This package parses the test request and requests the repo for the files
* to a temporary directory created with the name of author and timestamp.
* Requests BuildLibrary.cs for building test files into dll.
* Forms log data for each test request.
* Creates test request for test harness.
*
* public Interfaces:
* =================
* parse(): Parses the build request received from mother builder and returns the file list in the build request.
* builder(): Requests the BuildLibrary.cs to build the files into library.
* logbuilder(): Collects log information of each test.
* create_testrequest(): Creates a test request for test harness.
* rcvThreadProc(): Continuously monitors the receive queue and assigns tasks to other components.
* based on the message type it have received.
* swap(): Swaps the to and from addresses of the servers.
*
* Required Files:
* ===============
* XMLManager.cs,serialization.cs,MPCommService.cs,IService.cs
*
* Maintainance History:
* =====================
* ver 1.0
*
*/


using FederationComm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FederationServers
{
    class ChildBuilder
    {
        Comm comm { get; set; } = null;
        Thread rcvThread = null;
        public Message readyAgain { get; set; }
        public string Receivepath { get; set; } = "";
        public string dllPath { get; set; } = "";
        Builder builder1 = new Builder();
        public string name { get; set; }
        List<String> filesToBuild = new List<string>();
        List<LogData> data = new List<LogData>();

        public ChildBuilder(int port)
        {
            comm = new Comm("http://localhost", port);
            
        }


        /*----< Returns list of files by parsing build request >----------------*/

        public List<String> parse(String buildRequest)
        {
            Console.Write("\n \n Child Build parses the Build Request");
            List<String> files = new List<string>();
            TestRequest newRequest = buildRequest.FromXml<TestRequest>();
             name = newRequest.author;
             Receivepath = "../../../ChildBuildServer/" + name + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            Console.Write("\n\nTemporary directory created :" + Receivepath+"\n");
            
             Directory.CreateDirectory(Receivepath);
            List<TestElement> test = newRequest.tests;
            foreach (TestElement t in test)
            {
                files.Add(t.testDriver);
                List<String> codes = t.testCodes;
                foreach (String c in codes)
                {
                    files.Add(c);
                }
            }
            return files;
        }


        /*----< Activates all build server components >----------------*/

        public void builder(Message msg)
        {
            TestRequest newRequest = msg.body.FromXml<TestRequest>();
            List<TestElement> test = newRequest.tests;

            foreach (TestElement t in test)
            {
                filesToBuild.Add(t.testDriver);
                List<String> codes = t.testCodes;
                foreach (String c in codes)
                {
                    filesToBuild.Add(c);
                }
                String status = builder1.libraryBuilder(filesToBuild, Receivepath);
                filesToBuild.Clear();
                logbuilder(t, status, name);
            }

            String log = LogData.log_creator(data, "BuildLog");
            String temp = msg.toAddress;
            msg.toAddress = msg.fromAddress;
            msg.fromAddress = temp;
            msg.body = log;
            msg.type = Message.MessageType.BuildLog;
            comm.postMessage(msg);
            String testRequest = create_testrequest(data, Receivepath, name);
            if (!testRequest.Equals(""))
            {
                Message msg1 = new Message(Message.MessageType.TestRequest);
                msg1.fromAddress = msg.fromAddress;
                msg1.toAddress = "http://localhost:9090/IService";
                msg1.body = testRequest;
                comm.postMessage(msg1);
            }
            else
            {
                Console.Write("\n\n\n Sending Ready Message to Mother builder \n");
                comm.postMessage(readyAgain);
            }
            data.Clear();

        }


        /*----< Logs the Child BuildServer >----------------*/

        public void logbuilder(TestElement t, string status, string name)
        {
            LogData logData = new LogData();
            logData.author = name;
            logData.testname = t.testName;
            logData.testDriver = t.testDriver;
            if (status == "") logData.status = "Build Success";
            else { logData.status = "Build Failed"; logData.message = status; }
            data.Add(logData);
        }


        /*----< Returns a test request for test harness >----------------*/

        public string create_testrequest(List<LogData> test, string path, string name)
        {
            Console.Write("\n \nConstructing Test Request for TestHarness");
            List<TestElement> te = new List<TestElement>();
            foreach (LogData test1 in test)
            {
                if (test1.status.Equals("Build Success"))
                {
                    TestElement te1 = new TestElement();
                    te1.testName = test1.testname;
                    te1.addDriver(Path.GetFileNameWithoutExtension(test1.testDriver) + ".dll");
                    te.Add(te1);
                }
            }
            TestRequest tr = new TestRequest();
            tr.author = name;
            for (int i = 0; i < te.Count; i++)
                tr.tests.Add(te[i]);
            string trXml;
            if (tr.tests.Count >= 1)
            {
                trXml = tr.ToXml();
                File.WriteAllText(path + "/TestRequesttoHarness.xml", trXml);
                Console.Write("\n \nTestRequest for the TestHarness:\n {0}\n", trXml);
                return trXml;
            }
            else
            {
                Console.Write("\n *******Build failed so stopping further execution******** ");
                return "";
            }
        }


        /*----< Monitors the receive queue and assigns tasks to other components >----------------*/

        void rcvThreadProc()
        {
            Console.Write("\n  starting child builder's receive thread");
            while (true)
            {
                Message msg = comm.getMessage();
                msg.show();
                switch (msg.type)
                {
                    case Message.MessageType.BuildRequest:
                        Console.Write("\n The received build request is:\n"+msg.body);
                        msg.fileNames = parse(msg.body);
                        msg.filePath = Receivepath;
                        msg.fileName = null;
                        msg.body = msg.body;
                        msg = swap(msg);
                        msg.toAddress = "http://localhost:9070/IService";
                        msg.type = Message.MessageType.FileTransferStatus;
                        Console.Write("\n\n Requesting repo for files\n");
                        comm.postMessage(msg);
                        break;
                    case Message.MessageType.BuildDLL:
                        Console.Write("\n Received test files from repo");
                        builder(msg);
                        break;
                    case Message.MessageType.TestRequest:
                        Console.Write("\n\nTest Harness Requested for dll files\n");
                        msg = swap(msg);
                        msg.dllPath = Receivepath;
                        bool output1 = comm.postFile(msg);
                        if (output1)
                            Console.WriteLine("\n DLL Files Transfered successfully to Test Harness");
                        Console.Write("\n\n\n Sending Ready Message to Mother builder \n");
                        comm.postMessage(readyAgain);
                        break;
                }
                if (msg.body == "quit")
                {
                    Console.WriteLine("shut down host by client request");
                    break;
                }
                Thread.Sleep(1000);
            }
        }


        // ----------< Swaps to and from adress of the message to send reply or to communicate further>-------------------

        Message swap(Message msg)
        {
            if (msg.fromAddress != null)
            {
                String temp = msg.toAddress;
                msg.toAddress = msg.fromAddress;
                msg.fromAddress = temp;
            }
            return msg;
        }

#if(CHILDBUILDSERVER)
        static void Main(string[] args)
        {
            ChildBuilder rs = new ChildBuilder(Int32.Parse(args[0]));
            Message msg = new Message(Message.MessageType.connect);
            msg.toAddress = "http://localhost:"+args[0]+"/IService";
            msg.fromAddress = "http://localhost:"+args[0]+"/IService";
            rs.comm.postMessage(msg);

            Message msg1 = new Message(Message.MessageType.Ready);
            msg1.body = "ready";
            msg1.toAddress = "http://localhost:9080/IService";
            msg1.fromAddress = "http://localhost:"+args[0]  + "/IService";
            Console.Write("\n\n Sending Ready Message to Mother builder \n");
            rs.comm.postMessage(msg1);
            rs.readyAgain = msg1;
            rs.rcvThread = new Thread(rs.rcvThreadProc);
            rs.rcvThread.Start();


        }
#endif
    }
}
