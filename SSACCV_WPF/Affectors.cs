using System;

/// <summary>
/// Class that controls the various parameters affecting the calculation of restpoints, paths, etc.
/// </summary>
public class Affectors
{
	public static Affectors Instance;

	public delegate double Scale(double val);
	public delegate bool Verify(double val);

	/// <summary>
	/// The string we append to the end of the calculated restpoint csv file.
	/// </summary>
	/// <returns>The (partial) list of affectors that were used when calculating restpoints.</returns>
	public string GetRestpointIdentityString()
	{
		return string.Format("(Radius{0}_Cutoff{1}_MinDuration{2}_MaxDuration{3})",
			(int)Rest_Radius,
			(int)Rest_TimeDiffCutoff,
			(int)Rest_MinDuration,
			(int)Rest_MaxDuration);
	}

	/// <summary>
	/// The string we append to the end of the calculated path csv file.
	/// </summary>
	/// <returns>The (partial) list of affectors that were used when calculating paths.</returns>
	public string GetPathIdentityString()
	{
		return string.Format("(MinSegments{0}_MaxTime{1}_MinDist{2}_MaxDist{3})",
			Path_MinSegments,
			(int)Path_MaxSubsequentTime,
			(int)Path_MinSubsequentDistance,
			(int)Path_MaxSubsequentDistance);
	}

	#region Stay Points

	/// <summary>
	/// The radius of each restpoint (meters). Used when determining if a point resides within
	/// a given restpoint.
	/// Default = 50
	/// </summary>
	public double Rest_Radius { get; set; } = 50d;

	/// <summary>
	/// Time (minutes) before the next datapoint is considered outside of the current bonus calculation.
	/// Default = 50
	/// </summary>
	public double Rest_TimeDiffCutoff { get; set; } = 50d;

	#region Score filtering

	/// <summary>
	/// Used to verify restpoint group accuracy scores.
	/// Returns true if [ score &gt;= <see cref="Rest_MinAScore"/> ]
	/// </summary>
	public Verify aVerify = a => a >= Instance.Rest_MinAScore;

	/// <summary>
	/// Used to verify restpoint group durations.
	/// Returns true if [ <see cref="Rest_MinDuration"/> &lt;= duration &lt;= <see cref="Rest_MaxDuration"/> ]
	/// </summary>
	public Verify durationVerify = d => d >= Instance.Rest_MinDuration && d <= Instance.Rest_MaxDuration;

	/// <summary>
	/// Used to verify restpoint groups have enough points.
	/// Returns true if [ count &gt;= <see cref="Rest_MinGroupCount"/> ]
	/// </summary>
	public Verify countVerify = c => c >= Instance.Rest_MinGroupCount;

	/// <summary>
	/// The minimum allowable accuracy score for each restpoint group.
	/// Groups with scores lower than this number will be excluded from the final output.
	/// Default = 0.4
	/// </summary>
	public double Rest_MinAScore { get; set; } = 0.4d;

	/// <summary>
	/// The minimum duration (minutes from first point to last point) for each restpoint group.
	/// Groups with durations lower than this number will be excluded from the final output.
	/// Default = 10
	/// </summary>
	public double Rest_MinDuration { get; set; } = 10d;

	/// <summary>
	/// The maximum duration (minutes from first point to last point) for each restpoint group.
	/// Groups with durations higher than this number will be excluded from the final output.
	/// Default = 1440 (24hrs)
	/// </summary>
	public double Rest_MaxDuration { get; set; } = 24d * 60d;

	/// <summary>
	/// Minimum number of points (inclusive) in an area to be considered a restpoint group.
	/// Default = 5
	/// </summary>
	public int Rest_MinGroupCount { get; set; } = 5;

	#endregion

	#region Quantity

	/// <summary>
	/// Relative value of multiple points within the same restpoint.
	/// Default = 1
	/// </summary>
	public double Rest_QuantityWeight { get; set; } = 1d;

	/// <summary>
	/// The scale used when calculating quantity scores.
	/// Default: Math.Log
	/// </summary>
	public Scale QuantityScale = Math.Log;

	#endregion

	#region Temporal

	/// <summary>
	/// Relative value of prolonged periods of 'resting' inside a restpoint.
	/// Default = 20
	/// </summary>
	public double Rest_TemporalWeight { get; set; } = 20d;

	/// <summary>
	/// The scale used when calculating temporal scores.
	/// Default: Math.Log
	/// </summary>
	public Scale TemporalScale = Math.Log;

	#endregion

	#region Accuracy

	/// <summary>
	/// Datapoints with accuracy scores lower than this number will increase the overall 
	/// combined score, while higher accuracy scores will decrease it (usually the case).
	/// Recommended between 18 - 30.
	/// Default = 20
	/// </summary>
	public double Rest_AccuracyGoal { get; set; } = 20d;

	/// <summary>
	/// The maximum possible accuracy score.
	/// Default = 1.25
	/// </summary>
	public double Rest_AScoreCeiling { get; set; } = 1.25d;

	#endregion

	#endregion

	#region Paths

	/// <summary>
	/// Minumum number of segments (inclusive) a path must have to be valid.
	/// Default = 5
	/// </summary>
	public int Path_MinSegments { get; set; } = 5;

	/// <summary>
	/// Minimum amount of time (minutes) allowed when calculating the contiguity of subsequent points.
	/// Points occuring before this value will be excluded from the current path.
	/// Default = 0.5 (30 seconds)
	/// </summary>
	public double Path_MinSubsequentTime { get; set; } = 0.5d;

	/// <summary>
	/// Maximum amount of time (minutes) allowed when calculating the contiguity of subsequent points.
	/// Points occuring later than this value will be excluded from the current path.
	/// Default = 20
	/// </summary>
	public double Path_MaxSubsequentTime { get; set; } = 20d;

	/// <summary>
	/// Minumum distance (meters) required when calculating the contiguity of subsequent points.
	/// Points located closer than this value will be excluded from the current path.
	/// Default = 15m
	/// </summary>
	public double Path_MinSubsequentDistance { get; set; } = 15d;

	/// <summary>
	/// Maximum distance (meters) allowed when calculating the contiguity of subsequent points.
	/// Points located farther than this value will be excluded from the current path.
	/// Default = 200m
	/// </summary>
	public double Path_MaxSubsequentDistance { get; set; } = 200d;

	/// <summary>
	/// An extra bit of wiggle-room when calculating the equality of lat/lon positions.
	/// Used for removing erroneous points from a path.
	/// If the path is A -> B -> C, and Abs(A - C) is less than this value (essentially the same point), 
	/// then both B and C are removed from the path.
	/// Default = 0.001
	/// </summary>
	public double Path_EssentiallyEqualsDistance { get; set; } = 0.001d;

	#endregion

}
