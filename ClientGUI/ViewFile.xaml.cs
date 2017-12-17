////////////////////////////////////////////////////////////////////////////////////////
// ViewFile.xaml.cs : This package helps to display file contents on window.          //
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
* This package displays the file contents of build requests and log files.
*
* public Interfaces:
* ================= 
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FederationServers
{
    /// <summary>
    /// Interaction logic for ViewFile.xaml
    /// </summary>
    public partial class ViewFile : Window
    {
        /*----< Constructor >----------------*/
        public ViewFile()
        {
            InitializeComponent();
        }
    }
}
