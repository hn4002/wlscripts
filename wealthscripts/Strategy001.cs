using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using WealthLab;
using WealthLab.Indicators;

namespace WealthLab.Strategies
{
	public class MyStrategy : WealthScript
	{
		Color colorUp = Color.FromArgb(39, 54, 233);
		Color colorDown = Color.FromArgb(222, 50, 174);

		protected override void Execute()
		{
			
			SetBarColors( colorUp, colorDown );
			SetLogScale( PricePane, true );
			SetLogScale( VolumePane, true );


			DataSeries maFast = SMA.Series(Close, 20);
			DataSeries maSlow = SMA.Series(Close, 60);
			DataSeries ma = SMA.Series(Close, 20);

			PlotSeries(PricePane,SMA.Series(Close,20),Color.Red,LineStyle.Solid,2);
			PlotSeries(PricePane,SMA.Series(Close,60),Color.Green,LineStyle.Solid,2);
			PlotSeries(PricePane,SMA.Series(Close,20),Color.Blue,LineStyle.Solid,2);

			for(int bar = GetTradingLoopStartBar(61); bar &lt; Bars.Count; bar++)
			{
				if (IsLastPositionActive)
				{
					Position p = LastPosition;
					if (p.EntrySignal.Contains("Group1|"))
					{
						if (CrossUnder(bar, Close, ma))
						{
							SellAtClose(bar, p, "Group1");
						}
					}

				}
				else
				{
					if (CrossOver(bar, maFast, maSlow))
					{
						BuyAtClose(bar, "Group1|");
					}

				}
			}
		}
	}
}
