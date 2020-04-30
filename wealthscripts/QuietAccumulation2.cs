using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;
using Community.Indicators;

namespace WealthLab.Strategies
{
	public class MyStrategy : WealthScript
	{
		protected override void Execute()
		{
			var pctChange = ROC.Series(Close, 1);
			var rocUp = SeriesIsAboveValue.Series(pctChange, 0, 5);
			var rocDown = SeriesIsBelowValue.Series(pctChange, 3.0, 5);

			for (int bar = GetTradingLoopStartBar(1); bar &lt; Bars.Count; bar++)
			{
				if (rocUp[bar] &gt;= 5 &amp;&amp; rocDown[bar] &gt;= 5)
				{
					AnnotateBar("|", bar, false, Color.Black);
					AnnotateBar("|", bar, false, Color.Black);
					AnnotateBar("|", bar, false, Color.Black);
					AnnotateBar("5wQA", bar, false, Color.Black, Color.Yellow);
				}
			}

			var lh = LineStyle.Histogram;
			var ls = LineStyle.Solid;
			var pd = CreatePane(20, true, true);
			var pu = CreatePane(20, true, true);
			PlotSeries(pd, rocDown, Color.Red, lh, 1);
			PlotSeries(pu, rocUp, Color.Blue, lh, 1);
			DrawHorzLine(pd, 5, Color.Black, ls, 1);
			DrawHorzLine(pu, 5, Color.Black, ls, 1);
		}
	}
}
