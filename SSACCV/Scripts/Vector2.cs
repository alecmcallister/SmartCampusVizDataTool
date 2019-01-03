using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

	public bool IsWithinDistance(Vector2 other, decimal distance)
	{
		return (other - this).magnitude < distance;
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

	#endregion
}
