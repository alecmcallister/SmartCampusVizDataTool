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
		double timeDiff = Contents.Last().TimeDifference(point);
		double distanceDiff = Contents.Last().DistanceTo(point);

		if (timeDiff > Affectors.Instance.Path_MinSubsequentTime &&
			timeDiff < Affectors.Instance.Path_MaxSubsequentTime &&
			distanceDiff > Affectors.Instance.Path_MinSubsequentDistance &&
			distanceDiff < Affectors.Instance.Path_MaxSubsequentDistance)
		{
			if (Contents.Count > 2)
			{
				bool eq2 = Contents[Contents.Count - 2].location.EssentiallyEquals(point.location);
				if (eq2)
				{
					Contents.RemoveAt(Contents.Count - 1);
					return true;
				}
			}

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

			double minutesFromLast = (i > 0) ? output.Last().MinutesToNextPoint : 0;

			output.Add(new PathOutput()
			{
				UserID = UserID,
				PathID = PathID,
				PathPointID = i,
				//Date = point.loct,
				VerboseDate = point.loct,
				AcademicDay = point.academic_day,
				BuildingID = point.building_id,
				BuildingName = point.building_name,
				Lat = point.lat,
				Lon = point.lon,
				DistanceToNextPoint = (float)distanceToNext,
				MinutesToNextPoint = (float)minutesToNext,
				MinutesFromLast = (float)minutesFromLast,
				MaxTemp = (float)point.max_temp,
				MeanTemp = (float)point.mean_temp,
				TotalPrecip = (float)point.total_precip,
				Snow = point.snow,
				AzimuthPath = (float)Vector2.Azimuth(StartPoint.location, EndPoint.location),
				AzimuthSegment = next != null ? (float)Vector2.Azimuth(point.location, next.location) : 0f,
				Speed = minutesToNext > 0 ? (float)(distanceToNext / minutesToNext) : 0f
			});
		}

		return output;
	}
}
