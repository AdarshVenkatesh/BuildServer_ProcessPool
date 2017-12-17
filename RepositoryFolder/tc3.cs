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
  public class CodeToTest3
  {
    public void annunciator()
    {
            int a = 7;
            int b = 0;

      Console.Write(a/b);
    }
    static void Main(string[] args)
    {
      CodeToTest3 ctt = new CodeToTest3();
      ctt.annunciator();
      Console.Write("\n\n");
    }
  }
}
