////////////////////////////////////////////////////////////////////////////////////////
// MPCommService.cs: service for message passing communication.                       //
// ver 1.0                                                                            //
//                                                                                    //
//Language:     Visual C#                                                             //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-681 SMA Project 4                                                //
// Author      : Adarsh Venkatesh Bodineni,Syracuse University                        //
// Source      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////

/*   
 * Package Operations:
 * -------------------
 * This package defines three classes:
 * - Sender which implements the public methods:
 *   -------------------------------------------
 *   - connect          : opens channel and attempts to connect to an endpoint, 
 *                        trying multiple times to send a connect message
 *   - close            : closes channel
 *   - postMessage      : posts to an internal thread-safe blocking queue, which
 *                        a sendThread then dequeues msg, inspects for destination,
 *                        and calls connect(address, port)
 *   - postFile         : attempts to upload a file in blocks
 *   - getLastError     : returns exception messages on method failure
 * - Receiver which implements the public methods:
 *   ---------------------------------------------
 *   - start            : creates instance of ServiceHost which services incoming messages
 *   - postMessage      : Sender proxies call this message to enqueue for processing
 *   - getMessage       : called by Receiver application to retrieve incoming messages
 *   - close            : closes ServiceHost
 *   - openFileForWrite : opens a file for storing incoming file blocks
 *   - writeFileBlock   : writes an incoming file block to storage
 *   - closeFile        : closes newly uploaded file
 * - Comm which implements, using Sender and Receiver instances, the public methods:
 *   -------------------------------------------------------------------------------
 *   - postMessage      : send CommMessage instance to a Receiver instance
 *   - getMessage       : retrieves a CommMessage from a Sender instance
 *   - postFile         : called by a Sender instance to transfer a file
 * 
 *
 * Required Files:
 * ---------------
 * IMPCommService.cs, MPCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 
 * - first release
 * 
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FederationComm;
using System.ServiceModel;
using System.Threading;
using System.IO;

namespace FederationServers
{
    public class Receiver : IService
    {
        public static SWTools.BlockingQueue<Message> rcvQ { get; set; } = null;
        public bool restartFailed { get; set; } = false;
        ServiceHost commHost = null;
        FileStream fs = null;
        string lastError = "";
       

        /*----< constructor >------------------------------------------*/

        public Receiver()
        {
            if (rcvQ == null)
                rcvQ = new SWTools.BlockingQueue<Message>();
        }
        /*----< create ServiceHost listening on specified endpoint >---*/
        /*
         * baseAddress is of the form: http://IPaddress or http://networkName
         */
        public void start(string baseAddress, int port)
        {
            string address = baseAddress + ":" + port.ToString() + "/IService";
            createCommHost(address);
        }
        /*----< create ServiceHost listening on specified endpoint >---*/
        /*
         * address is of the form: http://IPaddress:8080/IMessagePassingComm
         */
        public void createCommHost(string address)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            commHost = new ServiceHost(typeof(Receiver), baseAddress);
            commHost.AddServiceEndpoint(typeof(IService), binding, baseAddress);
            commHost.Open();
        }
        /*----< enqueue a message for transmission to a Receiver >-----*/

        public void postMessage(Message msg)
        {
            rcvQ.enQ(msg);
        }
        /*----< retrieve a message sent by a Sender instance >---------*/

        public Message getMessage()
        {
            Message msg = rcvQ.deQ();
            if (msg.type == Message.MessageType.closeReceiver)
            {
                close();
            }
            return msg;
        }
        /*----< close ServiceHost >------------------------------------*/

        public void close()
        {
            Console.Write("\n  closing receiver - please wait");
            commHost.Close();
            Console.Write("\n  commHost.Close() returned");
        }
        /*---< called by Sender's proxy to open file on Receiver >-----*/

        public bool openFileForWrite(string name,string receivePath)
        {
            try
            {
                string filename = Path.Combine(receivePath, name);
                //  string writePath = Path.Combine(ServerEnvironment.root, name);
                fs = File.OpenWrite(filename);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< write a block received from Sender instance >----------*/

        public bool writeFileBlock(byte[] block)
        {
            try
            {
                fs.Write(block, 0, block.Length);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< close Receiver's uploaded file >-----------------------*/

        public void closeFile()
        {
            fs.Close();
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Sender class - sends messages and files to Receiver

    public class Sender
    {
        private IService channel;
        private ChannelFactory<IService> factory = null;
        private SWTools.BlockingQueue<Message> sndQ = null;
        private int port = 0;
        private string fromAddress = "";
        private string toAddress = "";
        Thread sndThread = null;
        int tryCount = 0, maxCount = 10;
        string lastError = "";
        string lastUrl = "";
        String path = "";
        string Storagepath = @"../../../RepositoryFolder";

        /*----< constructor >------------------------------------------*/

        public Sender(string baseAddress, int listenPort)
        {
            port = listenPort;
            fromAddress = baseAddress + listenPort.ToString() + "/IService";
            sndQ = new SWTools.BlockingQueue<Message>();
            sndThread = new Thread(threadProc);
            sndThread.Start();
        }
        /*----< creates proxy with interface of remote instance >------*/

        public void createSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            factory = new ChannelFactory<IService>(binding, address);
            channel = factory.CreateChannel();
        }
        /*----< attempts to connect to Receiver instance >-------------*/

        public bool connect(string baseAddress, int port)
        {
            toAddress = baseAddress + ":" + port.ToString() + "/IService";
            return connect(toAddress);
        }
        /*----< attempts to connect to Receiver instance >-------------*/
        /*
         * - attempts a finite number of times to connect to a Receiver
         * - first attempt to send will throw exception of no listener
         *   at the specified endpoint
         * - to test that we attempt to send a connect message
         */
        public bool connect(string toAddress)
        {
            int timeToSleep = 500;
            createSendChannel(toAddress);
            Message connectMsg = new Message(Message.MessageType.connect);
            while (true)
            {
                try
                {
                    channel.postMessage(connectMsg);
                    tryCount = 0;
                    return true;
                }
                catch (Exception ex)
                {
                    if (++tryCount < maxCount)
                    {
                        Thread.Sleep(timeToSleep);
                    }
                    else
                    {
                        lastError = ex.Message;
                        return false;
                    }
                }
            }
        }
        /*----< closes Sender's proxy >--------------------------------*/

        public void close()
        {
            if (factory != null)
                factory.Close();
        }
        /*----< processing for send thread >--------------------------*/
        /*
         * - send thread dequeues send message and posts to channel proxy
         * - thread inspects message and routes to appropriate specified endpoint
         */
        void threadProc()
        {
            while (true)
            {
                Message msg = sndQ.deQ();
                if (msg.type == Message.MessageType.closeSender)
                {
                   
                    break;
                }
                else
                {
                    close();
                    if (!connect(msg.toAddress))
                        return;
                    lastUrl = msg.toAddress;
                    channel.postMessage(msg);
                }
            }
        }
        /*----< main thread enqueues message for sending >-------------*/

        public void postMessage(Message msg)
        {
            sndQ.enQ(msg);
        }
        /*----< uploads file to Receiver instance >--------------------*/

        public bool postFile(Message msg)
        {
            FileStream fs = null;
            long bytesRemaining;
            try
            {
                close();
                connect(msg.toAddress);
                foreach (string fileName in msg.fileNames)
                {
                    if (msg.dllPath != null) path = Path.Combine(msg.dllPath, fileName);
                    else path = Path.Combine(Storagepath, fileName);
                    fs = File.OpenRead(path);
                    bytesRemaining = fs.Length;
                    channel.openFileForWrite(fileName,msg.filePath);
                    while (true)
                    {
                        long bytesToRead = Math.Min(1024, bytesRemaining);
                        byte[] blk = new byte[bytesToRead];
                        long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                        bytesRemaining -= numBytesRead;
                        channel.writeFileBlock(blk);
                        if (bytesRemaining <= 0) break;
                    }
                    channel.closeFile();
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            if (msg.fileNames[0].EndsWith(".cs"))
            {
                Message buildDll = new Message(Message.MessageType.BuildDLL);
                buildDll.toAddress = msg.toAddress;
                buildDll.fromAddress = msg.fromAddress;
                buildDll.body = msg.body;
                channel.postMessage(buildDll);
            }
            if (msg.fileNames[0].EndsWith(".dll"))
            {
                Message executeDll = new Message(Message.MessageType.TestExecute);
                executeDll.toAddress = msg.toAddress;
                executeDll.fromAddress = msg.fromAddress;
                executeDll.filePath = msg.filePath;
                executeDll.body = msg.body;
                channel.postMessage(executeDll);
            }
            return true;
        }
    }


    public class Comm
    {
        private Receiver rcvr = null;
        private Sender sndr = null;
        private string address = null;
        private int portNum = 0;

        /*----< constructor >------------------------------------------*/
        /*
         * - starts listener listening on specified endpoint
         */
        public Comm(string baseAddress, int port)
        {
            address = baseAddress;
            portNum = port;
            rcvr = new Receiver();
            rcvr.start(baseAddress, port);
            sndr = new Sender(baseAddress, port);
        }

        /*----< closes Comm instance >--------------------------*/

        public void close()
        {
            Console.Write("\n  Comm closing");
            rcvr.close();
            sndr.close();
        }
        /*----< post message to remote Comm >--------------------------*/

        public void postMessage(Message msg)
        {
            sndr.postMessage(msg);
        }
        /*----< retrieve message from remote Comm >--------------------*/

        public Message getMessage()
        {
            return rcvr.getMessage();
        }
        /*----< called by remote Comm to upload file >-----------------*/

        public bool postFile(Message msg)
        {
            return sndr.postFile(msg);
        }
#if (MPCOMMSERVICE)
        public static void Main()
        {
            Comm comm = new Comm("http://localhost:",8080);
            Message msg = new Message(Message.MessageType.connect);
            msg.fromAddress = "http://localhost:8080/IService";
            msg.toAddress = "http://localhost:8080/IService";
            comm.postMessage(msg);
            Message msg1=comm.getMessage();
            Console.Write("\n received message type is:" + msg1.type);
        }
#endif

    }

}
