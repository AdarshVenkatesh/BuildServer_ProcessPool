////////////////////////////////////////////////////////////////////////////////////////
// BuildLibrary.cs : This package is used for spawning a process to build dll.        //
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
* This package builds the library and reports success or failure of the build.
*
* public Interfaces:
* =================
* libraryBuilder(): spawns a process to build a dll file from cs files.
*
* Required Files:
* ===============
* TestInterfaces.dll
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
using System.Diagnostics;
using System.IO;


namespace FederationServers
{
    public class Builder
    {
        /*----< Builds library and returns build status >----------------*/

        public string libraryBuilder(List<string> files, String Directory)
        {
            Console.Write("\n\n*************Building Library************");
            Console.Write("\n\n Building libary at :" + Directory+"\n");
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            String filelist = "";
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);
                filelist = filelist + " " + filename;
            }

            filelist = "/nologo /target:library  /r:../../TestInterfaces/bin/Debug/TestInterfaces.dll" + filelist;

            Console.Write("\n \n Build Command:" + "csc " + filelist);

            pProcess.StartInfo.FileName = "csc";


            pProcess.StartInfo.Arguments = filelist;

            pProcess.StartInfo.UseShellExecute = false;

            pProcess.StartInfo.RedirectStandardOutput = true;

            pProcess.StartInfo.WorkingDirectory = Directory;

            //Start the process
            pProcess.Start();
            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            if (strOutput == "") Console.Write("\n \n Build Status: Build Success");
            else
            {
                Console.Write("\n \n Build Status: Build Failed");
                Console.WriteLine("\n \n Error Output:" + strOutput);
            }
            //Wait for process to finish
            pProcess.WaitForExit();
            return strOutput;
        }

#if (BUILD)
        static void Main(string[] args)
        {
            string Storagepath = @"../../../RepositoryFolder";
            Console.Write("Repository path is:"+Storagepath);
            Repo repo = new Repo();

            repo.getFilesHelper(Storagepath, "*.cs*");
            foreach (string file in repo.files)
            {
                Console.Write("\n \"{0}\"", file);
            }

            Builder build = new Builder();
            build.libraryBuilder(repo.files,Storagepath);
            Console.Read();
        }
#endif
    }
}
