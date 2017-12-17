////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs : This package helps to display things on window.               //
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
* This package displays the main window to the client.
* The package helps to display the build requests, logs from the repository to the client.
* The client can create build request and can store or send the build request to the mother builder.
* This module also demonstartes the requirements for WPF module in the console.
*
* public Interfaces:
* =================
* rcvThreadProc(): Continuously monitors the receive queue and assigns tasks to other components.
* addDrivers(): invokes a dispatcher to add test drivers to listbox received from repository.
* addTestCodes(): invokes a dispatcher to add test codes to listbox received from repository.
* addDriverDispatcher(): adds files to the listbox by checking access.
* fetchDrivers_Click(): sends a message to repository to for displaying test drivers.
* fetchTestCodes_Click(): sends a message to repository to for displaying test codes.
* Add_Click(): Adds test drivers and test codes for creating build request.
* Remove_Click(): Removes selected test drivers and test codes from final list box.
* Create_Click(): creates build request.
* saveBuildRequest_Click(): requests repository to save the build request.
* sendToMotherBuilder_Click(): sends the build request to mother builder.
* sendBuildRequests_Click(): sends build requests to mother builder.
* displayBuildRequests_Click(): sends a message to repository to for displaying build requests.
* addBuildRequests(): invokes a dispatcher to add build requests to listbox received from repository.
* viewBuildRequest_doubleClick(): opens a new window to display content of build request.
* viewSelectedFile(): displays the contents of a file
* displayBuildLogs_Click(): sends a message to repository to for displaying build logs.
* addBuildLogs(): invokes a dispatcher to add build logs to listbox received from repository.
* buildLog_doubleClick(): opens a new window to display content of build log.
* displayTestLogs_Click(): sends a message to repository to for displaying test logs.
* addTestLogs(): invokes a dispatcher to add test logs to listbox received from repository.
* testLog_doubleClick(): opens a new window to display content of test log.
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace FederationServers
{
    public partial class MainWindow : Window
    {
        IAsyncResult cbResult;
        Action<List<String>> proc = null;
        Message message;
        static int testNo = 1;
        Comm comm { get; set; } = null;
        Thread rcvThread = null;
        TestRequest tr;
        public MainWindow()
        {
            InitializeComponent();
            comm = new Comm("http://localhost", 9060);
            message = new Message(Message.MessageType.connect);
            message.toAddress = "http://localhost:9070/IService";
            message.fromAddress = "http://localhost:9060/IService";
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
            main();
            
        }


        /*----< Monitors the receive queue and assigns tasks to other components >----------------*/

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                Message msg1 = comm.getMessage();
                msg1.show();
                switch (msg1.type)
                {
                    case Message.MessageType.Drivers:
                        addDrivers(msg1.fileNames);
                        break;
                    case Message.MessageType.TestCodes:
                        addTestCodes(msg1.fileNames);
                        break;
                    case Message.MessageType.BuildRequests:
                        addBuildRequests(msg1.fileNames);
                        break;
                    case Message.MessageType.BuildLogs:
                        addBuildLogs(msg1.fileNames);
                        break;
                    case Message.MessageType.TestLogs:
                        addTestLogs(msg1.fileNames);
                        break;
                }
            }
        }


        /*----< Requests mother builder to start child builders >----------------*/

        private void startChildProcesses_Click(object sender,RoutedEventArgs e)
        {
           
           String num= numberofProcess.SelectedIndex.ToString();
           int nums= Convert.ToInt32(num);
            Message msg = new Message(Message.MessageType.SpawnProcess);
            msg.numberProcess = nums+1;
            msg.fromAddress = "http://localhost:9060/IService";
            msg.toAddress = "http://localhost:9080/IService";
            comm.postMessage(msg);
        }



        /*----< Invokes dispatcher to add files to listbox >----------------*/

        public void addDrivers(List<string> drivers)
        {
            proc = this.addDriverDispatcher;
            cbResult = proc.BeginInvoke(drivers, null, null);
        }


        /*----< Invokes dispatcher to add files to listbox >----------------*/

        public void addTestCodes(List<string> testCodes)
        {
            proc = this.addTestCodesDispatcher;
            cbResult = proc.BeginInvoke(testCodes, null, null);
        }


        /*----< dispatcher checks for access and dispatches to a method for addition of file to list box >----------------*/

        void addDriverDispatcher(List<string> drivers)
        {
            foreach (string file in drivers)
            {
                    if (Dispatcher.CheckAccess())
                        addFiletDriver(file);
                    else
                        Dispatcher.Invoke(
                          new Action<string>(addFiletDriver),
                          System.Windows.Threading.DispatcherPriority.Background,
                           new string[] { file }
                        );
            }
        }


        /*----< dispatcher checks for access and dispatches to a method for addition of file to list box >----------------*/

        void addTestCodesDispatcher(List<string> testCodes)
        {
            foreach (string file in testCodes)
            {
                if (Dispatcher.CheckAccess())
                    addFiletCases(file);
                else
                    Dispatcher.Invoke(
                      new Action<string>(addFiletCases),
                      System.Windows.Threading.DispatcherPriority.Background,
                       new string[] { file }
                    );
            }

        }


        /*----< Adds driver files to the list box >----------------*/

        void addFiletDriver(string file)
        {
            leftDriverBox.Items.Insert(0, file);
            Console.Write("\n Retrieved Test driver from Repo is:" + file);
        }


        /*----< Adds test code files to the list box >----------------*/

        void addFiletCases(string file)
        {
            leftTCBox.Items.Insert(0, file);
            Console.Write("\n Retrieved Test code from Repo is:" + file);
        }


        /*----< Requests repo for test drivers >----------------*/

        private void fetchDrivers_Click(object sender, RoutedEventArgs e)
        {
            leftDriverBox.Items.Clear();
            Message msg = message.Clone();
            msg.type = Message.MessageType.Drivers;
            comm.postMessage(msg);

        }


        /*----< Requests repo for test codes >----------------*/

        private void fetchTestCodes_Click(object sender, RoutedEventArgs e)
        {
            leftTCBox.Items.Clear();
            Message msg = message.Clone();
            msg.type = Message.MessageType.TestCodes;
            comm.postMessage(msg);

        }


        /*----< Adds selected drivers and files to a final list box >----------------*/

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if(leftDriverBox.SelectedItems.Count==0)
            {
                MessageBox.Show("Select one test Driver");
                return;
            }
            if(leftTCBox.SelectedItems.Count==0)
            {
                MessageBox.Show("Select atleast one test code");
                return;
            }
            foreach (var x in leftTCBox.SelectedItems)
            {
                Console.Write("\n The selected test code is:" + x);
                rightTCBox.Items.Insert(0, x);
            }
            foreach (var x in leftDriverBox.SelectedItems)
            {
                Console.Write("\n The selected test driver is:" + x);
                rightDriverBox.Items.Insert(0, x);
            }
        }


        /*----< Removes selected files from list box >----------------*/

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < rightTCBox.SelectedItems.Count; i++)
            {
                rightTCBox.Items.Remove(rightTCBox.SelectedItems[i]);
            }
            for (int i = 0; i < rightDriverBox.SelectedItems.Count; i++)
            {
                rightDriverBox.Items.Remove(rightDriverBox.SelectedItems[i]);
            }
            rightTCBox.Items.Refresh();
        }


        /*----< Generates new build request >----------------*/

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            List<String> testCases = new List<String>();
            List<String> drivers = new List<String>();
            if (rightDriverBox.Items.Count <= 0)
            {
                MessageBox.Show("Please select and add a Test Driver");
                return;
            }
            else if (rightTCBox.Items.Count <= 0)
            {
                MessageBox.Show("Please select and add atleast one testcode");
                return;
            }
            else
            {
                foreach (String testcase in rightTCBox.Items)
                {
                    testCases.Add(System.IO.Path.GetFileName(testcase));
                }
                foreach (String driver in rightDriverBox.Items)
                {
                    drivers.Add(System.IO.Path.GetFileName(driver));
                }
                if(tr==null)
                {
                    tr = new TestRequest();
                }
                TestElement testElement = new TestElement();
                testElement.addDriver(drivers[0]);
                foreach(string testcode in testCases)
                {
                    testElement.addCode(testcode);
                }
                testElement.testName = "test" + testNo;
                testNo++;
                tr.author = "Jim Fawcett";
                tr.tests.Add(testElement);
            }
            String xml = tr.ToXml();
            Console.Write("\n\nThe created XML is:\n" + xml + "\n");
            createdBuildRequest.Clear();
            createdBuildRequest.Text = xml;
        }


        /*----< Helps to add more tests to a build request >----------------*/
        private void addMoreTests_Click(object sender ,RoutedEventArgs e)
        {
            if(createdBuildRequest.Text.Equals("") || createdBuildRequest.Text==null)
            {
                MessageBox.Show("First create build request with one test");
                return;
            }
            leftDriverBox.SelectedIndex = -1;
            leftTCBox.SelectedIndex = -1;
            rightDriverBox.Items.Clear();
            rightTCBox.Items.Clear();
        }


        /*----< clears all listboxes to create one more build request >----------------*/

        private void clearAll_Click(object sender,RoutedEventArgs e)
        {
            leftDriverBox.SelectedIndex=-1;
            leftTCBox.SelectedIndex = -1;
            rightDriverBox.Items.Clear();
            rightTCBox.Items.Clear();
            createdBuildRequest.Clear();
            tr = null;
            testNo = 1;
        }


        /*----< Requests repo to save the generated build request >----------------*/

        private void saveBuildRequest_Click(object sender, RoutedEventArgs e)
        {
            if(createdBuildRequest.Text.Equals(""))
            {
                MessageBox.Show("Please create a build request");
                return;
            }
            Message msg = message.Clone();
            msg.body = createdBuildRequest.Text;
            msg.type = Message.MessageType.SaveBuildRequest;
            Console.Write("\nRequesting Repo to save build request\n");
            comm.postMessage(msg);
            MessageBox.Show("Build Request saved in Repo");
        }

        /*----< Sends created build request to mother builder >----------------*/

        private void sendToMotherBuilder_Click(object sender, RoutedEventArgs e)
        {
            if (createdBuildRequest.Text.Equals(""))
            {
                MessageBox.Show("Please create a build request");
                return;
            }
            Message msg = new Message(Message.MessageType.BuildRequest);
            msg.toAddress = "http://localhost:9080/IService";
            msg.fromAddress = "http://localhost:9060/IService";
            msg.body = createdBuildRequest.Text;
            comm.postMessage(msg);
            MessageBox.Show("Build Request sent to Mother Builder");
        }


        /*----< Sends build requests to Mother builder >----------------*/

        private void sendBuildRequests_Click(object sender,RoutedEventArgs e)
        {
            if(buildRequests.SelectedItems.Count<=0)
            {
                MessageBox.Show("Please select build Requests");
                return;
            }
            for(int i=0;i<buildRequests.SelectedItems.Count;i++)
            {
                String path = System.IO.Path.GetFullPath(buildRequests.SelectedItems[i].ToString());
                Message msg = new Message(Message.MessageType.BuildRequest);
                msg.toAddress = "http://localhost:9080/IService";
                msg.fromAddress = "http://localhost:9060/IService";
                msg.body = File.ReadAllText(path);
                Console.Write("\n sending the build request to mother builder\n");
                comm.postMessage(msg);
                
            }
            MessageBox.Show("Build Requests sent to Mother Builder");
        }


        /*----< Requests repo for build requests >----------------*/

        private void displayBuildRequests_Click(object sender, RoutedEventArgs e)
        {
            buildRequests.Items.Clear();
            Message msg = message.Clone();
            msg.type = Message.MessageType.BuildRequests;
            comm.postMessage(msg);
            buildRequests.Items.Clear();
        }


        /*----< Invokes dispatcher to add build request files to listbox >----------------*/

        public void addBuildRequests(List<string> buildRequests)
        {
            proc = this.addBuildRequestsDispatcher;
            cbResult = proc.BeginInvoke(buildRequests, null, null);
        }


        /*----< adds build requests to listbox >----------------*/

        void addFileBuildRequests(string file)
        {
            buildRequests.Items.Insert(0, file);
        }


        /*----< dispatcher checks for access and dispatches to a method for addition of file to list box >----------------*/

        void addBuildRequestsDispatcher(List<string> buildRequests)
        {
            foreach (string file in buildRequests)
            {
                if (Dispatcher.CheckAccess())
                    addFileBuildRequests(file);
                else
                    Dispatcher.Invoke(
                      new Action<string>(addFileBuildRequests),
                      System.Windows.Threading.DispatcherPriority.Background,
                       new string[] { file }
                    );
            }

        }


        /*----< displays build requests on pop up window >----------------*/

        private void viewBuildRequest_doubleClick(object sender, RoutedEventArgs e)
        {
            String file = buildRequests.SelectedValue as string;
            viewSelectedFile(file);
        }


        /*----< Opens a new window to display file content >----------------*/

        private void viewSelectedFile(string file)
        {
            ViewFile viewFile = new ViewFile();
            String path = System.IO.Path.GetFullPath(file);
            viewFile.displayContent.Text = File.ReadAllText(path);
            viewFile.Show();
        }


        /*----< Requests repo for build logs >----------------*/

        private void displayBuildLogs_Click(object sender, RoutedEventArgs e)
        {
            buildLogs.Items.Clear();
            Message msg = message.Clone();
            msg.type = Message.MessageType.BuildLogs;
            comm.postMessage(msg);

        }


        /*----< Invokes dispatcher to add build log files to listbox >----------------*/

        public void addBuildLogs(List<string> buildLogs)
        {
            proc = this.addBuildLogsDispatcher;
            cbResult = proc.BeginInvoke(buildLogs, null, null);
        }


        /*----< adds build logs to listbox >----------------*/

        void addFileBuildLogs(string file)
        {
            buildLogs.Items.Insert(0, file);
        }
        

        /*----< dispatcher checks for access and dispatches to a method for addition of file to list box >----------------*/

        void addBuildLogsDispatcher(List<string> buildLogs)
        {
            foreach (string file in buildLogs)
            {
                if (Dispatcher.CheckAccess())
                    addFileBuildLogs(file);
                else
                    Dispatcher.Invoke(
                      new Action<string>(addFileBuildLogs),
                      System.Windows.Threading.DispatcherPriority.Background,
                       new string[] { file }
                    );
            }

        }


        /*----< displays build logs on pop up window >----------------*/

        private void buildLog_doubleClick(object sender, RoutedEventArgs e)
        {
            String file = buildLogs.SelectedValue as string;
            viewSelectedFile(file);
        }


        /*----< Requests repo for test logs >----------------*/

        private void displayTestLogs_Click(object sender, RoutedEventArgs e)
        {
            testLogs.Items.Clear();
            Message msg = message.Clone();
            msg.type = Message.MessageType.TestLogs;
            comm.postMessage(msg);

        }


        /*----< Invokes dispatcher to add test log files to listbox >----------------*/

        public void addTestLogs(List<string> testLogs)
        {
            proc = this.addTestLogsDispatcher;
            cbResult = proc.BeginInvoke(testLogs, null, null);
        }


        /*----< adds test logs to listbox >----------------*/

        void addFileTestLogs(string file)
        {
            testLogs.Items.Insert(0, file);
        }


        /*----< dispatcher checks for access and dispatches to a method for addition of file to list box >----------------*/

        void addTestLogsDispatcher(List<string> testLogs)
        {
            foreach (string file in testLogs)
            {
                if (Dispatcher.CheckAccess())
                    addFileTestLogs(file);
                else
                    Dispatcher.Invoke(
                      new Action<string>(addFileTestLogs),
                      System.Windows.Threading.DispatcherPriority.Background,
                       new string[] { file }
                    );
            }

        }


        /*----< displays test logs on pop up window >----------------*/

        private void testLog_doubleClick(object sender, RoutedEventArgs e)
        {
            String file = testLogs.SelectedValue as string;
            viewSelectedFile(file);
        }

        void main()
        {
            Message msg = new Message(Message.MessageType.connect);
            msg.toAddress = "http://localhost:9060/IService";
            msg.fromAddress = "http://localhost:9060/IService";
            comm.postMessage(msg);

            Message msg2 = msg.Clone();
            msg2.type = Message.MessageType.BuildRequest;
            msg2.toAddress = "http://localhost:9070/IService";
            msg2.body = "Build Request";
            comm.postMessage(msg2);
        }

    }
}
