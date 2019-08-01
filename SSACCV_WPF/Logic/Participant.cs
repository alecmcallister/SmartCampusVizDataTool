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
	// The users ID
	public int ID { get; set; }

	// The users restpoints
	public List<Restpoint> RestPoints { get; set; } = new List<Restpoint>();

	// The users paths
	public List<Path> Paths { get; set; } = new List<Path>();

	// The points belonging to the user
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

	/// <summary>
	/// Makes sure the points are sorted nicely.
	/// </summary>
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
