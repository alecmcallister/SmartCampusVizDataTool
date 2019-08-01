using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class that represents a single spatio-temporal bout/walk/journey for a single user.
/// Consists of several <see cref="DataPoint"/> objects, which form a contiguous line from start to end.
/// The contiguity of points is constrained by the distance and time between adjacent points.
/// </summary>
public class Path
{
	/// <summary>
	/// The datapoints within this path
	/// </summary>
	public List<DataPoint> Contents { get; set; } = new List<DataPoint>();

	/// <summary>
	/// The user that this path belongs to
	/// </summary>
	public int UserID { get; set; }

	/// <summary>
	/// The path's ID (unique to each user)
	/// </summary>
	public int PathID { get; set; }

	/// <summary>
	/// The first datapoint in this path
	/// </summary>
	public DataPoint StartPoint => Contents.First();

	/// <summary>
	/// The last datapoint in this path
	/// </summary>
	public DataPoint EndPoint => Contents.Last();

	/// <summary>
	/// The date of the first datapoint that was added
	/// </summary>
	public DateTime StartDate => StartPoint.loct;

	/// <summary>
	/// The date of the last datapoint that was added
	/// </summary>
	public DateTime EndDate => EndPoint.loct;

	/// <summary>
	/// The span of time between the first and last datapoints
	/// </summary>
	public TimeSpan TimeSpan => EndDate - StartDate;

	/// <summary>
	/// The location (latitude, longitude) of the first datapoint
	/// </summary>
	public DataPoint StartLocation => Contents.First();

	/// <summary>
	/// The location (latitude, longitude) of the last datapoint
	/// </summary>
	public DataPoint EndLocation => Contents.Last();

	/// <summary>
	/// Number of segments this path has. Equal to the number of points - 1
	/// </summary>
	public int Segments => Contents.Count - 1;

	/// <summary>
	/// Instantiates a path object belonging to <paramref name="start"/>.userid, with ID <paramref name="pathID"/>, which starts at <paramref name="start"/>.
	/// </summary>
	/// <param name="start">The start of the path</param>
	/// <param name="pathID">The ID of this path</param>
	public Path(DataPoint start, int pathID)
	{
		UserID = start.userid;
		PathID = pathID;
		Contents.Add(start);
	}

	#region Add point

	/// <summary>
	/// Only adds points that are within a certain distance/ time of the previously added point
	/// </summary>
	/// <param name="point">The point we want to add</param>
	/// <returns>True if the point was added, false otherwise</returns>
	public bool ConditionalAddPoint(DataPoint point)
	{
		// Time (minutes) since the most recently added point
		double timeDiff = Contents.Last().TimeDifference(point);

		// Distance from the most recently added point
		double distanceDiff = Contents.Last().DistanceTo(point);

		// Check if the point meets the requirements to be added to this path
		if (timeDiff > Affectors.Instance.Path_MinSubsequentTime &&
			timeDiff < Affectors.Instance.Path_MaxSubsequentTime &&
			distanceDiff > Affectors.Instance.Path_MinSubsequentDistance &&
			distanceDiff < Affectors.Instance.Path_MaxSubsequentDistance)
		{
			// Remove points that "bounce" between two locations (possibly to a router).
			if (Contents.Count > 2)
			{
				if (Contents[Contents.Count - 2].location.EssentiallyEquals(point.location))
				{
					Contents.RemoveAt(Contents.Count - 1);
					return true;
				}
			}

			// Add the point to the path
			Contents.Add(point);
			return true;
		}

		// The point didn't qualify as part of this path
		return false;
	}

	#endregion

	#region Calculation

	/// <summary>
	/// Creates a list of path points from the list of datapoints (ready to be written to csv).
	/// Not much calculation happening here; Mostly just reformatting the data.
	/// </summary>
	/// <returns>List of point within this path</returns>
	public List<PathOutput> GetOutput()
	{
		List<PathOutput> output = new List<PathOutput>();

		// Iterate over all datapoints gathered
		for (int i = 0, j = 1; i < Contents.Count; i++, j++)
		{
			// The current point
			DataPoint point = Contents[i];

			// The next point
			DataPoint next = j < Contents.Count ? Contents[j] : null;

			// The distance (meters) to the next point
			double distanceToNext = next != null ? Vector2.AzimuthDistance(point.location, next.location) : 0;

			// The time (minutes) to the next point
			double minutesToNext = next != null ? (next.loct - point.loct).TotalMinutes : 0;

			// The time (minutes) from the last point
			double minutesFromLast = (i > 0) ? output.Last().MinutesToNextPoint : 0;

			// Instantiate a new path point, and add it to the output list
			output.Add(new PathOutput()
			{
				UserID = UserID,
				PathID = PathID,
				PathPointID = i,
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

		// Return the list of all path points
		return output;
	}

	#endregion

}
