using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

/// <summary>
/// Abstract representation of a popular staying location for a user.
/// Ex. classroom, favorite eating spot, etc.
/// </summary>
public class Staypoint
{
	/// <summary>
	/// The datapoints within this staypoint
	/// </summary>
	public ConcurrentBag<DataPoint> Contents { get; set; } = new ConcurrentBag<DataPoint>();

	public int UserID { get; set; }
	public int StayPointID { get; set; }

	public string BuildingID { get; set; }
	public string BuildingName { get; set; }

	public Vector2 Location { get; set; }
	public Vector2 Centroid => Vector2.Centroid(Contents.Select(p => p.location).ToList());

	public DateTime StartDate => Contents.Min(d => d.loct);
	public DateTime EndDate => Contents.Max(d => d.loct);

	/// <summary>
	/// Initializes a new staypoint centered on the given datapoint
	/// </summary>
	/// <param name="point">The raw point data</param>
	/// <param name="radius">The radius of this staypoint</param>
	public Staypoint(DataPoint point, int stayPointID)
	{
		UserID = point.userid;
		StayPointID = stayPointID;
		BuildingID = point.building_id;
		BuildingName = point.building_name;
		Location = point.location;
		Contents.Add(point);
	}

	#region Add point

	/// <summary>
	/// Only adds points that are within the given radius
	/// </summary>
	/// <param name="point">The point we want to potentially add</param>
	/// <returns>True if the point was added, false otherwise</returns>
	public bool ConditionalAddPoint(DataPoint point)
	{
		// This is currently the only metric we use to place points
		if (OverlapsPoint(point))
		{
			Contents.Add(point);
			return true;
		}

		return false;
	}

	#endregion

	#region Strength calculations

	/// <summary>
	/// Calculated the strength of the staypoint groups, and returns them in a list.
	/// </summary>
	/// <returns>Output list</returns>
	public List<StaypointOutput> GetOutput()
	{
		List<StaypointOutput> output = new List<StaypointOutput>();

		List<DataPoint> points = Contents.ToList();
		points.Sort();

		// Only do the calculation once
		Vector2 centroid = Centroid;

		List<DataPoint> tempGroup = new List<DataPoint>();

		for (int i = 0, j = 1; i < points.Count; i++, j++)
		{
			tempGroup.Add(points[i]);

			double timeDiff = Affectors.Instance.Stay_TimeDiffCutoff + 1;

			// Only include events that are within the cutoff time of each other (i.e. passing by briefly won't contribute)
			if (j != Contents.Count)
				timeDiff = (points[j].loct - points[i].loct).TotalMinutes;

			if (timeDiff > Affectors.Instance.Stay_TimeDiffCutoff)
			{
				DataPoint first = tempGroup.First();
				DataPoint last = tempGroup.Last();

				double aScore = CalculateAccuracyScoreOfGroup(tempGroup);
				double duration = (last.loct - first.loct).TotalMinutes;

				// Filter based on score
				bool addToOutput = FilterScore(aScore, duration, tempGroup.Count);

				if (addToOutput)
				{
					double qScore = CalculateQuantityScoreOfGroup(tempGroup);
					double tScore = CalculateTemporalScoreOfGroup(tempGroup);
					double cScore = qScore * tScore * aScore;

					Vector2 groupCentroid = Vector2.Centroid(tempGroup.Select(p => p.location).ToList());

					StaypointOutput spaghetti = new StaypointOutput()
					{
						UserID = UserID,
						StaypointID = StayPointID,
						StaypointGroupID = output.Count,
						StartDate = first.loct,
						EndDate = last.loct,
						StayDurationMinutes = (float)duration,
						AcademicDayStart = first.academic_day,
						AcademicDayEnd = last.academic_day,
						BuildingID = BuildingID,
						BuildingName = BuildingName,
						Lat = Location.X,
						Lon = Location.Y,
						MaxTemp = (float)tempGroup.Average(p => p.max_temp),
						MeanTemp = (float)tempGroup.Average(p => p.mean_temp),
						TotalPrecip = (float)tempGroup.Average(p => p.total_precip),
						Snow = (int)tempGroup.Average(p => p.snow),
						QuantityScore = (float)qScore,
						TemporalScore = (float)tScore,
						AccuracyScore = (float)aScore,
						CombinedScore = (float)cScore,
						CentroidLat = centroid.X,
						CentroidLon = centroid.Y,
						GroupCentroidLat = groupCentroid.X,
						GroupCentroidLon = groupCentroid.Y
					};

					output.Add(spaghetti);
				}

				tempGroup.Clear();
			}
		}

		return output;
	}

	/// <summary>
	/// Filters out staypoint groups under the accuracy score threshold, under the duration threshold, or below the count threshold.
	/// </summary>
	/// <param name="aScore">The group's accuracy score.</param>
	/// <param name="duration">How many consecutive minutes were spent in this group.</param>
	/// <param name="count">How many points were collected for this group</param>
	/// <returns>True if all requirements were met, false otherwise.</returns>
	public bool FilterScore(double aScore, double duration, int count)
	{
		return
			Affectors.Instance.aVerify(aScore) &&
			Affectors.Instance.durationVerify(duration) &&
			Affectors.Instance.countVerify(count);
	}

	#region Score calculation

	public double CalculateQuantityScoreOfGroup(List<DataPoint> dataPoints)
	{
		return Affectors.Instance.QuantityScale(Math.Max(1d, dataPoints.Count * Affectors.Instance.Stay_QuantityWeight));
	}

	public double CalculateTemporalScoreOfGroup(List<DataPoint> dataPoints)
	{
		double timeSpent = (dataPoints.Last().loct - dataPoints.First().loct).TotalMinutes;

		return Affectors.Instance.TemporalScale(Math.Max(1d, timeSpent * Affectors.Instance.Stay_TemporalWeight));
	}

	public double CalculateAccuracyScoreOfGroup(List<DataPoint> dataPoints)
	{
		if (dataPoints.Count == 0)
			return 0d;

		return Math.Min(dataPoints.Average(dp => Affectors.Instance.Stay_AccuracyGoal / dp.accuracy), Affectors.Instance.Stay_AScoreCeiling);
	}

	#endregion

	#endregion

	#region Helpers

	/// <summary>
	/// Does this staypoint overlap the given datapoint?
	/// </summary>
	/// <param name="point">The point in question.</param>
	/// <returns>True if the point is within <see cref="Radius"/> units, false otherwise</returns>
	public bool OverlapsPoint(DataPoint point)
	{
		return Location.IsWithinDistance(point.location, Affectors.Instance.Stay_Radius);
	}

	/// <summary>
	/// Returns all points within the given interval
	/// </summary>
	/// <param name="start">Start time.</param>
	/// <param name="end">End time.</param>
	/// <returns></returns>
	public List<DataPoint> PointsWithinTimeInterval(DateTime start, DateTime end)
	{
		return Contents.Where(x => x.loct > start && x.loct < end).ToList();
	}

	/// <summary>
	/// Perform an intersection of points in a and b
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns>A list of <see cref="DataPoint"/> which reside in both staypoints.</returns>
	public static List<DataPoint> operator &(Staypoint a, Staypoint b)
	{
		return a.Contents.Where(p => a.OverlapsPoint(p) && b.OverlapsPoint(p)).ToList();
	}

	/// <summary>
	/// Perform a union of points in a and b
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns>A list of <see cref="DataPoint"/> which reside in either staypoint.</returns>
	public static List<DataPoint> operator |(Staypoint a, Staypoint b)
	{
		return a.Contents.Union(b.Contents).ToList();
	}

	public static List<List<Staypoint>> GroupByDate(List<Staypoint> input)
	{
		input.Sort(Comparer<Staypoint>.Create((sp1, sp2) => { return sp1.StartDate.CompareTo(sp2.StartDate); }));

		// https://stackoverflow.com/questions/2697253/using-linq-to-group-a-list-of-objects-into-a-new-grouped-list-of-list-of-objects
		return input.GroupBy(sp => sp.StartDate.Date).Select(g => g.ToList()).ToList();
	}

	#endregion
}
