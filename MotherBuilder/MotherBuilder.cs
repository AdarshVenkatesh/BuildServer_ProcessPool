////////////////////////////////////////////////////////////////////////////////////////
// MotherBuilder.cs : This package spawns the child builders.                         //
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
* This package spawns the number of child builder processes.
* It maintains two blocking queues for build requests and ready messages.
*
* public Interfaces:
* =================
* processMessage(): Invoked if the message type is spawn process to help in spawning of child builders.
* rcvThreadProc(): Continuously monitors the receive queue and assigns tasks to other components.
* checkQueues(): Monitors request queue and ready queue and sends request to child builder if both are not empty.
* spawnProcessPool(): spawns number of child processes.
*
* Required Files:
* ===============
* MPCommService.cs,IService.cs,BlockingQueue.cs
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
using System.Diagnostics;
using System.IO;
using SWTools;

namespace FederationServers

{
    class MotherBuilder
    {
        static int numberOfProcesses = 0;
        Comm comm { get; set; } = null;
        Thread rcvThread = null;
        static BlockingQueue<Message> readyQueue;
        static BlockingQueue<Message> buildRequestQueue;

        public MotherBuilder()
        {
            comm = new Comm("http://localhost", 9080);
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();

        }


        //------< helps in spawning number of child builders >----------
        
        public void processMessage(Message msg)
        {
            spawnProcessPool(msg.numberProcess);
            return;
        }


        /*------< Monitors the receive queue of mother builder >----------------*/

        void rcvThreadProc()
        {
            Console.Write("\n  starting Mother builder's receive thread");
            while (true)
            {
                if (checkQueues()) { Console.Write("\nBoth queues are not empty so \n Successfully sent build request to child builder\n"); }
                Message msg1 = comm.getMessage();
                msg1.show();
                switch(msg1.type)
                {
                case Message.MessageType.SpawnProcess:
                        processMessage(msg1);
                        break;
                case Message.MessageType.BuildRequest:
                        buildRequestQueue.enQ(msg1);
                        break;
                case Message.MessageType.Ready:
                        readyQueue.enQ(msg1);
                        break;
                }
            Console.WriteLine("\nReady queue  :  {0}", readyQueue.size());
            Console.WriteLine("\nRequest queue :  {0}", buildRequestQueue.size());
            if (msg1.body == "quit")
            {
                Console.WriteLine("shut down host by client request");
                Console.WriteLine("size of ready queue\t:\t{0}", readyQueue.size());
                Console.WriteLine("size of request queue\t:\t{0}", buildRequestQueue.size());
                break;
            }
        }
    }

        //------< Check if any messages dropped into the ready queue and request queue >----------

        public bool checkQueues()
        {
            if (readyQueue.size() > 0 && buildRequestQueue.size() > 0)
            {
                Message mready = readyQueue.deQ();
                Message mrequest = buildRequestQueue.deQ();
                mrequest.toAddress = mready.fromAddress;
                comm.postMessage(mrequest);
                return true;
            }
            return false;
        }


        //------< spawns number of child builders >----------

        void spawnProcessPool(int numProcess)
        {
            for (int i = 0; i < numProcess; i++)
            {
                if (numberOfProcesses < 9)
                {
                    Process p2 = new Process();
                    string fileName = "../../../ChildBuildServer/bin/debug/ChildBuildServer.exe";
                    Process.Start(Path.GetFullPath(fileName), "908" + ((numberOfProcesses + 1).ToString()));
                    numberOfProcesses++;
                }
            }
            return;
        }
#if(MOTHERBUILDER)
        static void Main(string[] args)
        {
            Console.Write("****** This is Mother Builder*********\n");
            MotherBuilder rs = new MotherBuilder();
            readyQueue = new BlockingQueue<Message>();
            buildRequestQueue = new BlockingQueue<Message>();
            Message msg = new Message(Message.MessageType.connect);
            msg.toAddress = "http://localhost:9080/IService";
            msg.fromAddress = "http://localhost:9080/IService";
            rs.comm.postMessage(msg);

            Message msg3 = msg.Clone();
            msg3.type = Message.MessageType.SpawnProcess;
            msg3.numberProcess = 2;
            rs.comm.postMessage(msg3);
        }
#endif
    }
}
