using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// A simple Vector2 class that uses decimals instead of doubles (128b vs 64b precision).
/// Needed extra precision for lat/long calculations.
/// </summary>
public class Vector2
{
	public decimal X { get; set; } = 0m;
	public decimal Y { get; set; } = 0m;

	public decimal magnitude => Sqrt((X * X) + (Y * Y));

	public Vector2 normalized => (magnitude == 0) ? new Vector2(0m, 0m) : this / magnitude;

	public Vector2() : this(0m, 0m) { }
	public Vector2(decimal x, decimal y)
	{
		X = x;
		Y = y;
	}

	#region Operators

	public static Vector2 operator +(Vector2 a, Vector2 b)
	{
		return new Vector2(a.X + b.X, a.Y + b.Y);
	}

	public static Vector2 operator -(Vector2 a, Vector2 b)
	{
		return new Vector2(a.X - b.X, a.Y - b.Y);
	}

	public static Vector2 operator *(Vector2 a, decimal b)
	{
		return new Vector2(a.X * b, a.Y * b);
	}

	public static Vector2 operator /(Vector2 a, decimal b)
	{
		return new Vector2(a.X / b, a.Y / b);
	}

	public static Vector2 operator -(Vector2 a)
	{
		return a * -1m;
	}

	public static decimal Dot(Vector2 lhs, Vector2 rhs)
	{
		return (lhs.X * rhs.X) + (lhs.Y * rhs.Y);
	}

	public static double Azimuth(Vector2 from, Vector2 to)
	{
		return DegreeBearing((double)from.X, (double)from.Y, (double)to.X, (double)to.Y);
	}

	public double DistanceTo(Vector2 other)
	{
		return AzimuthDistance(this, other);
	}

	/// <summary>
	/// https://stackoverflow.com/questions/2042599/direction-between-2-latitude-longitude-points-in-c-sharp
	/// </summary>
	#region

	static double DegreeBearing(double lat1, double lon1, double lat2, double lon2)
	{
		double dLon = ToRad(lon2 - lon1);
		double dPhi = Math.Log(Math.Tan(ToRad(lat2) / 2d + Math.PI / 4d) / Math.Tan(ToRad(lat1) / 2d + Math.PI / 4d));

		if (Math.Abs(dLon) > Math.PI)
			dLon = dLon > 0 ? -(2d * Math.PI - dLon) : (2d * Math.PI + dLon);

		return ToBearing(Math.Atan2(dLon, dPhi));
	}

	public static double ToRad(double degrees)
	{
		return degrees * (Math.PI / 180d);
	}

	public static double ToDegrees(double radians)
	{
		return radians * (180d / Math.PI);
	}

	public static double ToBearing(double radians)
	{
		return (ToDegrees(radians) + 360d) % 360d;
	}

	/// <summary>
	/// https://www.movable-type.co.uk/scripts/latlong.html
	/// </summary>
	public static double AzimuthDistance(Vector2 from, Vector2 to)
	{
		double R = 6371e3;
		double lat1 = ToRad((double)from.X);
		double lat2 = ToRad((double)to.X);
		double dlat = ToRad((double)(to.X - from.X));
		double dlon = ToRad((double)(to.Y - from.Y));

		double a = Math.Sin(dlat / 2) * Math.Sin(dlat / 2) +
				Math.Cos(lat1) * Math.Cos(lat2) *
				Math.Sin(dlon / 2) * Math.Sin(dlon / 2);

		double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

		double d = R * c;
		return d;
	}

	#endregion

	public static Vector2 Centroid(List<Vector2> list)
	{
		Vector2 result = zero;
		list.ForEach(x => result += x);
		result /= list.Count;
		return result;
	}

	public override string ToString()
	{
		return "(" + X + ", " + Y + ")";
	}

	#endregion

	#region Helper

	public static Vector2 zero => new Vector2(0m, 0m);
	public static Vector2 one => new Vector2(1m, 1m);
	public static Vector2 up => new Vector2(0m, 1m);
	public static Vector2 down => -up;
	public static Vector2 right => new Vector2(1m, 0m);
	public static Vector2 left => -right;

	#endregion

	#region Maths

	public bool IsWithinDistance(Vector2 other, double distance)
	{
		return AzimuthDistance(this, other) < distance;
	}

	public static Vector2 Average(List<Vector2> vectors)
	{
		Vector2 result = zero;

		vectors.ForEach(x => result += x);

		return result / vectors.Count;
	}

	/// <summary>
	/// https://stackoverflow.com/questions/4124189/performing-math-operations-on-decimal-datatype-in-c
	/// The result of the calculations will differ from an actual value of the root on less than epslion.
	/// </summary>
	/// <param name="x">A number, from which we need to calculate the square root.</param>
	/// <param name="epsilon">An accuracy of calculation of the root from our number.</param>
	/// <returns>The square root of x</returns>
	public static decimal Sqrt(decimal x, decimal epsilon = 0.0M)
	{
		if (x < 0)
			throw new OverflowException("Cannot calculate square root from a negative number");

		decimal current = (decimal)Math.Sqrt((double)x), previous;
		do
		{
			previous = current;

			if (previous == 0.0M)
				return 0;

			current = (previous + x / previous) / 2;
		}
		while (Math.Abs(previous - current) > epsilon);
		return current;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
			return false;

		Vector2 vec = (Vector2)obj;

		return X == vec.X && Y == vec.Y;
	}

	public bool EssentiallyEquals(Vector2 b)
	{
		double dist = Affectors.Instance.Path_EssentiallyEqualsDistance;
		return (Math.Abs((double)(X - b.X)) < dist) && (Math.Abs((double)(Y - b.Y)) < dist);
	}

	#endregion
}
