using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
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

		Parallel.ForEach(Participants, participant =>
		{
			participant.Value.CleanPoints();
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

	public List<StaypointOutputBase> GetAnonStaypointOutput()
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

	public List<PathOutput_Base> GetAnonPathOutput()
	{
		List<PathOutput> output = GetPathOutput();

		if (output == null)
			return null;

		return AnonymizePathOutput(output);
	}

	#endregion

	#endregion

	List<StaypointOutputBase> AnonymizeStaypointOutput(List<StaypointOutput> list)
	{
		//list.Sort(Comparer<StaypointOutput>.Create((spo1, spo2) => { return spo1.StartDate.CompareTo(spo2.StartDate); }));
		List<StaypointOutputBase> anon = list.Select(sp => (StaypointOutputBase)sp).ToList();
		anon.Sort();
		return anon;
	}

	List<PathOutput_Base> AnonymizePathOutput(List<PathOutput> list)
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

		return output.Select(po => (PathOutput_Base)po).ToList();
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
