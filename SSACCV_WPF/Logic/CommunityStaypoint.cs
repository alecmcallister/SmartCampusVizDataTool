using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class CommunityStaypoint
{
	public List<Staypoint> Contents { get; set; } = new List<Staypoint>();

	public Vector2 Location { get; set; }
	public Vector2 Centroid { get { return Vector2.Centroid(Contents.Select(sp => sp.Location).ToList()); } }

	//public double CombinedScore => Contents.Sum(sp => sp.)

	public CommunityStaypoint(Staypoint sp)
	{
		ConditionalAddStaypoint(sp);
	}

	public bool ConditionalAddStaypoint(Staypoint sp)
	{
		if (Contents.Count == 0)
		{
			Contents.Add(sp);
			Location = sp.Centroid;
			return true;
		}

		if (Vector2.AzimuthDistance(Location, sp.Centroid) < Affectors.Instance.CommunityStay_MaxOverlapDistance)
		{
			Contents.Add(sp);
			return true;
		}

		return false;
	}

	public List<CommunityStaypointOutput> GetOutput()
	{
		List<CommunityStaypointOutput> output = new List<CommunityStaypointOutput>();

		// Group by day I guess?




		return output;
	}
}