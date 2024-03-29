(*Description...
<b>test</b>

*)
{$NO_AUTO_EXECUTE}   ChangeScale(#Weekly);
//============== Invoke CSharp Script and calculate all indiciators  =====================
var csScripting: ComVariant = CreateOleObject( 'WLE.CSharp' );
var myclass: ComVariant = csScripting.LoadFile ( 'CSharpScripts\ScreenOneWeekly.cs' );
if ( AsString(myclass) = '' ) then Abort;
myclass.ScreenOneWeeklyInit( IWealthLabAddOn3 );
//==============================================================================
var mainIndustry, mainIndustrySymbol, subIndustry, subIndustrySymbol, market, marketName: string;
myclass.GetIndustryGroupInfo ( GetSymbol, MainIndustry, MainIndustrySymbol, SubIndustry, SubIndustrySymbol );
myclass.GetMarketSymbol ( GetSymbol, market, marketName );

var bar: integer; var myDrawLabelX: integer = 38;
//______________________________________________________________________________
procedure PlotSecondarySeries( paneHeight: integer; symbol: string; title: string );
begin
  SetPrimarySeries( symbol ); myclass.SetPrimarySeries ( true );

  var emaFastSeries: integer = CreateSeries;
  var emaSlowSeries: integer = CreateSeries;
  var upperSeries: integer = CreateSeries;
  var lowerSeries: integer = CreateSeries;
  var greenHigh: integer = CreateSeries;
  var greenOpen: integer = CreateSeries;
  var greenClose: integer = CreateSeries;
  var greenLow: integer = CreateSeries;
  var mmacd: integer = CreateSeries;
  var macdSignal: integer = CreateSeries;
  var macdh: integer = CreateSeries;
  var macdhColor: integer = CreateSeries;
  var macdOffset: float = 0;
  var impulseStr: string;
  
  myclass.ScreenOneWeekly_GetAllSeries2( emaFastSeries, emaSlowSeries, upperSeries, lowerSeries,
    greenOpen, greenHigh, greenLow, greenClose, mmacd, macdSignal, macdh, macdhColor, macdOffset,
    impulseStr );

  var marketPane: integer = CreatePane( paneHeight, false, true );
  PlotSeries( mmacd, marketPane, 900, #Thick );
  PlotSeries( macdSignal, marketPane, 009, #Thin );
  DrawHorzLine( macdOffset, marketPane, 550, #Thin );
  for bar := 1 to BarCount-1 do
    DrawLine( bar, @macdh[bar], bar, macdOffset, marketPane, AsInteger(@macdhColor[bar]), #Thin );

  PlotSymbol( GetSymbol, marketPane, 600, #Candle );
  PlotSyntheticSymbol( GetSymbol, greenOpen, greenHigh, greenLow, greenClose, marketPane, 040, #Candle );
  PlotSeries( emaFastSeries, marketPane, 777, #Thin );
  PlotSeries( emaSlowSeries, marketPane, #Maroon, #Thin );
  PlotSeries( upperSeries, marketPane, #Teal, #thin );
  PlotSeries( lowerSeries, marketPane, #Teal, #thin );

  DrawLabel( ' ', marketPane );
  DrawLabel( ' ', marketPane );
  DrawText( title, marketPane, 5, 5, #Black, 10 );
  DrawLabel( 'Impulse: ' + impulseStr, marketPane );
  DrawLabel ('ChannelWidth (' + FormatFloat('#', ( 100 * (@upperSeries[BarCount-1]-@lowerSeries[BarCount-1]) / @emaSlowSeries[BarCount-1] ) ) + '%): '
       + FormatFloat('#.##', @upperSeries[BarCount-1]-@lowerSeries[BarCount-1]), marketPane);

  RestorePrimarySeries(); myclass.RestorePrimarySeries ( );
end;
//______________________________________________________________________________



//---------Plot in Primary Pane ---- EMA and Channel ---------------------------

var slowEmaSeries: integer = CreateSeries;
var mainEmaSeries: integer = CreateSeries;
var upperChannelSeries: integer = CreateSeries;
var lowerChannelSeries: integer = CreateSeries;
var channelStr: string;

myclass.ScreenOneWeekly_GetBasicSeries( slowEmaSeries, mainEmaSeries, upperChannelSeries, lowerChannelSeries, channelStr );

SingleCalcMode(true); AnnotateBar( 'H', HighestBar( BarCount-1, #High, BarCount ), true, 742, 10 ); SingleCalcMode(false);
DrawText( SubIndustry, 0, 130, 20, 000, 10 );
DrawText( myclass.BasicFundamentalString( GetSymbol ),0, 7, myDrawLabelX, 000, 10 ); myDrawLabelX := myDrawLabelX + 15;
PlotSeries( slowEmaSeries, 0, 777, #Thin );
PlotSeries( mainEMASeries, 0, #Maroon, #Thin );
PlotSeries( upperChannelSeries, 0, #Teal, #thin );
PlotSeries( lowerChannelSeries, 0, #Teal, #thin );
DrawText( channelStr, 0, 7, myDrawLabelX, 000, 10 ); myDrawLabelX := myDrawLabelX + 15;
DrawText( myclass.MembershipString( GetSymbol ),0, 7, myDrawLabelX, 000, 10 ); myDrawLabelX := myDrawLabelX + 15;

//------------------------- Earning Dates --------------------------------------
var EarningSeries: integer = CreateSeries;
myclass.GetEarningSeriesWeekly ( GetSymbol, EarningSeries );
for bar := 0 to BarCount-1 do
  if ( @EarningSeries[bar] > 0.5 )  then
    DrawImage( 'UpArrow', 0, bar, PriceLow(bar)*0.99, true);

//----------------------------- Volume -----------------------------------------
//HideVolume();
for Bar:=0 to BarCount-1 do if PriceClose(Bar) > PriceOpen(Bar) then SetSeriesBarColor(Bar,#Volume,#Green) else SetSeriesBarColor(Bar,#Volume,#Red);
{var volumePane: integer = CreatePane( 40, false, true );
var VolumeSeries2: integer = CreateSeries; @VolumeSeries2[BarCount-1] := 0;
PlotSeries( VolumeSeries2, volumePane, #Black, #Histogram );
PlotSeriesLabel( #Volume, volumePane, #Black, #ThickHist,'Volume' );
}
//---------------------------- Plot MACD ---------------------------------------
var mmacd : integer = CreateSeries;
var mmacdSignal : integer = CreateSeries;
var mmacdh : integer = CreateSeries;
var mmacdhColor : integer = CreateSeries;
var impulseStr: string;
var IsBullishDev, IsBearishDiv: boolean;
var BullBar1, BullBar2, BearBar1, BearBar2: integer;

myclass.GetMacdSeriesWithDiv ( mmacd, mmacdSignal, mmacdh, mmacdhColor, impulseStr,
         IsBullishDev, BullBar1, BullBar2, IsBearishDiv, BearBar1, BearBar2 );

var macdPane : integer = CreatePane( 40, false, true );
PlotSeriesLabel( mmacd, macdPane, #Red, #Thick, 'MACD' );
PlotSeriesLabel( MMacdSignal, macdPane, #Blue, #Thin, 'MACD Signal' );
DrawHorzLine( 0, macdPane, #Olive, #Thin );
for bar := 1 to BarCount-1 do SetSeriesBarColor( bar, mmacdh, AsInteger(@mmacdhColor[bar]) );
PlotSeries( mmacdh, macdPane, #Black, #Histogram);
DrawLabel( 'Impulse: ' + impulseStr, macdPane );
if ( IsBullishDev ) then
begin
 DrawLine( BullBar1, PriceLow(BullBar1)*0.98, BullBar2, PriceLow(BullBar2)*0.98, 0, #Aqua, #Thick );
 DrawLine( BullBar1, @mmacdh[BullBar1]*0.98, BullBar2, @mmacdh[BullBar2]*0.98, MacdPane,  #Aqua, #Thick );
 DrawText( 'Bullish Divergence', 0, 7, myDrawLabelX, 009, 10 ); myDrawLabelX := myDrawLabelX + 15;
end;
if ( IsBearishDiv ) then
begin
 DrawLine( BearBar1, PriceHigh(BearBar1)*1.02, BearBar2, PriceHigh(BearBar2)*1.02, 0, #Aqua, #Thick );
 DrawLine( BearBar1, @mmacdh[BearBar1]*1.02, BearBar2, @mmacdh[BearBar2]*1.02, MacdPane,  #Aqua, #Thick );
 DrawText( 'Bearish Divergence', 0, 7, myDrawLabelX, 009, 10 ); myDrawLabelX := myDrawLabelX + 15;
end;

//---------------------------- Other Plottings ----------------------------------
// Industry
PlotSecondarySeries( 150, subIndustrySymbol, subIndustrySymbol + ' || ' + mainIndustry + ' || ' + subIndustry );

// Market
PlotSecondarySeries( 150, market, market + ' (' + marketName + ') Weekly' );

//-------------------------- Print Position ------------------------------------
var EntryBar, EntryColor, ExitBar, ExitColor: integer;
myclass.PrintClosedPosition ( GetSymbol, IsWeekly, EntryBar, EntryColor, ExitBar, ExitColor );
SetBackgroundColor( EntryBar, EntryColor );
SetBackgroundColor( ExitBar, ExitColor );

