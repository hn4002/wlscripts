using System;
using System.Collections.Generic;
using System.Text;
using WealthLab;
using WealthLab.Indicators;
using System.IO;

public class FastPriorUptrend : WealthScript
{
	bool NO_AUTO_EXECUTE = true;
	
	protected override void Execute()
	{
		const string sep = ",";
		const string fmt = "0.00########";
		string dateFormat = "yyyyMMdd";
		string workingDir = @"Z:\Win10D";
		int barsInAYear = 250;

		ClearDebug();

		string today = DateTime.Now.ToString("yyyy-MM-dd");
		string path = Path.Combine(workingDir, "Data", today);
		
		PrintDebug(String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
			"Symbol", "HighPrice", "LowPrice", "PriceDiffPct", "Weeks", "PriceDiffPctWk"));

		for(int ds = 0; ds < DataSetSymbols.Count; ds++)
		{
			string symbol = DataSetSymbols[ds];
			Bars bars = GetExternalSymbol( symbol, false );

			PrintStatusBar("Processing: " + ds + " / " + DataSetSymbols.Count + " : " + bars.Symbol + "     " );
			
			if (bars.Count &gt; barsInAYear) {
				double highPrice = Highest.Value(bars.Count - 1, bars.High, barsInAYear);
				double highBar = HighestBar.Value(bars.Count - 1, bars.High, barsInAYear);
				double lowPrice = Lowest.Value(bars.Count - 1, bars.Low, barsInAYear);
				double lowBar = LowestBar.Value(bars.Count - 1, bars.Low, barsInAYear);
				if (highBar &gt; lowBar) {
					double priceDiffPct = (highPrice - lowPrice) / lowPrice * 100;
					double weeks = Math.Ceiling((highBar - lowBar) / 5.0);
					double priceDiffPctWk = priceDiffPct / weeks;
					PrintDebug(String.Format("{0}\t{1:f}\t{2:f}\t{3:f}\t{4}\t{5:f}", 
						symbol, highPrice, lowPrice, priceDiffPct, weeks, priceDiffPctWk));
					
					
				}
			}
		
			Bars.Cache.Clear(); 
		}
		//PrintStatusBar("Complete!");		
	}
}
