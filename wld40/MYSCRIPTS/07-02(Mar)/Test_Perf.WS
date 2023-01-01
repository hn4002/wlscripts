(*Description...
<html><b>mytext</b>
<i>italic</i>

*)

var count, bar, myema: integer;
var total : float = 0;

myema := EMASeries(#Close,5);
//myema := CreateSeries();
@myema[0] := 11;
@myema[1] := 12;
@myema[2] := 13;

var csScripting: ComVariant = CreateOleObject( 'WLExtra.Scripting' );
var compilerRes: integer = csScripting.CompileFile ( 'CsScripts/Test_Perf.cs' );
if ( compilerRes = -1 ) then
  exit;
Print ( 'CompilerResult = ' + IntToStr(compilerRes) );  PrintFlush;
var instanceId: integer = csScripting.CreateInstance ( 'MyScript.MyClass' );
csScripting.RunMethod2 ( instanceId, 'Init', IWealthLabAddOn3, BarCount );

var tick1: integer = GetTickCount;

    csScripting.RunMethod1 ( instanceId, 'SaveClosePrices', myema );

Print ( 'TickCount = ' + IntToStr ( GetTickCount - tick1 ) ); PrintFlush;
Print ( 'myema[0] = ' + FloatToStr ( @myema[0] ) );
Print ( 'myema[1] = ' + FloatToStr ( @myema[1] ) );
Print ( 'myema[2] = ' + FloatToStr ( @myema[2] ) );
Print ( 'myema[BarCount-1] = ' + FloatToStr ( @myema[BarCount-1] ) );

//csScripting.RunMethod0 ( instanceId, 'Verify' );




