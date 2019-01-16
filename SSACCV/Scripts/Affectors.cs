using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class that controls the various parameters affecting the calculation of staypoints, paths, etc.
/// </summary>
public static class Affectors
{
	public delegate double Scale(double val);

	#region Stay Points

	/// <summary>
	/// The radius of each staypoint (meters?). Used when determining if a point resides within
	/// a given staypoint.
	/// Default = 10
	/// </summary>
	public const decimal Stay_Radius = 20m;
	
	/// <summary>
	/// Should a single point be allowed to be in more than one staypoint?
	/// Default = false
	/// </summary>
	public const bool Stay_PointsCanExistInMultipleStayPoints = false;

	/// <summary>
	/// Time (minutes) before the next datapoint is considered outside of the current bonus calculation.
	/// Default = 80
	/// </summary>
	public const double Stay_TimeDiffCutoff = 50d;

	#region Score filtering

	public delegate bool Verify(double val);
	public static Verify aVerify = a => a >= Stay_MinAScore;
	public static Verify durationVerify = d => d >= Stay_MinDuration && d <= Stay_MaxDuration;
	public static Verify countVerify = c => c >= Stay_MinGroupCount;

	public const double Stay_MinAScore = 0.4d;

	public const double Stay_MinDuration = 10d;
	public const double Stay_MaxDuration = 6d * 60d;

	/// <summary>
	/// Minimum number of points (inclusive) in an area to be considered a staypoint group.
	/// Default = 5
	/// </summary>
	public const int Stay_MinGroupCount = 5;

	#endregion

	#region Quantity

	/// <summary>
	/// Relative value of multiple points within the same staypoint.
	/// Default = 1
	/// </summary>
	public const double Stay_QuantityWeight = 1d;

	/// <summary>
	/// Logarithmic scale for time.
	/// </summary>
	public static Scale QuantityScale = Math.Log;

	#endregion

	#region Temporal

	/// <summary>
	/// Relative value of prolonged periods of 'staying' inside a staypoint.
	/// Default = 20
	/// </summary>
	public const double Stay_TemporalWeight = 20d;

	/// <summary>
	/// Logarithmic scale for time.
	/// </summary>
	public static Scale TemporalScale = Math.Log;

	#endregion

	#region Accuracy

	/// <summary>
	/// Datapoints with accuracy scores lower than this number will increase the overall 
	/// combined score, while higher accuracy scores will decrease it (usually the case).
	/// Recommended between 18 - 30.
	/// Default = 20
	/// </summary>
	public const double Stay_AccuracyGoal = 20d;
	public const double Stay_AScoreCeiling = 1.25d;

	#endregion

	#endregion

	#region Paths

	/// <summary>
	/// Minumum number of paths (inclusive) that a user must have to be included in the final output.
	/// Users with less than this value will be excluded.
	/// Default = 0
	/// </summary>
	public const int Path_MinPaths = 0;

	/// <summary>
	/// Minumum number of segments (inclusive) a path must have to be valid.
	/// Default = 2
	/// </summary>
	public const int Path_MinSegments = 4;

	/// <summary>
	/// Maximum amount of time (minutes) allowed when calculating the contiguity of subsequent points.
	/// Points occuring later than this value will be excluded from the current path.
	/// Default = 20
	/// </summary>
	public const double Path_SubsequentPointTimeCutoff = 20d;

	/// <summary>
	/// Minumum distance (meters?) required when calculating the contiguity of subsequent points.
	/// Points located closer than this value will be excluded from the current path.
	/// Default = 2
	/// </summary>
	public const decimal Path_MinSubsequentDistanceThreshold = 15m;
	public const decimal Path_MaxSubsequentDistanceThreshold = 200m;

	#endregion

}
