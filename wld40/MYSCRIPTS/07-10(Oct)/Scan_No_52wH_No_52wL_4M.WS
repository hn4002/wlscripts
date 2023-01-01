{$NO_AUTO_EXECUTE}
UseUpdatedEma( true );
EnableSynch( false );

///////////////////////////////////////////////////////////////////////////////
function readWatchList(): TList;
begin
  var watchListNum: integer;
  var list : TList;

  list := TList.Create;
  for watchListNum := 0 to WatchListCount - 1 do
  begin
    list.Add( WatchListSymbol (watchListNum) );
  end;
  Result := list;
end;


///////////////////////////////////////////////////////////////////////////////

var list : TList;

list := readWatchList();

  var num: integer;
  var newList: TList = TList.Create;

  // Clear the new watch list if it exists
  WatchListClear (newWatchList);

  for num := 0 to list.Count-1 do
  begin
    PrintStatus ('PriceVolumeBarsRequirement: ' + IntToStr(num) +
        '/' + IntToStr(list.Count-1) + ' ' +  list.Item( num ));

    // Read the chart table for the symbol
    try
      SetPrimarySeries (list.Item( num ) );

      if( BarCount < minBars ) then
        continue;

      if( PriceClose( BarCount-1 ) < minPrice ) then
        continue;

      if( ( SMA( BarCount-1, #Volume, 22 ) < minVolume )
          or ( Volume( BarCount-1 ) < minVolume )
          ) then
        continue;

      WatchListAddSymbol (newWatchList, '', GetSymbol);
      newList.Add( GetSymbol );
    except
      continue;
    end;
  end;
  RestorePrimarySeries();
  ClearIndicators();

