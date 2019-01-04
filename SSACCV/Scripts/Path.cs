using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class that represents a single spatio-temporal bout/walk/journey for a single user.
/// Consists of several <see cref="DataPoint"/> objects, which form a contiguous line from start to end.
/// The contiguity of points is constrained by the distance and time between adjacent points.
/// </summary>
public class Path
{
	public int UserID { get; set; }
	public int PathID { get; set; }
	public List<DataPoint> Contents { get; set; } = new List<DataPoint>();
	public DataPoint StartPoint => Contents.First();
	public DataPoint EndPoint => Contents.Last();
	public DateTime StartDate => StartPoint.loct;
	public DateTime EndDate => EndPoint.loct;
	public TimeSpan TimeSpan => EndDate - StartDate;

	public DataPoint StartLocation => Contents.First();
	public DataPoint EndLocation => Contents.Last();

	public int Segments => Contents.Count - 1;

	/// <summary>
	/// Instantiates a path object belonging to <paramref name="start"/>.userid, with ID <paramref name="pathID"/>, which starts at <paramref name="start"/>.
	/// </summary>
	/// <param name="start">The start of the path</param>
	/// <param name="pathID">The user</param>
	public Path(DataPoint start, int pathID)
	{
		UserID = start.userid;
		PathID = pathID;
		Contents.Add(start);
	}

	public bool ConditionalAddPoint(DataPoint point)
	{
		if (DataPoint.TimeDifference(Contents.Last(), point) < Affectors.Path_SubsequentPointTimeCutoff && 
			Contents.Last().DistanceTo(point) > Affectors.Path_SubsequentDistanceThreshold)
		{
			Contents.Add(point);
			return true;
		}

		return false;
	}

	public List<PathPointOutput> GetOutput()
	{
		List<PathPointOutput> output = new List<PathPointOutput>();

		for (int i = 0, j = 1; i < Contents.Count; i++, j++)
		{
			DataPoint point = Contents[i];
			DataPoint next = j < Contents.Count ? Contents[j] : null;

			output.Add(new PathPointOutput()
			{
				UserID = UserID,
				PathID = PathID,
				PathPointID = i,
				StayPointID = point.staypointID,
				YYC_X = point.location.X,
				YYC_Y = point.location.Y,
				Date = point.loct,
				DistanceToNextPoint = next != null ? (next.location - point.location).magnitude : 0,
				MinutesToNextPoint = next != null ? (next.loct - point.loct).TotalMinutes : 0
			});
		}

		return output;
	}

}

public class PathPointOutput : IComparable<PathPointOutput>
{
	public int UserID { get; set; }
	public int PathID { get; set; }
	public int PathPointID { get; set; }
	public int StayPointID { get; set; }
	public decimal YYC_X { get; set; }
	public decimal YYC_Y { get; set; }
	public DateTime Date { get; set; }

	// 0 When last point in path
	public decimal DistanceToNextPoint { get; set; }

	// Minutes is measured from start to start, not end to start
	// 0 When last point in path
	public double MinutesToNextPoint { get; set; }

	public int CompareTo(PathPointOutput other)
	{
		int val = UserID.CompareTo(other.UserID);

		if (val == 0)
			val = PathID.CompareTo(other.PathID);

		if (val == 0)
			val = PathPointID.CompareTo(other.PathPointID);

		return val;
	}

	//// TODO: Change to individual path points instead of the overall path
	//public int StartStayPoint { get; set; }
	//public int EndStayPoint { get; set; }
}


/// <summary>
/// UNTIL I ACTUALLY CODE THE PROPER CLASS FOR STAYPOINTGROUP OBJECTS
/// </summary>
public static class SUPERSPHAGETTI
{
	/// <summary>
	/// DONT YOU FUCKING DARE LEAVE THIS IN THE PROJECT
	/// </summary>
	public static ConcurrentDictionary<StayPointGroupOutput, List<DataPoint>> SPHAGETTI = new ConcurrentDictionary<StayPointGroupOutput, List<DataPoint>>();
}