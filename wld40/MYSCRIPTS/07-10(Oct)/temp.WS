{$NO_AUTO_EXECUTE}


var bar, s1, s2, s3, s4: integer;

s1 := SMASeries ( #High, 20 );
s2 := SMASeries ( #High, 19 );

@s1[BarCount-1] := 5;
@s2[BarCount-1] := 1000000;

s3 := SMASeries ( s1, 1 );
s4 := SMASeries ( s2, 1 );

Print ( AsString(@s3[BarCount-1]) );
Print ( AsString(@s4[BarCount-1]) );


