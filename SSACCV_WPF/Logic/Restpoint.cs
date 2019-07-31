using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

/// <summary>
/// Abstract representation of a popular resting location for a user.
/// Ex. classroom, favorite eating spot, etc.
/// </summary>
public class Restpoint
{
	/// <summary>
	/// The datapoints within this restpoint
	/// </summary>
	public ConcurrentBag<DataPoint> Contents { get; set; } = new ConcurrentBag<DataPoint>();

	public int UserID { get; set; }
	public int RestPointID { get; set; }

	public string BuildingID { get; set; }
	public string BuildingName { get; set; }

	public Vector2 Location { get; set; }
	public Vector2 Centroid => Vector2.Centroid(Contents.Select(p => p.location).ToList());

	public DateTime StartDate => Contents.Min(d => d.loct);
	public DateTime EndDate => Contents.Max(d => d.loct);

	/// <summary>
	/// Initializes a new restpoint centered on the given datapoint
	/// </summary>
	/// <param name="point">The raw point data</param>
	/// <param name="radius">The radius of this restpoint</param>
	public Restpoint(DataPoint point, int restPointID)
	{
		UserID = point.userid;
		RestPointID = restPointID;
		BuildingID = point.building_id;
		BuildingName = point.building_name;
		Location = point.location;
		Contents.Add(point);
	}

	#region Add point

	/// <summary>
	/// Only adds points that are within <see cref="Affectors.Rest_Radius"/> meters of the center of this restpoint
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

	#region Calculations

	/// <summary>
	/// Calculates the strength of the restpoint groups, and returns them in a list.
	/// </summary>
	/// <returns>Output list</returns>
	public List<RestpointOutput> GetOutput()
	{
		List<RestpointOutput> output = new List<RestpointOutput>();

		// Get the points into a normal (non-concurrent) list, and sort them based on time
		List<DataPoint> points = Contents.ToList();
		points.Sort();

		// Save the results of the centroid calculation
		Vector2 centroid = Centroid;

		List<DataPoint> tempGroup = new List<DataPoint>();

		// Iterate over all datapoints gathered
		// Groups datapoints together to form "restpoint groups" based on several factors
		// (primarily the amount of time spent within the respoint without leaving)
		for (int i = 0, j = 1; i < points.Count; i++, j++)
		{
			tempGroup.Add(points[i]);

			double timeDiff = Affectors.Instance.Rest_TimeDiffCutoff + 1;

			// Only include datapoints that are within the cutoff time of each other (i.e. passing by briefly won't contribute)
			if (j != Contents.Count)
				timeDiff = (points[j].loct - points[i].loct).TotalMinutes;

			if (timeDiff > Affectors.Instance.Rest_TimeDiffCutoff)
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

					RestpointOutput spaghetti = new RestpointOutput()
					{
						UserID = UserID,
						RestpointID = RestPointID,
						RestpointGroupID = output.Count,
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
	/// Filters out restpoint groups under the accuracy score threshold, under the duration threshold, or below the count threshold.
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
		return Affectors.Instance.QuantityScale(Math.Max(1d, dataPoints.Count * Affectors.Instance.Rest_QuantityWeight));
	}

	public double CalculateTemporalScoreOfGroup(List<DataPoint> dataPoints)
	{
		double timeSpent = (dataPoints.Last().loct - dataPoints.First().loct).TotalMinutes;

		return Affectors.Instance.TemporalScale(Math.Max(1d, timeSpent * Affectors.Instance.Rest_TemporalWeight));
	}

	public double CalculateAccuracyScoreOfGroup(List<DataPoint> dataPoints)
	{
		if (dataPoints.Count == 0)
			return 0d;

		return Math.Min(dataPoints.Average(dp => Affectors.Instance.Rest_AccuracyGoal / dp.accuracy), Affectors.Instance.Rest_AScoreCeiling);
	}

	#endregion

	#endregion

	#region Logic

	/// <summary>
	/// Does this restpoint overlap the given datapoint?
	/// </summary>
	/// <param name="point">The point in question.</param>
	/// <returns>True if the point is within <see cref="Radius"/> units, false otherwise</returns>
	public bool OverlapsPoint(DataPoint point)
	{
		return Location.IsWithinDistance(point.location, Affectors.Instance.Rest_Radius);
	}

	#endregion
}
