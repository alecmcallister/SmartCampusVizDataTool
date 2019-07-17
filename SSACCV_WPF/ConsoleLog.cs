using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ConsoleLog
{

	#region Console Log

	static DateTime LogStartTime;
	static Task Log;

	public static void LogStart(string message)
	{
		LogStartTime = DateTime.Now;
		Console.WriteLine(message);
		Log = null;
	}

	public static void LogStop()
	{
		if (Log != null)
			Log.Wait();

		Log = null;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine((Console.CursorLeft != 0 ? "\n" : "") + "\tComplete");
		Console.ForegroundColor = ConsoleColor.DarkGreen;
		TimeSpan timeDiff = DateTime.Now - LogStartTime;
		int timeDiffMinutes = timeDiff.Minutes;
		double timeDiffSeconds = timeDiffMinutes > 0 ? timeDiff.Seconds : timeDiff.TotalSeconds;
		string minComp = timeDiffMinutes > 0 ? string.Format("\t{0}m ", timeDiffMinutes) : "\t";
		string totalComp = minComp + string.Format("{0:0.00}s\n", timeDiffSeconds);
		Console.WriteLine(totalComp);
		Console.ResetColor();
	}

	#region Progress

	public static int Prog;
	public static int Max;

	public static void LogProgress(int max)
	{
		Prog = 0;
		Max = max;

		Log = Task.Run(() =>
		{
			while (Prog < Max)
				WriteProgress(Prog, Max);

			WriteProgress(Prog, Max);
		});
	}

	#endregion

	#region Pretty Colors

	public static void WriteProgress(int prog, int max)
	{
		Console.CursorLeft = 0;
		Console.Write("(");

		Console.ForegroundColor = prog < max ? ConsoleColor.Yellow : ConsoleColor.Green;
		Console.Write("{0}", prog);
		Console.ResetColor();

		Console.Write("/{0})", max);
	}

	#endregion

	#endregion

}