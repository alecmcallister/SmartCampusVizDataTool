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
	public decimal lon_start { get; set; }
	public decimal lat_start { get; set; }
	public decimal yyc_x_start { get; set; }
	public decimal yyc_y_start { get; set; }
	public int accuracy { get; set; }
	public int loct_year { get; set; }
	public int loct_month { get; set; }
	public int loct_julian { get; set; }
	public int loct_dow { get; set; }
	public DateTime datetime_start { get; set; }
	public int yyc { get; set; }
	public DateTime datetime_end { get; set; }
	public decimal yyc_x_end { get; set; }
	public decimal yyc_y_end { get; set; }
	public double steplength { get; set; }
	public int id { get; set; }
	public string geom_start { get; set; }
	public string study_area { get; set; }
	public string used { get; set; }
	public double time_interval { get; set; }
	public double speed { get; set; }
	public int objectid_1 { get; set; }

	/// <summary>
	/// Where the point started
	/// </summary>
	public Vector2 yycStart => new Vector2(yyc_x_start, yyc_y_start);

	/// <summary>
	/// Where the point ended
	/// PROBABLY DON'T WANT TO USE THIS VALUE EVER
	/// </summary>
	public Vector2 yycEnd => new Vector2(yyc_x_end, yyc_y_end);

	/// <summary>
	/// The delta vector from <see cref="yycStart"/> to <see cref="yycEnd"/>
	/// </summary>
	public Vector2 yycDelta => yycEnd - yycStart;

	/// <summary>
	/// How long from start to finish
	/// </summary>
	public TimeSpan timeSpan => datetime_end - datetime_start;

	// Probably m/s? Depends on what unit is used by yyc_whatever data
	public decimal velocity => yycDelta.magnitude / (decimal)timeSpan.TotalSeconds;

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
			return DateTime.Compare(datetime_start, other.datetime_start);
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
		return (b.datetime_start - a.datetime_start).TotalMinutes;
	}

	/// <summary>
	/// Get the distance to <paramref name="b"/>
	/// </summary>
	/// <param name="b">The other point</param>
	/// <returns>The distance between the two points (in meters probably)</returns>
	public decimal DistanceTo(DataPoint b)
	{
		return (b.yycStart - yycStart).magnitude;
	}

	#endregion
}
