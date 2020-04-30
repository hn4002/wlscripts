using System;
using System.Collections.Generic;
using System.Text;
using WealthLab;
using System.IO;

public class Export2ASCII : WealthScript
{
	bool NO_AUTO_EXECUTE = true;
	
	protected override void Execute()
	{
		if(NO_AUTO_EXECUTE)
		{
			NO_AUTO_EXECUTE = false;
			return;
		}
		
		const string sep = ",";
		const string fmt = "0.00########";
		string dateFormat = "yyyyMMdd";
		string workingDir = @"Z:\Win10D";

		string today = DateTime.Now.ToString("yyyy-MM-dd");
		string path = Path.Combine(workingDir, "Data", today);
		
		if (!Directory.Exists(path))
		{
			PrintDebug("Creating directory: " + path); FlushDebug();
			Directory.CreateDirectory(path);
		}
		
		for(int ds = 0; ds < DataSetSymbols.Count; ds++)
		{
			string symbol = DataSetSymbols[ds];
			Bars bars = GetExternalSymbol( symbol, false );

			PrintStatusBar("Exporting: " + ds + " / " + DataSetSymbols.Count + " : " + bars.Symbol + "     " );
			
			string file = Path.Combine(path, symbol + ".csv");
		
			List<string&gt; datalist = new List<string&gt;();
		
			for(int bar = 0; bar < bars.Count; bar++)
			{			
				string csv = bars.Date[bar].ToString(dateFormat) + sep 
					+ bars.Open[bar].ToString(fmt) + sep 
					+ bars.High[bar].ToString(fmt) + sep 
					+ bars.Low[bar].ToString(fmt) + sep 
					+ bars.Close[bar].ToString(fmt) + sep 
					+ bars.Volume[bar].ToString("0");
			
				datalist.Add(csv);
			}
			File.WriteAllLines(file, datalist);
		
			Bars.Cache.Clear(); 
		}
		//PrintStatusBar("Complete!");		
	}
}
