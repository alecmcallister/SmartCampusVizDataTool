using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	public Vector2 Location { get; set; }

	/// <summary>
	/// Initializes a new staypoint centered on the given datapoint
	/// </summary>
	/// <param name="point">The raw point data</param>
	/// <param name="radius">The radius of this staypoint</param>
	public StayPoint(DataPoint point, int stayPointID)
	{
		UserID = point.userid;
		StayPointID = stayPointID;
		Location = point.location;
		Contents.Add(point);
		//point.staypointID = stayPointID;
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
			//point.staypointID = StayPointID;
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
	public List<StayPointGroupOutput> GetOutput()
	{
		List<StayPointGroupOutput> output = new List<StayPointGroupOutput>();

		List<DataPoint> points = Contents.ToList();
		points.Sort();

		List<DataPoint> tempGroup = new List<DataPoint>();

		for (int i = 0, j = 1; i < points.Count; i++, j++)
		{
			tempGroup.Add(points[i]);

			double timeDiff = Affectors.Stay_TimeDiffCutoff + 1;

			// Only include events that are within the cutoff time of each other (i.e. passing by briefly won't contribute)
			if (j != Contents.Count)
				timeDiff = (points[j].loct - points[i].loct).TotalMinutes;

			if (timeDiff > Affectors.Stay_TimeDiffCutoff)
			{
				DateTime startDate = tempGroup.First().loct;
				DateTime endDate = tempGroup.Last().loct;

				double duration = (endDate - startDate).TotalMinutes;

				double qScore = CalculateQuantityScoreOfGroup(tempGroup);
				double tScore = CalculateTemporalScoreOfGroup(tempGroup);
				double aScore = CalculateAccuracyScoreOfGroup(tempGroup);
				double cScore = qScore * tScore * aScore;

				// Filter based on score
				bool addToOutput = FilterScore(aScore, duration, tempGroup.Count);

				if (addToOutput)
				{
					StayPointGroupOutput sphagetti = new StayPointGroupOutput()
					{
						UserID = UserID,
						StayPointID = StayPointID,
						StayPointGroupID = output.Count,
						YYC_X = Location.X,
						YYC_Y = Location.Y,
						StartDate = startDate,
						EndDate = endDate,
						StayDurationMinutes = duration,
						QuantityScore = qScore,
						TemporalScore = tScore,
						AccuracyScore = aScore,
						CombinedScore = cScore
					};

					output.Add(sphagetti);
					SUPERSPHAGETTI.SPHAGETTI.TryAdd(sphagetti, tempGroup.ToList());
				}
				tempGroup.Clear();
			}
		}

		return output;
	}

	public bool FilterScore(double aScore, double duration, int count)
	{
		return
			Affectors.aVerify(aScore) &&
			Affectors.durationVerify(duration) &&
			Affectors.countVerify(count);
	}

	#region Score calculation

	public double CalculateQuantityScoreOfGroup(List<DataPoint> dataPoints)
	{
		return Affectors.QuantityScale(Math.Max(1d, dataPoints.Count * Affectors.Stay_QuantityWeight));
	}

	public double CalculateTemporalScoreOfGroup(List<DataPoint> dataPoints)
	{
		double timeSpent = (dataPoints.Last().loct - dataPoints.First().loct).TotalMinutes;

		return Affectors.TemporalScale(Math.Max(1d, timeSpent * Affectors.Stay_TemporalWeight));
	}

	public double CalculateAccuracyScoreOfGroup(List<DataPoint> dataPoints)
	{
		if (dataPoints.Count == 0)
			return 0d;

		return Math.Min(dataPoints.Average(dp => Affectors.Stay_AccuracyGoal / dp.accuracy), Affectors.Stay_AScoreCeiling);
	}

	#endregion

	#endregion

	#region Helpers

	/// <summary>
	/// Does this staypoint overlap the given datapoint?
	/// </summary>
	/// <param name="point">The point in question.</param>
	/// <returns>True if the point is within <see cref="Radius"/> units from <see cref="Location"/>, false otherwise</returns>
	public bool OverlapsPoint(DataPoint point)
	{
		return Location.IsWithinDistance(point.location, Affectors.Stay_Radius);
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

public class StayPointGroupOutput : IComparable<StayPointGroupOutput>
{
	public int UserID { get; set; }
	public int StayPointID { get; set; }
	public int StayPointGroupID { get; set; }
	public decimal YYC_X { get; set; }
	public decimal YYC_Y { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public double StayDurationMinutes { get; set; }
	public double QuantityScore { get; set; }
	public double TemporalScore { get; set; }
	public double AccuracyScore { get; set; }
	public double CombinedScore { get; set; }

	public int CompareTo(StayPointGroupOutput other)
	{
		int val = UserID.CompareTo(other.UserID);

		if (val == 0)
			val = StayPointID.CompareTo(other.StayPointID);

		if (val == 0)
			val = StayPointGroupID.CompareTo(other.StayPointGroupID);

		if (val == 0)
			val = StartDate.CompareTo(other.StartDate);

		return val;
	}
}

