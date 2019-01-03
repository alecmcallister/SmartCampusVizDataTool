using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a single user in the data, along with all data points that belong to them
/// </summary>
public class Participant : IComparable<Participant>
{
	public int ID { get; set; }

	public List<DataPoint> Points { get; set; } = new List<DataPoint>();
	public List<StayPoint> StayPoints { get; set; } = new List<StayPoint>();
	public List<Path> Paths { get; set; } = new List<Path>();

	public Participant() : this(-1) { }
	public Participant(int id)
	{
		ID = id;
	}

	#region Add/ remove points

	public void AddDataPoint(DataPoint point)
	{
		if (!Points.Contains(point))
			Points.Add(point);
	}

	public void RemoveDataPoint(DataPoint point)
	{
		if (Points.Contains(point))
			Points.Remove(point);
	}

	#endregion

	/// <summary>
	/// Calculate the staypoints for this user
	/// Populates the <see cref="StayPoints"/> list
	/// </summary>
	public void CalculateStayPoints()
	{
		StayPoints.Clear();

		bool pointsCanExistInMultipleStayPoints = false;

		Points.Sort();

		// Go through the sequence of points, and populate the staypoints list
		foreach (DataPoint point in Points)
		{
			bool flag = false;

			foreach (StayPoint staypoint in StayPoints)
				if (flag |= staypoint.ConditionalAddPoint(point))
					if (!pointsCanExistInMultipleStayPoints)
						break;

			if (!flag)
				StayPoints.Add(new StayPoint(point, StayPoints.Count));
		}

		// Minumum number of points in the area to be considered a staypoint
		// default = 4
		int minPoints = 4;

		// Maybe make the selection based on duration as well??
		StayPoints = StayPoints.Where(s => s.Contents.Count > minPoints).ToList();

		// Re-assign IDs
		StayPoints.ForEach(s => s.StayPointID = StayPoints.IndexOf(s));
	}

	/// <summary>
	/// Calculate the paths for this user
	/// Populates the <see cref="Paths"/> list
	/// </summary>
	public void CalculatePaths()
	{
		Paths.Clear();

		Path currentPath = null;

		foreach (DataPoint point in Points)
		{
			currentPath = currentPath ?? new Path(point, Paths.Count);

			// Try to append point to the current path
			if (!currentPath.ConditionalAddPoint(point))
			{
				if (currentPath.Segments >= Affectors.Path_MinSegments)
				{
					currentPath.StartStayPoint = GetStayPointForDataPoint(currentPath.StartPoint);
					currentPath.EndStayPoint = GetStayPointForDataPoint(currentPath.EndPoint);
					Paths.Add(currentPath);
				}

				currentPath = new Path(point, Paths.Count);
			}
		}
	}

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

	public int CompareTo(Participant other)
	{
		return ID.CompareTo(other.ID);
	}
}
