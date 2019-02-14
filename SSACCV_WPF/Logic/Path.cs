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
		if (DataPoint.TimeDifference(Contents.Last(), point) < Affectors.Instance.Path_SubsequentPointTimeCutoff &&
			Contents.Last().DistanceTo(point) > Affectors.Instance.Path_MinSubsequentDistanceThreshold &&
			Contents.Last().DistanceTo(point) < Affectors.Instance.Path_MaxSubsequentDistanceThreshold)
		{
			Contents.Add(point);
			return true;
		}

		return false;
	}

	public List<PathOutput> GetOutput()
	{
		List<PathOutput> output = new List<PathOutput>();

		for (int i = 0, j = 1; i < Contents.Count; i++, j++)
		{
			DataPoint point = Contents[i];
			DataPoint next = j < Contents.Count ? Contents[j] : null;
			double distanceToNext = next != null ? Vector2.AzimuthDistance(point.location, next.location) : 0;
			double minutesToNext = next != null ? (next.loct - point.loct).TotalMinutes : 0;

			output.Add(new PathOutput()
			{
				UserID = UserID,
				PathID = PathID,
				PathPointID = i,
				Date = point.loct,
				AcademicDay = point.academic_day,
				BuildingID = point.building_id,
				BuildingName = point.building_name,
				Lat = point.lat,
				Lon = point.lon,
				DistanceToNextPoint = distanceToNext,
				MinutesToNextPoint = minutesToNext,
				MaxTemp = point.max_temp,
				MeanTemp = point.mean_temp,
				TotalPrecip = point.total_precip,
				Snow = point.snow,
				AzimuthPath = Vector2.Azimuth(StartPoint.location, EndPoint.location),
				AzimuthSegment = next != null ? Vector2.Azimuth(point.location, next.location) : 0d,
				Speed = minutesToNext > 0 ? distanceToNext / minutesToNext : 0d
			});
		}

		return output;
	}
}
