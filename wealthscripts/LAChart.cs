//===========================================================================================================
// A WealthLab script to plot my faviorite indicators and chart marking for daily, weekly and monthly charts.
//----------------------------------------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;

public class MyStrategy : WealthScript
{
	//===========================================================================================================
	// Execute method. 
	// The control flow starts here.
	//----------------------------------------------------------------------------------------------------------- 
	protected override void Execute()
	{
		annotationSeries = null;
		if (Bars.Scale == BarScale.Daily)
		{
			dailyChart();
		}
		if (Bars.Scale == BarScale.Weekly)
		{
			weeklyChart();
		}
		if (Bars.Scale == BarScale.Monthly)
		{
			monthlyChart();
		}
	}

	//===========================================================================================================
	// Daily Chart
	//----------------------------------------------------------------------------------------------------------- 
	protected void dailyChart()
	{
		// Have some space in the right
		PadBars( 5 );

		// Simple Moving Averages
		PlotSeries(PricePane, SMA.Series(Close, 10), Color.Green, LineStyle.Solid, 1);
		PlotSeries(PricePane, SMA.Series(Close, 50), Color.Red, LineStyle.Solid, 1);
		PlotSeries(PricePane, SMA.Series(Close, 200), Color.Black, LineStyle.Solid, 1);

		PricePane.LogScale = false;
		VolumePane.LogScale = true;

		// S&P500 and RS Line
		drawIndexAndRsLines(1.02, 0.85);

		//show52WeeksHigh(251);
		
		//showXinYDaysUp(12, 15);
		//showXinYDaysUp(11, 13);
		//showXinYDaysUp(10, 11);
		showXinYDaysUpHistorical(12, 15);
		showXinYDaysUpHistorical(11, 13);
		showXinYDaysUpHistorical(10, 11);
	}

	//===========================================================================================================
	// Weekly Chart
	//----------------------------------------------------------------------------------------------------------- 
	protected void weeklyChart()
	{
		// Have some space in the right
		PadBars( 5 );

		// Simple Moving Averages
		PlotSeries(PricePane, SMA.Series(Close, 10), Color.Red, LineStyle.Solid, 1);
		PlotSeries(PricePane, SMA.Series(Close, 40), Color.Black, LineStyle.Solid, 1);

		PricePane.LogScale = true;
		VolumePane.LogScale = true;

		//show52WeeksHigh(52);
		
		drawIndexAndRsLines(1.02, 0.60);

		show5WeeksOfQuietAccumulation();
	}

	//===========================================================================================================
	// Monthly Chart
	//----------------------------------------------------------------------------------------------------------- 
	protected void monthlyChart()
	{
		// Simple Moving Averages
		PlotSeries(PricePane, SMA.Series(Close, 12), Color.Red, LineStyle.Solid, 1);
		PlotSeries(PricePane, SMA.Series(Close, 60), Color.Black, LineStyle.Solid, 1);

		PricePane.LogScale = true;
		VolumePane.LogScale = true;

		//show52WeeksHigh(12);
	}

	//===========================================================================================================
	// RS Line
	// indexUpFactor - typically 1.02 to 1.2
	// rsDownFactor - typically 0.60 to 0.85
	//----------------------------------------------------------------------------------------------------------- 
	protected void drawIndexAndRsLines(double indexUpFactor, double rsDownFactor)
	{
		// Index Line
		Bars SPX = GetExternalSymbol(".SPX", true);
		double highRefPrice = Highest.Series(High, 200)[Bars.Count - 1];
		DataSeries SPXNormalized = highRefPrice * indexUpFactor * SPX.Close / SPX.Close[Bars.Count - 1];
		PlotSeries(PricePane, SPXNormalized, Color.Black, LineStyle.Solid, 1);
		//DataSeries SPXNormalized2 = highRefPrice/2 + highRefPrice/2 * 1.02 * SPX.Close / SPX.Close[Bars.Count-1];
		//PlotSeries (PricePane, SPXNormalized2, Color.Red, LineStyle.Solid, 1 );

		// RS Line
		DataSeries RS = Bars.Close / SPX.Close;
		DataSeries RS2 = Close[Bars.Count - 1] * rsDownFactor * RS / RS[Bars.Count - 1];
		//PlotSeries (CreatePane( 30, true, true ), RS, Color.Blue, LineStyle.Solid, 1 );
		PlotSeries(PricePane, RS2, Color.Blue, LineStyle.Solid, 1);
	}

	//===========================================================================================================
	// 5 weeks of Quiet Accumulation
	//----------------------------------------------------------------------------------------------------------- 
	protected void show5WeeksOfQuietAccumulation()
	{
		DataSeries pctChange = Close * 2;
		for (int bar = 1; bar < Bars.Count; bar++)
		{
			pctChange[bar] = 100 * (Close[bar] - Close[bar - 1]) / Close[bar - 1];
		}
		for (int bar = 5; bar < Bars.Count; bar++)
		{
			if (pctChange[bar] > 0 && pctChange[bar] < 3
				&& pctChange[bar - 1] > 0 && pctChange[bar - 1] < 3
				&& pctChange[bar - 2] > 0 && pctChange[bar - 2] < 3
				&& pctChange[bar - 3] > 0 && pctChange[bar - 3] < 3
				&& pctChange[bar - 4] > 0 && pctChange[bar - 4] < 3)
			{
				AnnotateBar("|", bar, false, Color.Black);
				AnnotateBar("|", bar, false, Color.Black);
				AnnotateBar("|", bar, false, Color.Black);
				AnnotateBar("5wQA", bar, false, Color.Black, Color.Yellow);
			}
		}
	}

	//===========================================================================================================
	// AnnotateBarEx - Annotate Bar with some distance
	//----------------------------------------------------------------------------------------------------------- 
	DataSeries annotationSeries = null;
	protected void annotateBar(string text, int bar, bool aboveBar, Color color, Color backgroundColor)
	{
		if (annotationSeries == null)
		{
			annotationSeries = Close * 0;
		}
		if (annotationSeries[bar] < 1)
		{
			AnnotateBar("|", bar, false, Color.Black, Color.Yellow);
			AnnotateBar("|", bar, false, Color.Black, Color.Yellow);
			AnnotateBar("|", bar, false, Color.Black, Color.Yellow);
		}
		AnnotateBar(text, bar, false, Color.Black, Color.Yellow);
		annotationSeries[bar] = annotationSeries[bar] + 1;
	}

	//===========================================================================================================
	// Show X in Y Days Up
	//----------------------------------------------------------------------------------------------------------- 
	protected void showXinYDaysUp(int xDays, int yDays)
	{
		if (Bars.Count < (yDays + 1))
		{
			return;
		}
		
		int count = 0;
		for (int i = 0; i < yDays; i++)
		{
			int iBar = Bars.Count - 1 - i;
			if (Close[iBar-1] <= Close[iBar])
			{
				count++;
			}
		}
		if (count >= xDays)
		{
			string label = String.Format("Add On: {0} of {1} days UP", xDays, yDays);
			DrawLabel(PricePane, label);
			String annotation = String.Format("{0}/{1}", xDays, yDays);
			annotateBar(annotation, Bars.Count - 1, false, Color.Black, Color.Yellow);		
		}	
	}

	//===========================================================================================================
	// Show X in Y Days Up
	//----------------------------------------------------------------------------------------------------------- 
	protected void showXinYDaysUpHistorical(int numUpDays, int numTotalDays)
	{
		if (Bars.Count < (numTotalDays + 1))
		{
			return;
		}
		
		for (int bar = numTotalDays; bar < Bars.Count; bar++)
		{
			int count = 0;
			for (int lookback = numTotalDays - 1; lookback >= 0; lookback--)
			{
				int adjustedBar = bar - lookback;
				if (Close[adjustedBar-1] <= Close[adjustedBar])
				{
					count++;
				}
			}
			if (count >= numUpDays)
			{
				string annotation = String.Format("{0}/{1}", numUpDays, numTotalDays);
				annotateBar(annotation, bar, false, Color.Black, Color.Yellow);
			}	
		}
	}

	//===========================================================================================================
	// Show 52 weeks high
	//----------------------------------------------------------------------------------------------------------- 
	protected void show52WeeksHigh(int numBarsIn52Weeks)
	{
		// 52-week high
		for (int bar = numBarsIn52Weeks; bar < Bars.Count; bar++)
		{
			if (High[bar] > Highest.Series(High, numBarsIn52Weeks)[bar - 1])
			{
				// New 52-week high detected.  Paint the chart blue
				SetBackgroundColor(bar, Color.FromArgb(15, Color.Blue));
			}
		}
	}
}
