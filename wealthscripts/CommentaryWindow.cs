using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
using Community.Components; // CommentaryWindow here
/*** Requires installation of Community.Components Extension from www.wealth-lab.com &gt; Extensions ***/namespace WealthLab.Strategies
{
	public class MyStrategy : WealthScript
	{
		protected override void Execute()
		{
			// Path to the resulting HTML file
			string path = @"C:\Commentary.html";
         
			// Create an instance of the CommentaryWindow class
			CommentaryWindow cw = new CommentaryWindow( path );         
			for(int bar = Bars.Count-20; bar < Bars.Count; bar++)
			{
				// Add a line
				cw.AddLine( "Bar Number: " + bar.ToString() );
			}
         
			// Display Commentary Window
			cw.Display();
         
			// second commentary window *******************************
         
			// Path to the resulting HTML file
			string path2 = @"C:\Commentary2.html";
         
			// Create an instance of the CommentaryWindow class
			CommentaryWindow cw2 = new CommentaryWindow( path2 );         
			for(int bar = Bars.Count-40; bar < Bars.Count - 20; bar++)
			{
				// Add a line
				cw2.AddLine( "Bar Number: " + bar.ToString() );
			}
         
			// Display Commentary Window
			cw2.Display();

		}
	}
}
