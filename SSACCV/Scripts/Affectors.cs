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
	#region Stay Points



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
	public const int Path_MinSegments = 2;

	/// <summary>
	/// Maximum amount of time (minutes) allowed when calculating the contiguity of subsequent points.
	/// Points occuring later than this value will be excluded from the current path.
	/// Default = 30
	/// </summary>
	public const double Path_SubsequentPointTimeCutoff = 20d;

	/// <summary>
	/// Minumum distance (meters?) required when calculating the contiguity of subsequent points.
	/// Points located closer than this value will be excluded from the current path.
	/// Default = 2
	/// </summary>
	public const decimal Path_SubsequentDistanceThreshold = 5m;

	#endregion

}
