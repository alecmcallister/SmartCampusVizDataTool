using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class for managing all participants (users) in the dataset
/// </summary>
public class ParticipantManager
{
	/// <summary>
	/// Dictionary containing all participants (users) in the dataset
	/// </summary>
	public SortedDictionary<int, Participant> Participants = new SortedDictionary<int, Participant>();
	public Participant this[int i] { get { return Participants.ContainsKey(i) ? Participants[i] : null; } }

	/// <summary>
	/// Takes a list of <see cref="DataPoint"/> objects, instantiate a <see cref="Participant"/> for each of the unique 
	/// <see cref="DataPoint.userid"/> values seen, and assigns each datapoint to the corresponding participant.
	/// </summary>
	/// <param name="points">All points read/ converted from the csv input file</param>
	public void AddDataPoints(List<DataPoint> points)
	{
		foreach (DataPoint point in points)
		{
			if (!Participants.ContainsKey(point.userid))
				Participants.Add(point.userid, new Participant(point.userid));

			Participants[point.userid].AddDataPoint(point);
		}
	}

	#region Output

	#region StayPoint output

	/// <summary>
	/// Calculates the participants staypoint groups, and returns them in a list ready to be written to a csv.
	/// </summary>
	/// <returns>The list of staypoint groups</returns>
	public List<StayPointGroupOutput> GetStayPointOutput()
	{
		Console.WriteLine("Calculating StayPoint output...");
		DateTime loadStart = DateTime.Now;

		List<StayPointGroupOutput> output = new List<StayPointGroupOutput>();

		int i = 1;
		int max = Participants.Count;

		foreach (Participant participant in Participants.Values)
		{
			participant.CalculateStayPoints();
			participant.StayPoints.ForEach(sp => { sp.CalculateStrength(); output.Add(sp.TotalOutput); output.AddRange(sp.Groups); });

			WriteProgress(i++, max);
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("\n\tComplete\n\t{0:0.00}s\n", (DateTime.Now - loadStart).TotalSeconds);
		Console.ResetColor();

		return output;
	}
	#endregion

	#region Path output

	/// <summary>
	/// Calculates the participants paths, and returns them in a list ready to be written to a csv.
	/// </summary>
	/// <returns>The list of paths</returns>
	public List<PathPointOutput> GetPathOutput()
	{
		Console.WriteLine("Calculating Path output...");
		DateTime loadStart = DateTime.Now;

		List<PathPointOutput> output = new List<PathPointOutput>();

		int i = 1;
		int max = Participants.Count;

		foreach (Participant participant in Participants.Values)
		{
			participant.CalculatePaths();
			if (participant.Paths.Count >= Affectors.Path_MinPaths)
				participant.Paths.ForEach(p => { output.AddRange(p.GetOutput()); });

			WriteProgress(i++, max);
		}

		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine("\n\tComplete\n\t{0:0.00}s\n", (DateTime.Now - loadStart).TotalSeconds);
		Console.ResetColor();

		List<PathPointOutput> onlyOne = new List<PathPointOutput>();
		MostPaths().Paths.ForEach(p => { onlyOne.AddRange(p.GetOutput()); });

		//return output;
		return onlyOne;
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

	#region Pretty Colors

	public void WriteProgress(int prog, int max)
	{
		Console.CursorLeft = 0;
		Console.Write("(");

		Console.ForegroundColor = prog < max ? ConsoleColor.Yellow : ConsoleColor.Green;
		Console.Write("{0}", prog);
		Console.ResetColor();

		Console.Write("/{0})", max);
	}

	#endregion

}
