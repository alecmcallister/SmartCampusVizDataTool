using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Class for managing all participants (users) in the dataset
/// </summary>
public class ParticipantManager
{
	/// <summary>
	/// Dictionary containing all participants (users) in the dataset
	/// </summary>
	//public SortedDictionary<int, Participant> Participants = new SortedDictionary<int, Participant>();
	public Participant this[int i] { get { return Participants.ContainsKey(i) ? Participants[i] : null; } }

	public ConcurrentDictionary<int, Participant> Participants = new ConcurrentDictionary<int, Participant>();

	public bool ReadyForCalc;

	/// <summary>
	/// Takes a list of <see cref="DataPoint"/> objects, instantiate a <see cref="Participant"/> for each of the unique 
	/// <see cref="DataPoint.userid"/> values seen, and assigns each datapoint to the corresponding participant.
	/// </summary>
	/// <param name="points">All points read/ converted from the csv input file</param>
	public void AddDataPoints(List<DataPoint> points)
	{
		ReadyForCalc = false;

		Participants.Clear();

		ConsoleLog.LogStart("Adding DataPoints...");
		ConsoleLog.LogProgress(points.Count);

		Parallel.ForEach(points, point =>
		{
			Participants.AddOrUpdate(point.userid, new Participant(point.userid), (i, p) => { p.AddDataPoint(point); return p; });
			Interlocked.Increment(ref ConsoleLog.Prog);
		});

		ConsoleLog.LogStop();

		ReadyForCalc = true;
	}

	#region Output

	#region StayPoint output

	/// <summary>
	/// Calculates the participants staypoint groups, and returns them in a list ready to be written to a csv.
	/// </summary>
	/// <returns>The list of staypoint groups</returns>
	public List<StaypointOutput> GetStayPointOutput()
	{
		if (!ReadyForCalc)
			return null;

		ConsoleLog.LogStart("Calculating StayPoint output...");
		ConsoleLog.LogProgress(Participants.Count);

		ConcurrentBag<StaypointOutput> output = new ConcurrentBag<StaypointOutput>();

		Parallel.ForEach(Participants.Values, participant =>
		{
			participant.CalculateStayPoints();

			participant.StayPoints.ForEach(sp =>
			{
				sp.GetOutput().ForEach(spo =>
				{
					output.Add(spo);
				});
			});

			Interlocked.Increment(ref ConsoleLog.Prog);
		});

		ConsoleLog.LogStop();

		List<StaypointOutput> sorted = output.ToList();
		sorted.Sort();

		ReAssignIDs(sorted);

		return sorted;
	}

	void ReAssignIDs(List<StaypointOutput> output)
	{
		int id = 0;

		int stayID = 0;

		int x = 0;
		int y = 0;

		foreach (StaypointOutput g in output)
		{
			if (g.UserID != id)
			{
				id = g.UserID;
				stayID = g.StaypointID;
				x = 0;
				y = 0;
			}
			else
			{
				if (g.StaypointID != stayID)
				{
					stayID = g.StaypointID;
					x++;
					y = 0;
				}
				else
				{
					y++;
				}
			}

			g.StaypointID = x;
			g.StaypointGroupID = y;
		}
	}

	#endregion

	#region Path output

	/// <summary>
	/// Calculates the participants paths, and returns them in a list ready to be written to a csv.
	/// </summary>
	/// <returns>The list of paths</returns>
	public List<PathOutput> GetPathOutput()
	{
		if (!ReadyForCalc)
			return null;

		ConsoleLog.LogStart("Calculating Path output...");
		ConsoleLog.LogProgress(Participants.Count);

		ConcurrentBag<PathOutput> output = new ConcurrentBag<PathOutput>();

		Parallel.ForEach(Participants.Values, participant =>
		{
			participant.CalculatePaths();

			if (participant.Paths.Count >= Affectors.Instance.Path_MinPaths)
			{
				participant.Paths.ForEach(p =>
				{
					p.GetOutput().ForEach(ppo =>
					{
						output.Add(ppo);
					});
				});
			}

			Interlocked.Increment(ref ConsoleLog.Prog);
		});

		ConsoleLog.LogStop();

		List<PathOutput> sorted = output.ToList();
		sorted.Sort();
		return sorted;
	}

	#endregion

	#endregion

	#region Misc participant factoids

	public Participant MostEntries()
	{
		return Participants.Values.Aggregate((p1, p2) => p1.Points.Count > p2.Points.Count ? p1 : p2);
	}

	public Participant MostStayPoints()
	{
		return Participants.Values.Aggregate((p1, p2) => p1.StayPoints.Count > p2.StayPoints.Count ? p1 : p2);
	}
	public Participant MostPaths()
	{
		return Participants.Values.Aggregate((p1, p2) => p1.Paths.Count > p2.Paths.Count ? p1 : p2);
	}

	#endregion

}

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