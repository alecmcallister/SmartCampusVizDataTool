using System;
using System.Collections.Generic;
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
	public List<DataPoint> Contents { get; set; } = new List<DataPoint>();

	public int UserID { get; set; }
	public int StayPointID { get; set; }
	public Vector2 Location { get; set; }
	public DateTime StartDate => Contents.Min(p => p.datetime_start);
	public DateTime EndDate => Contents.Max(p => p.datetime_end);

	public decimal Radius { get; set; }

	public TimeSpan TimeSpan => EndDate - StartDate;

	// TODO: Change to method (like paths)
	public List<StayPointGroupOutput> Groups { get; set; } = new List<StayPointGroupOutput>();

	public StayPointGroupOutput TotalOutput
	{
		get
		{
			double qScore = Groups.Sum(g => g.QuantityScore);
			double tScore = Groups.Sum(g => g.TemporalScore);
			double aScore = Groups.Average(g => g.AccuracyScore);
			double cScore = Groups.Sum(g => g.CombinedScore);

			return new StayPointGroupOutput()
			{
				UserID = UserID,
				StayPointID = StayPointID,
				StayPointGroupID = -1,
				YYC_X = Location.X,
				YYC_Y = Location.Y,
				StartDate = StartDate,
				EndDate = EndDate,
				StayDurationMinutes = Groups.Sum(g => g.StayDurationMinutes),
				QuantityScore = qScore,
				TemporalScore = tScore,
				AccuracyScore = aScore,
				CombinedScore = cScore
			};
		}
	}

	/// <summary>
	/// Initializes a new staypoint centered on the given datapoint
	/// </summary>
	/// <param name="point">The raw point data</param>
	/// <param name="radius">The radius of this staypoint</param>
	public StayPoint(DataPoint point, int stayPointID, decimal radius = 10m)
	{
		UserID = point.userid;
		StayPointID = stayPointID;
		Location = point.yycStart;
		Radius = radius;
		Contents.Add(point);
	}

	/// <summary>
	/// Does this staypoint overlap the given datapoint?
	/// </summary>
	/// <param name="point">The point in question.</param>
	/// <returns>True if the point is within <see cref="Radius"/> units from <see cref="Location"/>, false otherwise</returns>
	public bool OverlapsPoint(DataPoint point)
	{
		return Location.IsWithinDistance(point.yycStart, Radius);
	}

	/// <summary>
	/// Returns a list of all staypoints that overlap the given datapoint
	/// </summary>
	/// <param name="staypoints">List of staypoints forming the overlap region</param>
	/// <param name="point">The point to check</param>
	/// <returns>A subset of the provided staypoints where each staypoint overlaps the datapoint</returns>
	public static List<StayPoint> ListOverlapsPoint(List<StayPoint> staypoints, DataPoint point)
	{
		return staypoints.Where(p => p.OverlapsPoint(point)).ToList();
	}

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

	#region Strength calculations

	/// <summary>
	/// Gets the strength of this stay point
	/// </summary>
	public void CalculateStrength()
	{
		Groups.Clear();

		// TODO: Add factor3 = accuracy of grouped points

		// Minutes before the next datapoint is considered outside of the current bonus calculation
		double timeDiffCutoff = 80d;

		// Keep track of the current running-total minutes spent within this staypoint

		List<DataPoint> tempGroup = new List<DataPoint>();

		for (int i = 0, j = 1; i < Contents.Count; i++, j++)
		{
			tempGroup.Add(Contents[i]);

			double timeDiff = timeDiffCutoff + 1d;

			if (j != Contents.Count)
			{
				// Only include events that are within the cutoff time of each other (i.e. passing by briefly won't contribute)
				timeDiff = (Contents[j].datetime_start - Contents[i].datetime_end).TotalMinutes;
			}

			if (timeDiff > timeDiffCutoff)
			{
				DateTime startDate = tempGroup.First().datetime_start;
				DateTime endDate = tempGroup.Last().datetime_end;

				double duration = (endDate - startDate).TotalMinutes;

				double qScore = CalculateQuantityScoreOfGroup(tempGroup);
				double tScore = CalculateTemporalScoreOfGroup(tempGroup);
				double aScore = CalculateAccuracyScoreOfGroup(tempGroup);
				double cScore = qScore * Math.Max(1d, tScore) * aScore;

				Groups.Add(new StayPointGroupOutput()
				{
					UserID = UserID,
					StayPointID = StayPointID,
					StayPointGroupID = Groups.Count,
					YYC_X = Location.X,
					YYC_Y = Location.Y,
					StartDate = startDate,
					EndDate = endDate,
					StayDurationMinutes = duration,
					QuantityScore = qScore,
					TemporalScore = tScore,
					AccuracyScore = aScore,
					CombinedScore = cScore
				});

				tempGroup.Clear();
			}
		}
	}

	double quantityWeight = 1d;
	public double CalculateQuantityScoreOfGroup(List<DataPoint> dataPoints)
	{
		return dataPoints.Count * quantityWeight;
	}

	double temporalWeight = 1.5d;
	public double CalculateTemporalScoreOfGroup(List<DataPoint> dataPoints, double timeSpentThreshold = 10d)
	{
		double timeSpent = (dataPoints.Last().datetime_end - dataPoints.First().datetime_start).TotalMinutes;

		return timeSpent > Math.Max(1d, timeSpentThreshold) ? Math.Log(timeSpent * temporalWeight) : 0d;
	}

	public double CalculateAccuracyScoreOfGroup(List<DataPoint> dataPoints)
	{
		if (dataPoints.Count == 0)
			return 0d;

		double accuracyThreshold = 20d;

		return dataPoints.Average(dp => accuracyThreshold / dp.accuracy);
	}

	// Incorporate factor corresponding to staypoints used repeatedly over separate dates
	// Factor to increase score of staypoints that are used commonly year-round (don't relate to a classroom)

	// Do path calculation for each user

	#endregion

	#region Helpers

	/// <summary>
	/// Returns all points within the given interval
	/// </summary>
	/// <param name="start">Start time.</param>
	/// <param name="end">End time.</param>
	/// <returns></returns>
	public List<DataPoint> PointsWithinTimeInterval(DateTime start, DateTime end)
	{
		return Contents.Where(x => x.datetime_start > start && x.datetime_end < end).ToList();
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

public class StayPointGroupOutput
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
}

