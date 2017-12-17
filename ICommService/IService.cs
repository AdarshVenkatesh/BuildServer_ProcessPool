////////////////////////////////////////////////////////////////////////////////////////
// IService.cs : This package is interface for Communication.                         //
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
* This package is the interface for the client. 
*
* public Interfaces:
* =================
* ICommunication : Interface used for message passing and file transfer.
*
* Required Files:
* ===============
* MPCommService.cs
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
using System.ServiceModel;
using System.Runtime.Serialization;

namespace FederationComm
{
    [ServiceContract(Namespace = "FederationComm")]
    public interface IService
    {
        /*----< support for message passing >--------------------------*/

        [OperationContract(IsOneWay = true)]
        void postMessage(Message msg);

        // private to receiver so not an OperationContract
        Message getMessage();

        /*----< support for sending file in blocks >-------------------*/

        [OperationContract]
        bool openFileForWrite(string name,string receivePath);
        [OperationContract]
        bool writeFileBlock(byte[] block);
        [OperationContract]
        void closeFile();
    }
    [DataContract]
    public class Message
    {
        public enum MessageType
        {
            [EnumMember]    //To the connectivity between the client and the host
            connect,
            [EnumMember]    //Repository sends to the BuildServer
            BuildRequest,
            [EnumMember]    //Buildier sends to the TestHarness
            TestRequest,
            [EnumMember]    //To place the Builders into ready queue maintained at build server
            Ready,
            [EnumMember]    //Builders sends to intitate connection to repo for writing logs
            BuildLog,
            [EnumMember]    //TestHarness writes to the Repository in the directory path specified
            TestLog,
            [EnumMember]    //Request for the Build Request file
            Request,
            [EnumMember]    //Child builder request for files from repo
            FileTransferStatus,
            [EnumMember]        //to say that building process can be started.
            BuildDLL,
            [EnumMember]       //client requests for test drivers from repo
            Drivers,
            [EnumMember]       //client requests for test codes from repo
            TestCodes,
            [EnumMember]      //client requests repo to save newly generated build request
            SaveBuildRequest,
            [EnumMember]       //client requests repo for build requests
            BuildRequests,
            [EnumMember]       //client requests repo for build logs
            BuildLogs,
            [EnumMember]       //child builder asks test harness to execute after sending libraries
            TestExecute,
            [EnumMember]        //client requests repo for test logs
            TestLogs,
            [EnumMember]
            closeReceiver,
            [EnumMember]
            closeSender,
            [EnumMember]         //mother builder spawns number of child builders
            SpawnProcess,
            [EnumMember]
            quit
        }
        public Message(MessageType mt)
        {
            type = mt;
        }
        /*----< data members - all serializable public properties >----*/
        [DataMember]
        public string toAddress { get; set; }

        [DataMember]
        public string fromAddress { get; set; }

        [DataMember]
        public MessageType type { get; set; } = MessageType.connect;

        [DataMember]
        public string fileName { get; set; }

        [DataMember]
        public string filePath { get; set; }

        [DataMember]
        public string command { get; set; }

        [DataMember]
        public string dllPath { get; set; }

        [DataMember]
        public string body { get; set; }

        [DataMember]
        public List<string> fileNames { get; set; } = new List<string>();

        [DataMember]
        public int numberProcess { get; set; }

        [DataMember]
        public string errorMsg { get; set; } = "no error";
        //-< Displays the message transferred between the sender(client/proxy) and reciever(host) >-----------
        public void show()
        {
            Console.Write("\n  MessageRecieved:");
            Console.Write("\n    MessageType : {0}", type.ToString());
            if (toAddress != null)
                Console.Write("\n    to          : {0}", toAddress);
            if (fromAddress != null)
                Console.Write("\n    from        : {0}", fromAddress);
            if (fileName != null)
                Console.Write("\n    fileName   :  {0}", fileName);
            if (fileNames != null)
            {
                Console.Write("\n    List of Files   :");


                if (fileNames.Count > 0)
                    Console.Write("\n      ");
                foreach (string arg in fileNames)
                    Console.Write("{0} ", arg);
            }
            Console.Write("\n    errorMsg    : {0}\n", errorMsg);

        }

        /*----< clones the attributes of a message >--------------------------*/
        public Message Clone()
        {
            Message msg = new Message(Message.MessageType.connect);
            msg.type = type;
            msg.toAddress = toAddress;
            msg.fromAddress = fromAddress;
            msg.fileName = fileName;
            msg.command = command;
            return msg;
        }

        public static void Main()
        {
            Console.WriteLine("Hello World");
        }
    }

}
