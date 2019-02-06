﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Class that controls the various parameters affecting the calculation of staypoints, paths, etc.
/// </summary>
public class Affectors
{
	public static Affectors Instance;

	public delegate double Scale(double val);

	public string GetStaypointIdentityString()
	{
		return string.Format("({0}, {1}, {2}, {3})", 
			(int)Stay_Radius, 
			(int)Stay_TimeDiffCutoff, 
			(int)Stay_MinDuration, 
			Stay_MinGroupCount);
	}
	
	public string GetPathIdentityString()
	{
		return string.Format("({0}, {1}, {2}, {3}, {4})", 
			Path_MinPaths, 
			Path_MinSegments, 
			(int)Path_SubsequentPointTimeCutoff,
			(int)Path_MinSubsequentDistanceThreshold, 
			(int)Path_MaxSubsequentDistanceThreshold);
	}

	#region Stay Points

	/// <summary>
	/// The radius of each staypoint (meters?). Used when determining if a point resides within
	/// a given staypoint.
	/// Default = 50
	/// </summary>
	public double Stay_Radius { get; set; } = 50d;
	
	/// <summary>
	/// Should a single point be allowed to be in more than one staypoint?
	/// Default = false
	/// </summary>
	public bool Stay_PointsCanExistInMultipleStayPoints = false;

	/// <summary>
	/// Time (minutes) before the next datapoint is considered outside of the current bonus calculation.
	/// Default = 50
	/// </summary>
	public double Stay_TimeDiffCutoff { get; set; } = 50d;

	#region Score filtering

	public delegate bool Verify(double val);
	public Verify aVerify = a => a >= Instance.Stay_MinAScore;
	public Verify durationVerify = d => d >= Instance.Stay_MinDuration && d <= Instance.Stay_MaxDuration;
	public Verify countVerify = c => c >= Instance.Stay_MinGroupCount;

	public double Stay_MinAScore { get; set; } = 0.4d;

	public double Stay_MinDuration { get; set; } = 10d;
	public double Stay_MaxDuration { get; set; } = 24d * 60d;

	/// <summary>
	/// Minimum number of points (inclusive) in an area to be considered a staypoint group.
	/// Default = 5
	/// </summary>
	public int Stay_MinGroupCount { get; set; } = 5;

	#endregion

	#region Quantity

	/// <summary>
	/// Relative value of multiple points within the same staypoint.
	/// Default = 1
	/// </summary>
	public double Stay_QuantityWeight { get; set; } = 1d;

	/// <summary>
	/// Logarithmic scale for time.
	/// </summary>
	public Scale QuantityScale = Math.Log;

	#endregion

	#region Temporal

	/// <summary>
	/// Relative value of prolonged periods of 'staying' inside a staypoint.
	/// Default = 20
	/// </summary>
	public double Stay_TemporalWeight { get; set; } = 20d;

	/// <summary>
	/// Logarithmic scale for time.
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
	public double Stay_AccuracyGoal { get; set; } = 20d;
	public double Stay_AScoreCeiling { get; set; } = 1.25d;

	#endregion

	#endregion

	#region Paths

	/// <summary>
	/// Minumum number of paths (inclusive) that a user must have to be included in the final output.
	/// Users with less than this value will be excluded.
	/// Default = 0
	/// </summary>
	public int Path_MinPaths { get; set; } = 0;

	/// <summary>
	/// Minumum number of segments (inclusive) a path must have to be valid.
	/// Default = 2
	/// </summary>
	public int Path_MinSegments { get; set; } = 4;

	/// <summary>
	/// Maximum amount of time (minutes) allowed when calculating the contiguity of subsequent points.
	/// Points occuring later than this value will be excluded from the current path.
	/// Default = 20
	/// </summary>
	public double Path_SubsequentPointTimeCutoff { get; set; } = 20d;

	/// <summary>
	/// Minumum distance (meters?) required when calculating the contiguity of subsequent points.
	/// Points located closer than this value will be excluded from the current path.
	/// Default = 2
	/// </summary>
	public double Path_MinSubsequentDistanceThreshold { get; set; } = 15d;
	public double Path_MaxSubsequentDistanceThreshold { get; set; } = 200d;

	#endregion

	#region Community Staypoints

	/// <summary>
	/// Distance that another staypoint must be within to be considered overlapping.
	/// Default = 25
	/// </summary>
	public double CommunityStay_MaxOverlapDistance { get; set; } = 25d;

	#endregion
}