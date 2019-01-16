using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Represents a single user in the data, along with all data points that belong to them
/// </summary>
public class Participant : IComparable<Participant>
{
	public int ID { get; set; }

	public List<StayPoint> StayPoints { get; set; } = new List<StayPoint>();
	public List<Path> Paths { get; set; } = new List<Path>();

	public ConcurrentBag<DataPoint> Points { get; set; } = new ConcurrentBag<DataPoint>();

	public Participant() : this(-1) { }
	public Participant(int id)
	{
		ID = id;
	}

	#region Add points

	/// <summary>
	/// Adds the point to our collection.
	/// </summary>
	/// <param name="point">The point to add</param>
	public void AddDataPoint(DataPoint point)
	{
		Points.Add(point);
	}

	#endregion

	#region Staypoints

	/// <summary>
	/// Calculate the staypoints for this user.
	/// Populates the <see cref="StayPoints"/> list.
	/// </summary>
	public void CalculateStayPoints()
	{
		StayPoints.Clear();

		List<DataPoint> points = Points.ToList();
		points.Sort();

		// Go through the sequence of points, and populate the staypoints list
		foreach (DataPoint point in points)
		{
			bool flag = false;

			foreach (StayPoint staypoint in StayPoints)
				if (flag |= staypoint.ConditionalAddPoint(point))
					if (!Affectors.Stay_PointsCanExistInMultipleStayPoints)
						break;

			if (!flag)
				StayPoints.Add(new StayPoint(point, StayPoints.Count));
		}
	}

	#endregion

	#region Paths

	/// <summary>
	/// Calculate the paths for this user
	/// Populates the <see cref="Paths"/> list
	/// </summary>
	public void CalculatePaths()
	{
		Paths.Clear();

		Path currentPath = null;

		List<DataPoint> points = Points.ToList();
		points.Sort();

		foreach (DataPoint point in points)
		{
			currentPath = currentPath ?? new Path(point, Paths.Count);

			// Try to append point to the current path
			if (!currentPath.ConditionalAddPoint(point))
			{
				if (currentPath.Segments >= Affectors.Path_MinSegments)
					Paths.Add(currentPath);

				currentPath = new Path(point, Paths.Count);
			}
		}
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Gets the staypoint id that this point resides in (-2 if none)
	/// </summary>
	/// <param name="point"></param>
	/// <returns>The staypointID of the area, or -2 if none</returns>
	public int GetStayPointForDataPoint(DataPoint point)
	{
		int stayPointID = -2;

		foreach (StayPoint stayPoint in StayPoints)
		{
			if (stayPoint.Contents.Contains(point))
			{
				stayPointID = stayPoint.StayPointID;
				break;
			}
		}

		return stayPointID;
	}

	#endregion

	#region Compare

	/// <summary>
	/// Compares the participant to another, based on their <see cref="Participant.ID"/>.
	/// </summary>
	/// <param name="other">Who to compare against</param>
	/// <returns>I'm never actually sure with these methods...</returns>
	public int CompareTo(Participant other)
	{
		return ID.CompareTo(other.ID);
	}

	#endregion
}
