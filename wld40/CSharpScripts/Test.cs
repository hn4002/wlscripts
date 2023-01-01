
/*[SCRIPT]
#AddReference Interop.WealthLab.dll
#AddReference WLE.dll
#IncludeFile CSharpScripts\Include\StdLib.cs
#IncludeFile CSharpScripts\Include\InternalLib.cs

[/SCRIPT]*/


using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

using WealthLab;


namespace MyScript
{
	public partial class MyClass 
    {
		public void TestTest ( )
		{
			try
			{
                Print ( "It is working" );
				MessageBox.Show ( "Test is working. CurrDir = " + Environment.CurrentDirectory, "Test" );
			}
			catch ( Exception e )
			{
				MessageBox.Show ( e.Message + "\r\n" + e.StackTrace );
			}

		}
		public void TestTest2 ( IWealthLabAddOn3 wl )
		{
			try
			{
				StdLibInit ( wl );
            }
			catch ( Exception e )
			{
				MessageBox.Show ( e.Message + "\r\n" + e.StackTrace );
			}

		}		
	}
}

