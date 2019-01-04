using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a row of data belonging to a user
/// </summary>
public class DataPoint : IComparable<DataPoint>
{
	public int userid { get; set; }
	public decimal yyc_x_start { get; set; }
	public decimal yyc_y_start { get; set; }
	public int accuracy { get; set; }
	public DateTime loct { get; set; }

	/// <summary>
	/// Where the point started
	/// </summary>
	public Vector2 location => new Vector2(yyc_x_start, yyc_y_start);

	public int staypointID { get; set; } = -1;

	#region Compare

	/// <summary>
	/// Compare ID, and start date
	/// </summary>
	/// <param name="other">The point to compare against</param>
	/// <returns>If different ID: 1 if higher than other, -1 if lower than other. If same ID: </returns>
	public int CompareTo(DataPoint other)
	{
		if (other == null)
			return 1;

		if (userid != other.userid)
			return userid.CompareTo(other.userid);

		else
			return DateTime.Compare(loct, other.loct);
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Get the time period between points <paramref name="a"/> and <paramref name="b"/>
	/// </summary>
	/// <param name="a">Start point (earlier time)</param>
	/// <param name="b">End point (later time)</param>
	/// <returns>The total time (in minutes) between a.datetime_start and b.datetime_start</returns>
	public static double TimeDifference(DataPoint a, DataPoint b)
	{
		return (b.loct - a.loct).TotalMinutes;
	}

	/// <summary>
	/// Get the distance to <paramref name="b"/>
	/// </summary>
	/// <param name="b">The other point</param>
	/// <returns>The distance between the two points (in meters probably)</returns>
	public decimal DistanceTo(DataPoint b)
	{
		return (b.location - location).magnitude;
	}

	#endregion
}
