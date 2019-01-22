using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

/// <summary>
/// Abstract representation of a popular staying location for a user.
/// Ex. classroom, favorite eating spot, etc.
/// </summary>
public class StayPoint
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
	public Vector2 YYC_Location { get; set; }

	/// <summary>
	/// Initializes a new staypoint centered on the given datapoint
	/// </summary>
	/// <param name="point">The raw point data</param>
	/// <param name="radius">The radius of this staypoint</param>
	public StayPoint(DataPoint point, int stayPointID)
	{
		UserID = point.userid;
		StayPointID = stayPointID;
		BuildingID = point.building_id;
		BuildingName = point.building_name;
		Location = point.location;
		YYC_Location = point.yyc_location;
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
				DateTime startDate = first.loct;
				DateTime endDate = last.loct;

				double duration = (endDate - startDate).TotalMinutes;

				double qScore = CalculateQuantityScoreOfGroup(tempGroup);
				double tScore = CalculateTemporalScoreOfGroup(tempGroup);
				double aScore = CalculateAccuracyScoreOfGroup(tempGroup);
				double cScore = qScore * tScore * aScore;

				// Filter based on score
				bool addToOutput = FilterScore(aScore, duration, tempGroup.Count);

				if (addToOutput)
				{
					StaypointOutput spaghetti = new StaypointOutput()
					{
						UserID = UserID,
						StaypointID = StayPointID,
						StaypointGroupID = output.Count,
						StartDate = startDate,
						EndDate = endDate,
						StayDurationMinutes = duration,
						AcademicDayStart = first.academic_day,
						AcademicDayEnd = last.academic_day,
						BuildingID = BuildingID,
						BuildingName = BuildingName,
						Lat = Location.X,
						Lon = Location.Y,
						YYC_X = YYC_Location.X,
						YYC_Y = YYC_Location.Y,
						MaxTemp = tempGroup.Average(p => p.max_temp),
						MeanTemp = tempGroup.Average(p => p.mean_temp),
						TotalPrecip = tempGroup.Average(p => p.total_precip),
						Snow = (int)tempGroup.Average(p => p.snow),
						QuantityScore = qScore,
						TemporalScore = tScore,
						AccuracyScore = aScore,
						CombinedScore = cScore
					};

					output.Add(spaghetti);
				}
				tempGroup.Clear();
			}
		}

		return output;
	}

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
	/// <returns>True if the point is within <see cref="Radius"/> units from <see cref="YYC_Location"/>, false otherwise</returns>
	public bool OverlapsPoint(DataPoint point)
	{
		return YYC_Location.IsWithinDistance(point.yyc_location, Affectors.Instance.Stay_Radius);
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
	public static List<DataPoint> operator &(StayPoint a, StayPoint b)
	{
		return a.Contents.Where(p => a.OverlapsPoint(p) && b.OverlapsPoint(p)).ToList();
	}

	/// <summary>
	/// Perform a union of points in a and b
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns>A list of <see cref="DataPoint"/> which reside in either staypoint.</returns>
	public static List<DataPoint> operator |(StayPoint a, StayPoint b)
	{
		return a.Contents.Union(b.Contents).ToList();
	}

	#endregion
}
