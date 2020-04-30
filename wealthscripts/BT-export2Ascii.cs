/**
 * Run this script on a DataSet using Back Testing.
 *
 */
public class Export2ASCII : WealthScript
{	
	protected override void Execute()
	{
		const string sep = ",";
		const string fmt = "0.00########";
		string dateFormat = "yyyyMMdd";
		string workingDir = @"Z:\Win10D";

		string today = DateTime.Now.ToString("yyyy-MM-dd");
		string path = Path.Combine(workingDir, "Data", today);
		
		if (!Directory.Exists(path))
		{
			PrintDebug("Creating directory: " + path); FlushDebug();
			Directory.CreateDirectory(path);
		}

		PrintStatusBar("Exporting: " + Bars.Symbol + "     " );
			
		string file = Path.Combine(path, Bars.Symbol + ".csv");
		
		List<string> datalist = new List<string>();
		
		for(int bar = 0; bar < Bars.Count; bar++)
		{			
			string csv = Date[bar].ToString(dateFormat) + sep 
				+ Open[bar].ToString(fmt) + sep 
				+ High[bar].ToString(fmt) + sep 
				+ Low[bar].ToString(fmt) + sep 
				+ Close[bar].ToString(fmt) + sep 
				+ Volume[bar].ToString("0");
			
			datalist.Add(csv);
		}
		File.WriteAllLines(file, datalist);
		
		Bars.Cache.Clear(); 
		//PrintStatusBar("Complete!");		
	}
}
