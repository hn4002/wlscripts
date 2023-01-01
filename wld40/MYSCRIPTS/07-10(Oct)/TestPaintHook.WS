{$NO_AUTO_EXECUTE}
UseUpdatedEma( true );
EnableSynch( false );

var tick1 : integer = GetTickCount;

var csScripting: ComVariant = CreateOleObject( 'WLE.CSharp' );
var myclass : ComVariant = csScripting.LoadFile ( 'CSharpScripts\Test.cs' );
Print ( 'myclass = #' + AsString(myclass) + '$' );  PrintFlush;
if ( AsString(myclass) = '' ) then
  exit;

Print( 'Time = ' + IntToStr(GetTickCount() - tick1) + ' ms' );


//myclass.TestTest();

myclass.RegisterPaintHook ( IWealthLabAddOn3 );

DrawLabel ( 'Testing_this_label', 0 );


