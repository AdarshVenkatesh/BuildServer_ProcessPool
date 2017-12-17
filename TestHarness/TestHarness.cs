////////////////////////////////////////////////////////////////////////////////////////
// TestHarness.cs : This package is used for executing the library.                   //
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
* This package parses test request received from child build server and loads the library into 
* test harness. It executes the libraries and sends the logs to Repository.
*
* public Interfaces:
* =================
* parse(): Parses test requests received from build server 
* testExecute(): supplies DLLs for execution. 
* logbuilder(): Collects the log information of TestHarness.
* LoadTests(): Loads DLLs and identifies the test driver in it.
* run(): Runs the test driver, hence executing the whole DLL.
* rcvThreadProc(): Continuously monitors the receive queue and assigns tasks to other components.
* based on the message type it have received.
*
* Required Files:
* ===============
* XMLManager.cs,Repository.cs,ITest.cs,TestInterfaces.dll
*
* Maintainance History:
* =====================
* ver 1.0
*
*/



using FederationComm;
using LoadingTests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FederationServers
{
    class MockTestHarness
    {
        Comm comm { get; set; } = null;
        Thread rcvThread = null;
        public string Receivepath { get; set; } = "";
        List<LogData> data = new List<LogData>();
        static ITest testDriver;

        public MockTestHarness()
        {
            comm = new Comm("http://localhost",9090);
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();

        }


        /*----< Returns files by parsing test request >----------------*/

        public List<String> parse(String buildRequest)
        {

            Console.Write("\n \n TestHarness server parses the Test Request");
            List<String> files = new List<string>();
            TestRequest newRequest = buildRequest.FromXml<TestRequest>();
            Receivepath = "../../../TestHarness/" + "DLL_PATH" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            Directory.CreateDirectory(Receivepath);
            string testRequest = Path.GetFullPath(Receivepath) + "/TestRequest" + DateTime.Now.ToFileTime() + ".xml";
            XDocument doc = XDocument.Parse(buildRequest);
            doc.Save(testRequest);
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


        /*----< Activates all test harness components >----------------*/

        public void testExecute(Message msg)
        {
            TestRequest newRequest = msg.body.FromXml<TestRequest>();
            String name = newRequest.author;
            String testLog = "";
            List<TestElement> test = newRequest.tests;
            foreach (TestElement t in test)
            {
                string[] tempFiles = Directory.GetFiles(msg.filePath, t.testDriver);
                tempFiles[0] = Path.GetFullPath(tempFiles[0]);

                string status = LoadTests(tempFiles[0]);
                logbuilder(t, status, name);
            }
            if (test.Count != 0)
            {
                testLog=LogData.log_creator(data, "TestLog");
            }
            Message msg1 = new Message(Message.MessageType.TestLog);
            msg1.fromAddress = msg.toAddress;
            msg1.toAddress = "http://localhost:9070/IService";
            msg1.body = testLog;
            comm.postMessage(msg1);
            data.Clear();
            Console.Write("\n\n **********************End of processing a Test Request***********************************\n ");
        }


        /*----< Logs the TestHarness >----------------*/

        public void logbuilder(TestElement t, string status, string name)
        {
            LogData logData = new LogData();
            logData.author = name;
            logData.testname = t.testName;
            if (status == null) logData.status = "Test Success";
            else { logData.status = "Test Failed"; logData.message = status; }
            data.Add(logData);
        }

        
        //----< load test dlls and identifies test driver >-------------------------------

        public string LoadTests(string file)
        {
            Console.Write("\n\n  Loads and runs : \"{0}\"", file);
            string status = "";
            try
            {
                Assembly assem = Assembly.LoadFrom(file);
                Type[] types = assem.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsClass && typeof(ITest).IsAssignableFrom(t))  // does this type derive from ITest ?
                    {
                        ITest tdr = (ITest)Activator.CreateInstance(t);    // create instance of test driver
                        testDriver = tdr;
                        status = run(testDriver);
                    }
                }
            }

            catch (Exception ex)
            {
                // here, in the real testharness you log the error

                Console.Write("\nError:  {0}", ex.Message);
                return ex.Message;
            }
            return status;
        }

        //----< run the test on library loaded in LoadTests >------------

        public string run(ITest testDriver)
        {
            try
            {
                if (testDriver.test() == true)
                    Console.Write("\n Test status:  Test passed");
                else
                    Console.Write("\n Test status:  Test failed");
            }
            catch (Exception ex)
            {
                Console.Write("\n Error:  {0}", ex.Message);
                return ex.Message;
            }
            return null;
        }


        /*----< Monitors the receive queue and assigns tasks to other components >----------------*/

        void rcvThreadProc()
        {
            Console.Write("\n  starting Test Harness's receive thread");
            while (true)
            {
                Message msg1 = comm.getMessage();
                msg1.show();
                switch(msg1.type)
                {
                    case Message.MessageType.TestRequest:
                        Console.Write("\n Test Harness receives a test request\n");
                        msg1.fileNames = parse(msg1.body);
                        msg1.filePath = Receivepath;
                        msg1.fileName = null;
                        msg1.body = msg1.body;
                        msg1 = swap(msg1);
                        Console.Write("\nTest harness requests child builder for libraries\n");
                        comm.postMessage(msg1);
                        break;
                    case Message.MessageType.TestExecute:
                        Console.Write("\n test harness received libraries from child builder\n");
                        testExecute(msg1);
                        break;
                }

                // pass the Dispatcher's action value to the main thread for execution
            }
        }


        // -----------< Swaps to and from adress of the message to send reply or to communicate further>-------------------

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
#if(TESTHARNESS)
        static void Main(string[] args)
        {
            Console.Write("\n **********This is Test Harness********");
            MockTestHarness th = new MockTestHarness();
            Message msg = new Message(Message.MessageType.connect);
            msg.toAddress = "http://localhost:9090/IService";
            msg.fromAddress = "http://localhost:9090/IService";
            th.comm.postMessage(msg);
        }
#endif
    }
}
