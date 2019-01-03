using System;
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
	public DateTime StartDate => StartPoint.datetime_start;
	public DateTime EndDate => EndPoint.datetime_start;
	public TimeSpan TimeSpan => EndDate - StartDate;

	public DataPoint StartLocation => Contents.First();
	public DataPoint EndLocation => Contents.Last();

	public int Segments => Contents.Count - 1;

	public int StartStayPoint { get; set; }
	public int EndStayPoint { get; set; }

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
				YYC_X = point.yycStart.X,
				YYC_Y = point.yycStart.Y,
				//Date = point.datetime_start,
				DistanceToNextPoint = next != null ? (next.yycStart - point.yycStart).magnitude : 0,
				MinutesToNextPoint = next != null ? (next.datetime_start - point.datetime_start).TotalMinutes : 0
			});
		}

		return output;
	}

}

public class PathPointOutput
{
	public int UserID { get; set; }
	public int PathID { get; set; }
	public int PathPointID { get; set; }
	public decimal YYC_X { get; set; }
	public decimal YYC_Y { get; set; }

	// Dropped end date for simplicity
	//public DateTime Date { get; set; }

	// 0 When last point in path
	public decimal DistanceToNextPoint { get; set; }

	// Minutes is measured from start to start, not end to start
	// 0 When last point in path
	public double MinutesToNextPoint { get; set; }

	//// TODO: Change to individual path points instead of the overall path
	//public int StartStayPoint { get; set; }
	//public int EndStayPoint { get; set; }
}