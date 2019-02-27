﻿using System;
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
	public bool Anonymize = true;

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
	public List<StaypointOutput> GetStaypointOutput()
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

	public List<StaypointOutput_Anon> GetAnonStaypointOutput()
	{
		List<StaypointOutput> output = GetStaypointOutput();

		if (output == null)
			return null;

		return AnonymizeStaypointOutput(output);
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

	#region Community StayPoint output

	public List<CommunityStaypointOutput> GetCommunityStayPointOutput()
	{
		if (!ReadyForCalc)
			return null;

		List<CommunityStaypointOutput> output = new List<CommunityStaypointOutput>();

		List<Staypoint> staypoints = new List<Staypoint>();

		Participants.Values.ToList().ForEach(p => { staypoints.AddRange(p.StayPoints); });

		List<List<Staypoint>> grouped = Staypoint.GroupByDate(staypoints);

		foreach (List<Staypoint> group in grouped)
		{
			CommunityStaypoint csp = new CommunityStaypoint(group[0]);

			// Separated by date

			bool flag = false;

			foreach (Staypoint sp in group)
				if (flag |= csp.ConditionalAddStaypoint(sp))
					break;

			if (flag)
			{

			}
		}

		return output;
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

		Parallel.ForEach(Participants, kvp =>
		{
			Participant participant = kvp.Value;
			participant.CalculatePaths();

			participant.Paths.ForEach(p =>
			{
				p.GetOutput().ForEach(ppo =>
				{
					output.Add(ppo);
				});
			});

			Interlocked.Increment(ref ConsoleLog.Prog);
		});

		ConsoleLog.LogStop();

		List<PathOutput> sorted = output.ToList();
		sorted.Sort();
		return sorted;
	}

	public List<PathOutput_Anon> GetAnonPathOutput()
	{
		List<PathOutput> output = GetPathOutput();

		if (output == null)
			return null;

		return AnonymizePathOutput(output);
	}

	#endregion

	#endregion

	List<StaypointOutput_Anon> AnonymizeStaypointOutput(List<StaypointOutput> list)
	{
		list.Sort(Comparer<StaypointOutput>.Create((spo1, spo2) => { return spo1.StartDate.CompareTo(spo2.StartDate); }));
		
		return list.Select(sp => new StaypointOutput_Anon(sp)).ToList();
	}

	List<PathOutput_Anon> AnonymizePathOutput(List<PathOutput> list)
	{
		List<List<PathOutput>> grouped = GroupPaths(list);
		grouped.Sort(Comparer<List<PathOutput>>.Create((po1, po2) => { return po1[0].Date.CompareTo(po2[0].Date); }));
		List<PathOutput> output = grouped.SelectMany(g => g).ToList();

		for (int i = 0, id = -1; i < output.Count; i++)
		{
			PathOutput po1 = output[i];

			if (po1.PathPointID == 0)
				id++;

			po1.PathID = id;
			po1.UserID = 0;
		}

		return output.Select(po => new PathOutput_Anon(po)).ToList();
	}

	List<List<PathOutput>> GroupPaths(List<PathOutput> list)
	{
		List<List<PathOutput>> output = new List<List<PathOutput>>();

		List<PathOutput> group = new List<PathOutput>();

		for (int i = 0; i < list.Count; i++)
		{
			PathOutput spo1 = list[i];

			if (spo1.PathPointID == 0 && group.Count > 0)
			{
				output.Add(group.ToList());
				group.Clear();
			}

			group.Add(spo1);
		}

		output.Add(group.ToList()); // Add the last path

		return output;
	}
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