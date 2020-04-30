using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
using System.IO;

namespace WealthLab.Strategies
{
	public class MyStrategy : WealthScript
	{

		protected override void Execute()
		{
			string today = DateTime.Now.ToString("yyyy-MM-dd");
			string workingDir = @"Z:\Win10D";
			string path = Path.Combine(workingDir, "Data", today);
		
			if (!Directory.Exists(path))
				throw new Exception("You must create the directory " + path);

			PrintStatusBar(today + " Complete!");
			
			//Pushed indicator ChartPane statements
			ChartPane paneEquitySummaryScore1 = CreatePane(40,true,true);
			ChartPane paneEquitySummaryCategory1 = CreatePane(40,true,true);
			ChartPane paneEarnings1 = CreatePane(40,true,true);

			//Pushed indicator PlotSeries statements
			PlotFundamentalItems(paneEquitySummaryScore1,"equity summary score",Color.FromArgb(255,255,0,0),LineStyle.Solid,1);
			PlotFundamentalItems(paneEquitySummaryCategory1,"equity summary category",Color.FromArgb(255,255,0,0),LineStyle.Solid,1);
			PlotFundamentalItems(paneEarnings1,"estimated earnings",Color.FromArgb(255,0,128,128),LineStyle.Solid,1);
			PlotFundamentalItems(paneEarnings1,"earnings per share",Color.FromArgb(255,255,0,0),LineStyle.Solid,1);

		}
	}
}
