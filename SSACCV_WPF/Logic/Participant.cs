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

	public List<Restpoint> RestPoints { get; set; } = new List<Restpoint>();
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

	public void CleanPoints()
	{
		List<DataPoint> points = Points.ToList();
		points.Sort();
		Points = new ConcurrentBag<DataPoint>(points);
	}

	#endregion

	#region Restpoints

	/// <summary>
	/// Calculate the restpoints for this user.
	/// Populates the <see cref="RestPoints"/> list.
	/// </summary>
	public void CalculateRestPoints()
	{
		RestPoints.Clear();

		List<DataPoint> points = Points.ToList();
		points.Sort();

		// Go through the sequence of points, and populate the restpoints list
		foreach (DataPoint point in points)
		{
			bool flag = false;

			foreach (Restpoint restpoint in RestPoints)
				if (flag |= restpoint.ConditionalAddPoint(point))
					break;

			if (!flag)
				RestPoints.Add(new Restpoint(point, RestPoints.Count));
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
				if (currentPath.Segments >= Affectors.Instance.Path_MinSegments)
					Paths.Add(currentPath);

				currentPath = new Path(point, Paths.Count);
			}
		}
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Gets the restpoint id that this point resides in (-2 if none)
	/// </summary>
	/// <param name="point"></param>
	/// <returns>The restpointID of the area, or -2 if none</returns>
	public int GetRestPointForDataPoint(DataPoint point)
	{
		int restPointID = -2;

		foreach (Restpoint restPoint in RestPoints)
		{
			if (restPoint.Contents.Contains(point))
			{
				restPointID = restPoint.RestPointID;
				break;
			}
		}

		return restPointID;
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
