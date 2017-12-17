/////////////////////////////////////////////////////////////////////
// CodeToTest2.cs - define code to be tested                       //
//                                                                 //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2016 //
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadingTests
{
  public class CodeToTest2
  {
    public void annunciator()
    {
            int a = 7;
            int b = 9;

      Console.Write("\n  Addition of : {0},{1} is {2}",a,b,a+b);
    }
    static void Main(string[] args)
    {
      CodeToTest2 ctt = new CodeToTest2();
      ctt.annunciator();
      Console.Write("\n\n");
    }
  }
}
