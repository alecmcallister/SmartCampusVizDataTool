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
	// Whether or not to strip personal information from the final output
	public bool Anonymize = true;

	// Indexer
	public Participant this[int i] { get { return Participants.ContainsKey(i) ? Participants[i] : null; } }

	/// <summary>
	/// Dictionary containing all participants (users) in the dataset
	/// </summary>
	public ConcurrentDictionary<int, Participant> Participants = new ConcurrentDictionary<int, Participant>();

	// Are we ready to perform calculations on the parsed datapoints?
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

		// Create new participants/ edit existing participants
		Parallel.ForEach(points, point =>
		{
			Participants.AddOrUpdate(point.userid, new Participant(point.userid), (i, p) => { p.AddDataPoint(point); return p; });
			Interlocked.Increment(ref ConsoleLog.Prog);
		});

		// Clean + sort each participants datapoints
		Parallel.ForEach(Participants, participant =>
		{
			participant.Value.CleanPoints();
		});

		ConsoleLog.LogStop();

		ReadyForCalc = true;
	}

	#region Output

	#region RestPoint output

	/// <summary>
	/// Calculates the participants restpoint groups, and returns them in a list ready to be written to a csv.
	/// </summary>
	/// <returns>The list of restpoint groups</returns>
	public List<RestpointOutput> GetRestpointOutput()
	{
		if (!ReadyForCalc)
			return null;

		ConsoleLog.LogStart("Calculating RestPoint output...");
		ConsoleLog.LogProgress(Participants.Count);

		ConcurrentBag<RestpointOutput> output = new ConcurrentBag<RestpointOutput>();

		// Calculate each participants restpoints
		Parallel.ForEach(Participants.Values, participant =>
		{
			participant.CalculateRestPoints();

			participant.RestPoints.ForEach(sp =>
			{
				sp.GetOutput().ForEach(spo =>
				{
					output.Add(spo);
				});
			});

			Interlocked.Increment(ref ConsoleLog.Prog);
		});

		ConsoleLog.LogStop();

		List<RestpointOutput> sorted = output.ToList();
		sorted.Sort();

		ReAssignIDs(sorted);

		return sorted;
	}

	/// <summary>
	/// Gets the anonymized version of the restpoint output
	/// </summary>
	/// <returns>Anonymized restpoint output</returns>
	public List<RestpointOutputBase> GetAnonRestpointOutput()
	{
		List<RestpointOutput> output = GetRestpointOutput();

		if (output == null)
			return null;

		return AnonymizeRestpointOutput(output);
	}

	/// <summary>
	/// Reassigns IDs so that they are each 1 away. Called after some IDs have been removed from the list.
	/// </summary>
	/// <param name="output">The same list, except with proper IDs</param>
	void ReAssignIDs(List<RestpointOutput> output)
	{
		int id = 0;
		int stayID = 0;
		int x = 0;
		int y = 0;

		foreach (RestpointOutput g in output)
		{
			if (g.UserID != id)
			{
				id = g.UserID;
				stayID = g.RestpointID;
				x = 0;
				y = 0;
			}
			else
			{
				if (g.RestpointID != stayID)
				{
					stayID = g.RestpointID;
					x++;
					y = 0;
				}
				else
				{
					y++;
				}
			}

			g.RestpointID = x;
			g.RestpointGroupID = y;
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

		// Calculate each participants paths
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

	/// <summary>
	/// Gets the anonymized version of the path output
	/// </summary>
	/// <returns>Anonymized path output</returns>
	public List<PathOutput_Base> GetAnonPathOutput()
	{
		List<PathOutput> output = GetPathOutput();

		if (output == null)
			return null;

		return AnonymizePathOutput(output);
	}

	#endregion

	#endregion

	#region Anonymization

	/// <summary>
	/// Anonymize the restpoint output
	/// </summary>
	/// <param name="list">The output that needs anonymization</param>
	/// <returns>Anonymized respoint output</returns>
	List<RestpointOutputBase> AnonymizeRestpointOutput(List<RestpointOutput> list)
	{
		List<RestpointOutputBase> anon = list.Select(sp => (RestpointOutputBase)sp).ToList();
		anon.Sort();
		return anon;
	}

	/// <summary>
	/// Anonymize the path output
	/// </summary>
	/// <param name="list">The output that needs anonymization</param>
	/// <returns>Anonymized path output</returns>
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

	/// <summary>
	/// Used in anonymization. Groups paths together so we don't need to reference their UserID
	/// </summary>
	/// <param name="list">The user's paths</param>
	/// <returns>The grouped paths</returns>
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

	#endregion

}
