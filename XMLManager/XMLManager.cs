////////////////////////////////////////////////////////////////////////////////////////
// XMLManager.cs : This package is used for parsing and creating XML.                 //
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
* This package is used for parsing the test request and for creating the test request XML and 
* log XML.
*
* public Interfaces:
* =================
* log_creator(): Forms logs into an XML.
*
* Required Files:
* ===============
*
* Maintainance History:
* =====================
* ver 1.0
*
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FederationServers
{
    public class LogData
    {
        public string status { get; set; } = "";
        public string testDriver { get; set; } = "";
        public string testname { get; set; } = "";
        public string author { get; set; } = "";
        public string message { get; set; } = "";
        public string BuildLogPath { get; set; } = "../../RepositoryFolder/BuildLog";
        public string TestLogPath { get; set; } = "../../RepositoryFolder/TestLog";

        public LogData()
        {
            if (!Directory.Exists(BuildLogPath))
                Directory.CreateDirectory(BuildLogPath);
            if (!Directory.Exists(TestLogPath))
                Directory.CreateDirectory(TestLogPath);
        }

        /*----< Creates log XML for each test request >----------------*/
        public static string log_creator(List<LogData> data, string rootName)
        {
            XDocument doc =
              new XDocument(
                new XElement(rootName,
                  new XElement("author", data[0].author),
                      from LogData data1 in data
                      select new XElement(
                      "TestElement", new XElement("TestName", data1.testname),
                                     new XElement("Status", data1.status),
                                      new XElement("ErrorMessage", data1.message)
                          )));
            Console.Write("\n \n" + rootName + " for Test Request is:\n" + doc.ToString());
            return doc.ToString();
        }
    }
    public class TestElement  /* information about a single test */
    {
        public string testName { get; set; }
        public string testDriver { get; set; }
        public List<string> testCodes { get; set; } = new List<string>();

        public TestElement() { }
        public TestElement(string name)
        {
            testName = name;
        }
        public void addDriver(string name)
        {
            testDriver = name;
        }
        public void addCode(string name)
        {
            testCodes.Add(name);
        }
        public override string ToString()
        {
            string temp = "\n    test: " + testName;
            temp += "\n      testDriver: " + testDriver;
            foreach (string testCode in testCodes)
                temp += "\n      testCode:   " + testCode;
            return temp;
        }
    }


    public class TestRequest  /* a container for one or more TestElements */
    {
        public string author { get; set; }
        public List<TestElement> tests { get; set; } = new List<TestElement>();

        public TestRequest() { }
        public TestRequest(string auth)
        {
            author = auth;
        }
        public override string ToString()
        {
            string temp = "\n  author: " + author;
            foreach (TestElement te in tests)
                temp += te.ToString();
            return temp;
        }

#if(XMLMANAGER)
        static void Main(string[] args)
        {
            ///////////////////////////////////////////////////////////////
            // Serialize and Deserialize TestRequest data structure

            TestElement te1 = new TestElement();
            te1.testName = "test1";
            te1.addDriver("td1.dll");
            te1.addCode("tc1.dll");
            te1.addCode("tc2.dll");

            TestElement te2 = new TestElement();
            te2.testName = "test2";
            te2.addDriver("td2.dll");
            te2.addCode("tc3.dll");
            te2.addCode("tc4.dll");

            TestRequest tr = new TestRequest();
            tr.author = "Jim Fawcett";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            string trXml = tr.ToXml();
            Console.Write("\n  Serialized TestRequest data structure:\n\n  {0}\n", trXml);

            TestRequest newRequest = trXml.FromXml<TestRequest>();
            string typeName = newRequest.GetType().Name;
            Console.Write("\n  deserializing xml string results in type: {0}\n", typeName);
            Console.Write(newRequest);
            Console.WriteLine();
        }
#endif
    }
}
