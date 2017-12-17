////////////////////////////////////////////////////////////////////////////////////////
// ITest.cs : This package declares ITest Interface.                                  //
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
* This package declares Itest Interface with a bool test method.
*
* public Interfaces:
* =================
* test(): a method declared in interface which is implemented by test drivers.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingTests
{
    public interface ITest
    {
        /*----< method that will be implemented by test drivers >----------------*/
        bool test();
    }
    class TestDemo : ITest
    {
        public bool test()
        {
            Console.Write("The bool test method is implemented \n");
            return true;
        }

#if (ITEST)
        static void Main(string[] args)
        {
            Console.Write("\n Demonstration of a class implementing an interface \n");
            ITest obj = new TestDemo();
            obj.test();
        }
#endif
    }
}
