
/*[SCRIPT]
#AddReference Interop.WealthLab.dll
#AddReference WLE.dll
[/SCRIPT]*/




using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;

using WealthLab;


namespace MyScript
{
    partial class MyClass
    {
		//____________________________________________________________________________________________
		/// <summary>
		/// Method to test this libary.
		/// </summary>
		//--------------------------------------------------------------------------------------------
		public void FundamentalTest ( )
		{
			try
			{
				MessageBox.Show ( "Fundamental is working. CurrDir = " + Environment.CurrentDirectory, "Fundamental" );
			}
			catch ( Exception e )
			{
				MessageBox.Show ( e.Message + "\r\n" + e.StackTrace );
			}

		}
		//____________________________________________________________________________________________
		// Returns a string which contains basic fundamental information
		//--------------------------------------------------------------------------------------------
		public string BasicFundamentalString ( string Symbol )
		{
			try
			{
				long MarketCap = 0L, Shares = 0L;
				string MarketCapStr = "", SharesStr = "";

				GetKeyStats ( Symbol, ref MarketCap, ref Shares, ref MarketCapStr, ref SharesStr );

				string retStr = String.Format ( "MCap:  {0}      OShares:  {1}",
					MarketCapStr, SharesStr );

				// Now try to get group info
				string Sector = "", SectorSymbol = "", Industry = "", IndustrySymbol = "";
				GetIndustryGroupInfo ( Symbol, ref Sector, ref SectorSymbol,
					ref Industry, ref IndustrySymbol );
				if ( Regex.IsMatch ( IndustrySymbol, @"MG\d{3}" ) )
				{
					long GMarketCap = 0L, GShares = 0L;
					string GMarketCapStr = "", GSharesStr = "";

					if ( GetKeyStats ( IndustrySymbol, ref GMarketCap, ref GShares, ref GMarketCapStr,
						ref GSharesStr ) && GMarketCap != 0 )
					{
						double MarketCapPer = (double) 100 * MarketCap / GMarketCap;
						retStr = String.Format ( "MCap:  {0} ({1:G2}% of {2})     OShares:  {3}",
									MarketCapStr, MarketCapPer, GMarketCapStr, SharesStr );

					}
				}

				return retStr;
			}
			catch ( Exception )
			{
			}
			return "";
		}
		//____________________________________________________________________________________________
		// Returns key stats
		//--------------------------------------------------------------------------------------------
		public bool GetKeyStats ( string Symbol, ref long MarketCapM, ref long OutStandingSharesM, 
			ref string MarketCapStr, ref string OutStandingSharesStr  )
		{
			MarketCapM = OutStandingSharesM = 0;
			MarketCapStr = OutStandingSharesStr = "N/A";
			try
			{
				string FileName = RootDir + @"\Fundamentals\KeyStats\AllStats\" + Symbol + ".csv";
				if ( !File.Exists ( FileName ) )
				{
					return false;
				}

				StreamReader sr = new StreamReader ( FileName );

				//MarketCapitalization,7.4B
				//SharesOutstanding,137M
				
				MarketCapStr = GetToken ( sr.ReadLine ( ), 1, "," );
				OutStandingSharesStr = GetToken ( sr.ReadLine(), 1, "," );
				
				MarketCapM = HumanReadableToLong ( MarketCapStr ) / 1000000L;
				OutStandingSharesM = HumanReadableToLong ( OutStandingSharesStr ) / 1000000L;

				sr.Close ( );
				return true;
			}
			catch ( Exception )
			{
			}
			return false;
		}
		//____________________________________________________________________________________________
		// Converts Human Readable to Long
		//--------------------------------------------------------------------------------------------
		public long HumanReadableToLong ( string NumStr )
		{
			long RetNum = 0;
			Match m = Regex.Match ( NumStr, @"(?<num>\d+\.?\d*)(?<suffix>[MB]?)" );
			if ( m.Success )
			{
				double num = Double.Parse ( m.Groups["num"].ToString ( ) );
				string suffix = m.Groups["suffix"].ToString ( );
				switch ( suffix )
				{
					case "": RetNum = (long) Math.Round ( num ); Debugger.Break ( ); break;
					case "B": RetNum = (long) Math.Round ( num * 1000000000L ); break;
					case "M": RetNum = (long) Math.Round ( num * 1000000L ); break;
					default:
						MessageBox.Show ( "Problem occured" );
						Debugger.Break ( ); break;
				}
			}
			else
			{
				MessageBox.Show ( "Problem occured" );
				Debugger.Break ( );
			}
			return RetNum;
		}
		//____________________________________________________________________________________________
		// Converts from Long To Human Readable
		//--------------------------------------------------------------------------------------------
		public string LongToHumanReadable ( long Num )
		{
			string ret = "";
			long Millions = Num / 1000000L;
			if ( Millions >= 1000L )
			{
				double Billions = (double) Millions / 1000.0;
				if ( Billions >= 10 )
				{
					ret = String.Format ( "{0:f0}B", Billions );
				}
				else
				{
					ret = String.Format ( "{0:f1}B", Billions );
				}
			}
			else
			{
				ret = String.Format ( "{0}M", Millions );
			}
			return ret;
		}
		//____________________________________________________________________________________________
		// Returns a string which contains basic membership information
		//--------------------------------------------------------------------------------------------
		public string MembershipString ( string Symbol )
		{
			try
			{
				using ( StreamReader sr = new StreamReader ( RootDir + @"\Fundamentals\Membership\Members\" + Symbol + ".csv" ) ) 
				{
					string line = sr.ReadLine ( );
					sr.Close ( );

					if ( line != null && line.Trim ( ) != "" )
					{
						line = line.Substring ( 0, line.Length - 1 );
						line = "Membership: " + line;
					}
					else
					{
						line = "";
					}

					return line;
				}
			}
			catch ( Exception )
			{
			}
			return "";
		}
		//____________________________________________________________________________________________
		// Returns Industry group information.
		//--------------------------------------------------------------------------------------------
		public void GetIndustryGroupInfo ( string Symbol, ref string MainIndustry,
			ref string MainIndustrySymbol, ref string SubIndustry, ref string SubIndustrySymbol )
		{
			MainIndustry = "";
			MainIndustrySymbol = "";
			SubIndustry = "";
			SubIndustrySymbol = "";
			try
			{
				try
				{
					using ( StreamReader sr = new StreamReader ( RootDir
						+ @"\Fundamentals\industryGroups\Telechart\" + Symbol + ".csv" ) )
					{
						MainIndustry = sr.ReadLine ( );
						MainIndustrySymbol = sr.ReadLine ( );
						SubIndustry = sr.ReadLine ( );
						SubIndustrySymbol = sr.ReadLine ( );
						sr.Close ( );
					}
				}
				catch ( Exception )
				{
				}

				// For indexes and for new symbols
				if ( SubIndustry == "" )
				{
					// Set SP-500 as the default subindustry

					MainIndustry = "Standard & Poors 500";
					MainIndustrySymbol = "SP-500";
					SubIndustry = "Standard & Poors 500";
					SubIndustrySymbol = "SP-500";
					
					if ( Symbol == "SP-500" )
					{
						MainIndustry = "Dow Jones Industrials";
						MainIndustrySymbol = "DJ-30";
						SubIndustry = "Dow Jones Industrials";
						SubIndustrySymbol = "DJ-30";
					}
				}

				// For possible industry group as symbols
				if( Symbol.Length == 5 && Regex.IsMatch ( Symbol, @"MG\d\d\d" ) )
				{
					int industryNum = Int32.Parse ( Symbol.Substring ( 2, 3 ) );
					if( industryNum % 10 == 0 )
					{
						// This is main industry group, plot SP-500
						MainIndustry = "Standard & Poors 500";
						MainIndustrySymbol = "SP-500";
						SubIndustry = "Standard & Poors 500";
						SubIndustrySymbol = "SP-500";
					}
					else
					{
						// This is sub-industry group, plot main industry group
						SubIndustry = MainIndustry;
						SubIndustrySymbol = MainIndustrySymbol;
					}
				}
			}
			catch ( Exception )
			{
			}
		}
		//____________________________________________________________________________________________
		// Returns Market Symbol
		//--------------------------------------------------------------------------------------------
		public void GetMarketSymbol ( string Symbol, ref string MarketSymbol, ref string MarketName )
		{
			MarketSymbol = "SP-500";
			MarketName = "Standard & Poors 500";
			if ( Symbol == "DJ-30"  || Symbol == "SP-500")
			{
				MarketSymbol = "COMPQX";
				MarketName = "NASDAQ Composite";
			}
			else if ( Symbol == "COMPQX" )
			{
				MarketSymbol = "DJ-30";
				MarketName = "Dow Jones Industrial Average";
			}			
		}

		//____________________________________________________________________________________________
		//
		//--------------------------------------------------------------------------------------------
	}
}

