using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using WealthLab.Core;
using WealthLab.Indicators;
using WealthLab.Backtest;



namespace WLHnChart
{
	public class HnChart101 : UserStrategyBase
	{
		// We need this to load the settings
		public const string settingsFile = @"C:\WLD\HnChart101.settings.yaml";

		// Load these values from Settings file
		struct Settings
		{
			public int extendedBars;
			public string SP500;     //"^GSPC"; // ^GSPC, .SPX, SPY, SP500
			public string NASDAQ;   //"^IXIC"; // ^GSPC, .SPX, SPY, SP500

			public string tradeFile;
			public string mpFile;
			public string wooFile;

			public Color upColor; // = System.Drawing.ColorTranslator.FromHtml("#1D13E3");
			public Color downColor; // = System.Drawing.ColorTranslator.FromHtml("#D30E9F");
		}

		// Class variables
		int lastBarIdx = 0;
		bool isIndex = false;
		Settings settings;

		public void loadSettings()
		{
			Dictionary<string, string> settingsMap = new Dictionary<string, string>();
			string[] lines = System.IO.File.ReadAllLines(settingsFile);
			foreach (string line in lines)
			{
				// extendedBars: 0
				line.Trim();
				if (line.Equals("") || line.StartsWith("#"))
				{
					continue;
				}
				string[] columns = line.Split(':', 2);
				var key = columns[0].Trim();
				var value = columns[1].Trim();
				settingsMap[key] = value;
			}
			this.settings.extendedBars = int.Parse(settingsMap["extendedBars"]);
			this.settings.SP500 = settingsMap["SP500"];
			this.settings.NASDAQ = settingsMap["NASDAQ"];
			this.settings.tradeFile = settingsMap["tradeFile"];
			this.settings.mpFile = settingsMap["mpFile"];
			this.settings.wooFile = settingsMap["wooFile"];
			this.settings.upColor = System.Drawing.ColorTranslator.FromHtml(settingsMap["upColor"]);
			this.settings.downColor = System.Drawing.ColorTranslator.FromHtml(settingsMap["downColor"]);
		}

		//create indicators and other objects here, this is executed prior to the main trading loop
		public override void Initialize(BarHistory bars)
		{
			loadSettings();

			// Have some space in the right
			bars.ExtendedBars = this.settings.extendedBars;
			this.lastBarIdx = bars.Count - 1 - this.settings.extendedBars;

			// Is Index
			if (bars.Symbol.StartsWith("^") || bars.Symbol.StartsWith("0")) 
				this.isIndex = true;

			// Company Symbol and Name
			string symbolCompanyName = bars.Symbol;
			if (bars.SecurityName == null && bars.SecurityName != "")
				symbolCompanyName = symbolCompanyName + " - " + bars.SecurityName;
			symbolCompanyName = symbolCompanyName + " - " + bars.EndDateDisplay;
			DrawHeaderText(symbolCompanyName, Color.Magenta, 18);

			if (bars.Scale == HistoryScale.Daily)
			{
				dailyChart(bars);
			}
			if (bars.Scale == HistoryScale.Weekly)
			{
				weeklyChart(bars);
			}
			if (bars.Scale == HistoryScale.Monthly)
			{
				monthlyChart(bars);
			}
		}

		//execute the strategy rules here, this is executed once for each bar in the backtest history
		public override void Execute(BarHistory bars, int idx) { }

		//===========================================================================================================
		// Daily Chart
		//----------------------------------------------------------------------------------------------------------- 
		protected void dailyChart(BarHistory bars)
		{
			// Simple Moving Averages
			PlotTimeSeriesLine(SMA.Series(bars.Close, 10), "SMA10", "Price", Color.Green, 1);
			PlotTimeSeriesLine(SMA.Series(bars.Close, 50), "SMA50", "Price", Color.Red, 1);
			PlotTimeSeriesLine(SMA.Series(bars.Close, 200), "SMA200", "Price", Color.Black, 1);

			// S&P500 and RS Line
			//if (bars.Symbol != this.settings.SP500)
			{
				drawIndexAndRsLines(bars, 1.02, 0.85);
			}

			//show52WeeksHigh(bars, 251);
			showTrades(bars, this.settings.tradeFile);
			showMP(bars, this.settings.mpFile);
			showWoo(bars, this.settings.wooFile);
			showVolume(bars);

			showXinYDaysUp(bars, 12, 15);
			//showXinYDaysUp(bars, 11, 13);
			//showXinYDaysUp(bars, 10, 11);
			//showXinYDaysUpHistorical(bars, 12, 15);
			//showXinYDaysUpHistorical(bars, 11, 13);
			//showXinYDaysUpHistorical(bars, 10, 11);
		}

		//===========================================================================================================
		// Weekly Chart
		//----------------------------------------------------------------------------------------------------------- 
		protected void weeklyChart(BarHistory bars)
		{
			// Simple Moving Averages
			PlotTimeSeriesLine(SMA.Series(bars.Close, 10), "SMA10", "Price", Color.Red, 1);
			PlotTimeSeriesLine(SMA.Series(bars.Close, 40), "SMA40", "Price", Color.Black, 1);

			//PricePane.LogScale = true;
			//VolumePane.LogScale = true;

			//show52WeeksHigh(52);

			// S&P500 and RS Line
			drawIndexAndRsLines(bars, 1.02, 0.60);

			show5WeeksOfQuietAccumulation(bars);
		}

		//===========================================================================================================
		// Monthly  Chart
		//----------------------------------------------------------------------------------------------------------- 
		protected void monthlyChart(BarHistory bars)
		{
			// Simple Moving Averages
			PlotTimeSeriesLine(SMA.Series(bars.Close, 12), "SMA12", "Price", Color.Red, 1);
			PlotTimeSeriesLine(SMA.Series(bars.Close, 60), "SMA60", "Price", Color.Black, 1);

			//PricePane.LogScale = true;
			//VolumePane.LogScale = true;

			//show52WeeksHigh(12);
		}

		//===========================================================================================================
		// Bar Annotations
		//----------------------------------------------------------------------------------------------------------- 
		private Dictionary<int, string> _annotationsAboveMap = new Dictionary<int, string>();
		private Dictionary<int, string> _annotationsBelowMap = new Dictionary<int, string>();

		protected void DrawBarAnnotation2(string text, int idx, bool aboveBar)
		{
			if (aboveBar)
			{
				if (_annotationsAboveMap.ContainsKey(idx))
					_annotationsAboveMap[idx] = _annotationsAboveMap[idx] + "\n" + text;
				else
					_annotationsAboveMap[idx] = text;

			}
			else
			{
				if (_annotationsBelowMap.ContainsKey(idx))
					_annotationsBelowMap[idx] = _annotationsBelowMap[idx] + "\n" + text;
				else
					_annotationsBelowMap[idx] = text;
			}
		}
		protected void DrawAllAnnotationsAtEnd(Color colorAbove, Color colorBelow, int fontSize)
		{
			foreach (KeyValuePair<int, string> entry in _annotationsAboveMap)
			{
				DrawBarAnnotation(entry.Value + "\n ", entry.Key, true, colorAbove, fontSize);
			}
			foreach (KeyValuePair<int, string> entry in _annotationsBelowMap)
			{
				DrawBarAnnotation(" \n" + entry.Value, entry.Key, false, colorBelow, fontSize);
			}
		}
		protected void ClearAllAnnotations()
		{
			_annotationsAboveMap.Clear();
			_annotationsBelowMap.Clear();
		}

		//===========================================================================================================
		// RS Line
		// indexUpFactor - typically 1.02 to 1.2
		// rsDownFactor - typically 0.60 to 0.85
		//----------------------------------------------------------------------------------------------------------- 
		protected void drawIndexAndRsLines(BarHistory bars, double indexUpFactor, double rsDownFactor)
		{
			StartIndex = 200;
			BarHistory SPX = GetHistory(bars, this.settings.SP500);
			BarHistory NDX = GetHistory(bars, this.settings.NASDAQ);
			//
			// Last 5 Bars Gains, weekly, monthly, quaterly gains
			DrawHeaderText(String.Format("{0,-15} {1,6} {2,6} {3,6} {4,7} {5,6} {6,11} {7,6} {8,6}",
				"Gains", "-5D", "-4D", "-3D", "-2D", "D", "W", "M", "Q"), Color.Brown, 14); 
			BarHistory[] barsArr = new BarHistory[]{ bars, SPX, NDX};
			string[] names = new string[3] { "INST", "SP5X", "NDX" };

			// Following indexes are from the end (that is why named prime)
			int[] startIdxPrime = new int[8] { -6, -5, -4, -3, -2, -6, -23, -67 };
			int[] endIdxPrime = new int[8] { -5, -4, -3, -2, -1, -1, -1, -1 };
			for (int i = 0; i < 3; i++)
			{
				var currBars = barsArr[i];
				object[] args = new object[9];
				args[0] = names[i];
				for (int j = 0; j < 8; j++)
				{
					var barsRequired = (-1 * startIdxPrime[j]) + 1;
					var startIdx = currBars.Count + startIdxPrime[j];
					var endIdx = currBars.Count + endIdxPrime[j];
					if (currBars.Count > barsRequired)
					{
						
						args[j + 1] = 100.0 * (currBars.Close[endIdx] - currBars.Close[startIdx]) / currBars.Close[startIdx];
					} else
                    {
						args[j + 1] = 0.0;
					}
					
				}
				DrawHeaderText(String.Format("{0,-15} {1,7:0.0} {2,7:0.0} {3,7:0.0} {4,7:0.0} {5,7:0.0} {6,12:0.0} {7,7:0.0} {8,7:0.0}",
					args), Color.Brown, 14);
			}

			// Index Line
			double highRefPrice = Highest.Value(lastBarIdx, bars.High, 200);
			//TimeSeries SPXNormalized = highRefPrice * indexUpFactor * SPX.Close / SPX.Close[lastBarIdx];
			//PlotTimeSeriesLine(SPXNormalized, "SPXNormalized", "Price", Color.Black, 1);

			var factor = highRefPrice * indexUpFactor / SPX.Close[lastBarIdx];
			BarHistory SPXScaledUp = new BarHistory(SPX);
			BarHistory SPXScaledDown = new BarHistory(SPX);
			for (int i = 0; i < SPX.Count; i++)
            {
				if (SPX.Close[i] >= SPX.Open[i])
				{
					SPXScaledUp.Add(SPX.DateTimes[i],
									SPX.Open[i] * factor,
									SPX.High[i] * factor,
									SPX.Low[i] * factor,
									SPX.Close[i] * factor,
									SPX.Volume[i]);
					SPXScaledDown.Add(SPX.DateTimes[i], float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
				}
				else
                {
					SPXScaledUp.Add(SPX.DateTimes[i], float.NaN, float.NaN, float.NaN, float.NaN, float.NaN);
					SPXScaledDown.Add(SPX.DateTimes[i],
									SPX.Open[i] * factor,
									SPX.High[i] * factor,
									SPX.Low[i] * factor,
									SPX.Close[i] * factor,
									SPX.Volume[i]);

				}
			}
			PlotBarHistoryStyle(SPXScaledUp, "Price", "bars", this.settings.upColor, false);
			PlotBarHistoryStyle(SPXScaledDown, "Price", "bars", this.settings.downColor, false);

			// RS Line
			TimeSeries RS = bars.Close / SPX.Close;
			TimeSeries RS2 = bars.Close[lastBarIdx] * rsDownFactor * RS / RS[lastBarIdx];
			//PlotTimeSeriesLine(RS, "RS", "RS", Color.Blue, 1);
			PlotTimeSeriesLine(RS2, "RS2", "Price", Color.Blue, 1);
		}

		//===========================================================================================================
		// 5 weeks of Quiet Accumulation
		//----------------------------------------------------------------------------------------------------------- 
		protected void show5WeeksOfQuietAccumulation(BarHistory bars)
		{
			ClearAllAnnotations();
			TimeSeries pctChange = ROC.Series(bars.Close, 1);

			for (int bar = 5; bar < bars.Count; bar++)
			{
				if (pctChange[bar] > 0 && pctChange[bar] < 3
				   && pctChange[bar - 1] > 0 && pctChange[bar - 1] < 3
				   && pctChange[bar - 2] > 0 && pctChange[bar - 2] < 3
				   && pctChange[bar - 3] > 0 && pctChange[bar - 3] < 3
				   && pctChange[bar - 4] > 0 && pctChange[bar - 4] < 3)
				{
					for (int i = 0; i <= 3; i++)
						DrawBarAnnotation2("|", bar, true);
					DrawBarAnnotation2("5wQA", bar, true);
				}
			}
			DrawAllAnnotationsAtEnd(Color.Black, Color.Black, 9);
		}

		//===========================================================================================================
		// Show 52 weeks high
		//----------------------------------------------------------------------------------------------------------- 
		protected void show52WeeksHigh(BarHistory bars, int numBarsIn52Weeks)
		{
			// 52-week high
			for (int bar = numBarsIn52Weeks; bar < bars.Count; bar++)
			{
				if (bars.High[bar] > Highest.Series(bars.High, numBarsIn52Weeks)[bar - 1])
				{
					// New 52-week high detected.  Paint the chart blue
					SetBackgroundColor(bars, bar, Color.FromArgb(15, Color.Blue));
				}
			}
		}

		//===========================================================================================================
		// Volume Pane
		//----------------------------------------------------------------------------------------------------------- 
		protected void showVolume(BarHistory bars)
		{
			TimeSeries myVolume = bars.Volume * 1.0;
			TimeSeries zero = bars.Volume * 0.0;
			for (int bar = 0; bar < bars.Count; bar++)
			{
				if (bars.Close[bar] > bars.Open[bar])
				{
					SetSeriesBarColor(myVolume, bar, this.settings.upColor);
				}
				else
				{
					SetSeriesBarColor(myVolume, bar, this.settings.downColor);
				}
			}
			PlotTimeSeries(myVolume, "MyVolume", "MyVolume", Color.Red, PlotStyles.Histogram);
			PlotTimeSeries(zero, "-", "MyVolume", Color.Black, PlotStyles.Line);
		}

		//===========================================================================================================
		// Show X in Y Days Up
		//----------------------------------------------------------------------------------------------------------- 
		protected void showXinYDaysUp(BarHistory bars, int xDays, int yDays)
		{
			int lastBarIdx = bars.Count - bars.ExtendedBars - 1;
			if (lastBarIdx < yDays)
			{
				return;
			}

			int count = 0;
			for (int i = 0; i < yDays; i++)
			{
				int iBar = lastBarIdx - i;
				if (bars.Close[iBar - 1] <= bars.Close[iBar])
				{
					count++;
				}
			}
			if (count >= xDays)
			{
				string label = String.Format("Add On: {0} of {1} days UP", xDays, yDays);
				DrawHeaderText(label, Color.Red, 10);
				String annotation = String.Format("{0}/{1}", xDays, yDays);
				DrawBarAnnotation(annotation, lastBarIdx, true, Color.Red, 12);
			}
		}

		//===========================================================================================================
		// Show X in Y Days Up
		//----------------------------------------------------------------------------------------------------------- 
		protected void showXinYDaysUpHistorical(BarHistory bars, int numUpDays, int numTotalDays)
		{
			int lastBarIdx = bars.Count - bars.ExtendedBars - 1;
			if (lastBarIdx < numTotalDays)
			{
				return;
			}

			for (int bar = numTotalDays; bar <= lastBarIdx; bar++)
			{
				int count = 0;
				for (int lookback = numTotalDays - 1; lookback >= 0; lookback--)
				{
					int adjustedBar = bar - lookback;
					if (bars.Close[adjustedBar - 1] <= bars.Close[adjustedBar])
					{
						count++;
					}
				}
				if (count >= numUpDays)
				{
					string annotation = String.Format("{0}/{1}", numUpDays, numTotalDays);
					DrawBarAnnotation(annotation, bar, true, Color.Red, 12);
				}
			}
		}

		//===========================================================================================================
		// Show MP
		//----------------------------------------------------------------------------------------------------------- 
		protected void showMP(BarHistory bars, string mpFile)
		{
			Dictionary<int, string> markers = new Dictionary<int, string>();
			string prevMp = "";

			string[] lines = System.IO.File.ReadAllLines(mpFile);
			foreach (string line in lines)
			{
				// 1/3/17,R,,5,9,d
				// 3/31/17,P,d,6,5,
				// 3/29/18,C,,x,x,
				string[] columns = line.Split(',');
				if (columns.Length < 6)
					continue;
				var dateStr = columns[0].Trim();
				var mp = columns[1].Trim();
				if (mp == "")
					continue;
				// If mp is not changing, then don't do anything
				if (mp == prevMp)
					continue;
				// There is mp change, so try to get bar and save in the map
				DateTime d = DateTime.Parse(dateStr);
				int mpBar = bars.IndexOf(d, true);
				if (mpBar < 0)
					continue;
				markers[mpBar] = mp;
				prevMp = mp;
			}
			
			foreach (KeyValuePair<int, string> entry in markers)
			{
				string mp = entry.Value;
				int idx = entry.Key;
				if (mp == "R")
				{
					//DrawDot(idx, bars.Low[idx] * 0.98, Color.FromArgb(160, 0, 255, 0), 5);
					DrawBarAnnotation("⬤", idx, false, Color.FromArgb(180, 0, 255, 0), 14, true);
				}
				else if (mp == "C")
				{
					//DrawDot(idx, bars.Low[idx] * 0.98, Color.FromArgb(160, 255, 0, 0), 5);
					DrawBarAnnotation("⬤", idx, false, Color.FromArgb(200, 255, 0, 0), 14, true);
				}
				else
                {
					//DrawDot(idx, bars.Low[idx] * 0.98, Color.FromArgb(160, 255, 127, 80), 5);
					DrawBarAnnotation("⬤", idx, false, Color.FromArgb(200, 245, 176, 65), 14, true);
				}
			}
			DrawHeaderText("⬤ - Market Pulse");
		}

		//===========================================================================================================
		// Show Woo
		//----------------------------------------------------------------------------------------------------------- 
		protected void showWoo(BarHistory bars, string wooFile)
        {	
			Dictionary<int, string> markers = new Dictionary<int, string>();
			string prevWoo = "";

			string[] lines = System.IO.File.ReadAllLines(wooFile);
			foreach (string line in lines)
			{
				// 3/14/17,WOO
				// 4/14/17,
				// 7/6/17,Correction
				string[] columns = line.Split(',');
				if (columns.Length < 2)
					continue;
				var dateStr = columns[0].Trim();
				var woo = columns[1].Trim();
				if (woo == "")
					continue;
				// If woo is not changing, then don't do anything
				if (woo == prevWoo)
					continue;
				// There is woo change, so try to get bar and save in the map
				DateTime d = DateTime.Parse(dateStr);
				int wooBar = bars.IndexOf(d, true);
				if (wooBar < 0)
					continue;
				markers[wooBar] = woo;
				prevWoo = woo;
			}

			foreach (KeyValuePair<int, string> entry in markers)
			{
				string woo = entry.Value;
				int idx = entry.Key;
				if (woo == "WOO")
				{
					DrawBarAnnotation("▲", entry.Key, false, Color.FromArgb(160, 0, 255, 0), 14, true);
				}
				else
				{
					DrawBarAnnotation("▲", entry.Key, false, Color.FromArgb(160, 255, 0, 0), 14, true);
				}
			}
			DrawHeaderText("▲ - WOO");
		}

		public static Image Resize(Image image, int newWidth)
		{
			// Using Image in WL7
			//Image imgGreen = Image.FromFile(@"C:\WLD\2018-LeeMP\w-green1.png");
			//Image imgRed = Image.FromFile(@"C:\WLD\2018-LeeMP\w-red1.jpeg");
			//System.Drawing.Image _imgR;
			//imgGreen = Resize(imgGreen, 10);
			//imgRed = Resize(imgRed, 10);
			//DrawImage(imgGreen, idx, bars.Low[idx]*0.97, "Price", 5);


			var newHeight = image.Height * newWidth / image.Width;

			var res = new Bitmap(newWidth, newHeight);

			using (var graphic = Graphics.FromImage(res))
			{
				graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphic.SmoothingMode = SmoothingMode.HighQuality;
				graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphic.CompositingQuality = CompositingQuality.HighQuality;
				graphic.DrawImage(image, 0, 0, newWidth, newHeight);
			}

			return res;
		}


		//===========================================================================================================
		// Show Trades
		//----------------------------------------------------------------------------------------------------------- 
		protected void showTrades(BarHistory bars, string tradeFile)
		{
			ClearAllAnnotations();

			if (bars.Symbol == this.settings.SP500 || bars.Symbol == this.settings.NASDAQ || bars.Symbol == "1EQUITY")
			{
				showTradesOnIndexChart(bars, tradeFile);
				return;
			}

			string[] lines = System.IO.File.ReadAllLines(tradeFile);
			foreach (string line in lines)
			{
				// 1/2/18,3/12/18,ABMD,B1,105,closed,191.28,290.53,"20,084","30,506","10,421",52%,50,5.19%
				string[] columns = line.Split(',');
				//PrintDebug(line);
				var symbol = columns[2];
				if (symbol == bars.Symbol)
				{
					var label = columns[3];

					// Buy 
					var buyDateStr = columns[0];
					if (buyDateStr != "")
					{
						DateTime d = DateTime.Parse(buyDateStr);
						int buyBar = bars.IndexOf(d, true);
						if (buyBar > 0)
						{
							double buyPrice = Double.Parse(columns[6]);
							DrawBarAnnotation2(label, buyBar, true);
							int nextBar = buyBar + 1;
							if (buyBar == bars.Count + bars.ExtendedBars - 1) nextBar = buyBar;  // Make sure the index is within the limit
							DrawLine(buyBar - 1, buyPrice, nextBar, buyPrice, Color.Orange, 3);

							// If it is B1, then draw the pivot point and 25% target
							if (label == "B1")
							{
								var pivotPrice = Highest.Value(buyBar, bars.High, 26 * 5);
								var pivotBar = (int)bars.High.GetHighestBar(buyBar, 26 * 5);
								var targetPrice = pivotPrice * 1.25;
								DrawLine(pivotBar, pivotPrice, bars.Count - 1, pivotPrice, Color.Orange, 1);
								DrawLine(pivotBar, targetPrice, bars.Count - 1, targetPrice, Color.Orange, 1);

								// Find breakout day
								var breakoutBar = -1;
								for (int i = pivotBar + 1; i < bars.Count - 1; i++)
								{
									if (bars.High[i] > pivotPrice)
									{
										breakoutBar = i;
										break;
									}
								}
								if (breakoutBar != -1)
								{
									SetBackgroundColor(bars, breakoutBar, Color.FromArgb(15, Color.Blue));
									var timeGoalBar = breakoutBar + 40;
									if (timeGoalBar < bars.Count)
									{
										SetBackgroundColor(bars, timeGoalBar, Color.FromArgb(15, Color.Blue));
									}
								}
							}
						}
					}

					// Sell 
					var sellDateStr = columns[1];
					if (sellDateStr != "")
					{
						DateTime d = DateTime.Parse(sellDateStr);
						int sellBar = bars.IndexOf(d, true);
						WriteToDebugLog("sellDateStr = " + sellDateStr);
						WriteToDebugLog("d = " + d);
						WriteToDebugLog("sellBar = " + sellBar);
						if (sellBar > 0)
						{
							double sellPrice = Double.Parse(columns[7]);
							DrawBarAnnotation2(label, sellBar, false);
							int nextBar = sellBar + 1;
							if (sellBar == bars.Count + bars.ExtendedBars - 1) nextBar = sellBar;  // Make sure the index is within the limit
							DrawLine(sellBar - 1, sellPrice, nextBar, sellPrice, Color.Brown, 3);
						}
					}
				}
			}

			DrawAllAnnotationsAtEnd(Color.FromArgb(255, 211, 84 ,0), Color.Brown, 14);
		}

		struct Slot
		{
			public int n;
			public string text;

			public Slot(int n, string text)
			{
				this.n = n;
				this.text = text;
			}
		}
		private Dictionary<int, List<Slot>> slotReservedMap = new Dictionary<int, List<Slot>>();
		protected void addTradeAnnotation(BarHistory bars, int bar, string text, int lines, int sideClearance)
		{
			int emptySlot = -1;
			int startBar = bar - sideClearance;
			int endBar = bar + sideClearance;
			if (startBar < 0) startBar = 0;
			if (endBar > bars.Count - 1) endBar = bars.Count - 1;
			HashSet<int> slotsOccupied = new HashSet<int>();
			for (int i = startBar; i <= endBar; i++)
			{
				if (!slotReservedMap.ContainsKey(i))
				{
					continue;
				}
				List<Slot> slotReserved = slotReservedMap[i];
				foreach (Slot slot in slotReserved)
				{
					slotsOccupied.Add(slot.n);
				}
			}
			for (int i = 0; i < 100; i++)
			{
				if (!slotsOccupied.Contains(i))
				{
					emptySlot = i;
					break;
				}
			}
			if (emptySlot != -1)
			{
				Slot newSlot = new Slot(emptySlot, text);
				List<Slot> slotReserved;
				if (slotReservedMap.ContainsKey(bar))
				{
					slotReserved = slotReservedMap[bar];
				}
				else
				{
					slotReserved = new List<Slot>();
				}
				slotReserved.Add(newSlot);
				slotReservedMap[bar] = slotReserved;
			}
		}

		protected void DrawAllTradeAnnotations()
		{
			foreach (KeyValuePair<int, List<Slot>> entry in slotReservedMap)
			{
				List<Slot> slots = entry.Value;
				StringBuilder sb = new StringBuilder();
				slots.Sort((x, y) => x.n.CompareTo(y.n));
				int idx = 0;  // Pointer in the slot
				for (int i = 0; i < 100; i++)
				{
					if (idx >= slots.Count)
						break;
					if (slots[idx].n == i)
					{
						sb.Append(slots[idx].text + "\n");
						idx++;
					}
					else
					{
						sb.Append(" \n \n");
					}
				}
				DrawBarAnnotation("\n" + sb.ToString() + "\n ", entry.Key, false, Color.Orange, 14);
			}
		}

		protected void showTradesOnIndexChart(BarHistory bars, string tradeFile)
		{
			string[] lines = System.IO.File.ReadAllLines(tradeFile);
			foreach (string line in lines)
			{
				// 1/2/18,3/12/18,ABMD,B1,105,closed,191.28,290.53,"20,084","30,506","10,421",52%,50,5.19%
				string[] columns = line.Split(',');
				//PrintDebug(line);
				var symbol = columns[2];
				var label = columns[3];
				if (label == "B1")
				{
					// Buy 
					var buyDateStr = columns[0];
					if (buyDateStr != "")
					{
						DateTime d = DateTime.Parse(buyDateStr);
						int buyBar = bars.IndexOf(d, true);
						if (buyBar > 0)
						{
							var gainPct = columns[11];
							//DrawBarAnnotation2(symbol + "\n" + gainPct, buyBar, false);
							addTradeAnnotation(bars, buyBar, symbol + "\n" + gainPct, 2, 2);

							// Draw a short line in the bar in the middle
							double buyPrice = (bars.High[buyBar] + bars.Low[buyBar]) / 2;
							int nextBar = buyBar + 1;
							if (buyBar == bars.Count + bars.ExtendedBars - 1) nextBar = buyBar;  // Make sure the index is within the limit
							DrawLine(buyBar - 1, buyPrice, nextBar, buyPrice, Color.Orange, 3);
						}
					}
				}
			}

			DrawAllTradeAnnotations();
		}

	}
}
