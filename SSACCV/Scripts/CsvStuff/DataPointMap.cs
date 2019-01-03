using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Maps columns in the csv file to fields in a new <see cref="DataPoint"/> object
/// </summary>
public sealed class DataPointMap : ClassMap<DataPoint>
{
	/// <summary>
	/// General constructor; Initializes the map within here.
	/// </summary>
	public DataPointMap()
	{
		Map(m => m.userid).Name("userid");
		Map(m => m.yyc_x_start).Name("yyc_x_start").ConvertUsing(row => TryConvertDecimal(row, "yyc_x_start", 0m));
		Map(m => m.yyc_y_start).Name("yyc_y_start").ConvertUsing(row => TryConvertDecimal(row, "yyc_y_start", 0m));
		Map(m => m.accuracy).Name("accuracy");
		Map(m => m.loct).Name("loct");
	}

	/// <summary>
	/// Converter for double values.
	/// </summary>
	/// <param name="row">Row</param>
	/// <param name="column">Column</param>
	/// <param name="fallback">The value to return if the converter fails</param>
	/// <returns>The value converted into a double</returns>
	public double TryConvertDouble(IReaderRow row, string column, double fallback)
	{
		double val = fallback;
		double.TryParse(row.GetField(column), out val);
		return val;
	}

	/// <summary>
	/// Converter for decimal values.
	/// </summary>
	/// <param name="row">Row</param>
	/// <param name="column">Column</param>
	/// <param name="fallback">The value to return if the converter fails</param>
	/// <returns>The value converted into a decimal</returns>
	public decimal TryConvertDecimal(IReaderRow row, string column, decimal fallback)
	{
		decimal val = fallback;
		decimal.TryParse(row.GetField(column), out val);
		return val;
	}
}
