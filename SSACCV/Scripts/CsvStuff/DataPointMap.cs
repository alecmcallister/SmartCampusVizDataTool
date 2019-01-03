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
		Map(m => m.lon_start).Name("lon_start");
		Map(m => m.lat_start).Name("lat_start");
		Map(m => m.yyc_x_start).Name("yyc_x_start");
		Map(m => m.yyc_y_start).Name("yyc_y_start");
		Map(m => m.accuracy).Name("accuracy");
		Map(m => m.loct_year).Name("loct_year");
		Map(m => m.loct_month).Name("loct_month");
		Map(m => m.loct_julian).Name("loct_julian");
		Map(m => m.loct_dow).Name("loct_dow");
		Map(m => m.datetime_start).Name("datetime_start");
		Map(m => m.yyc).Name("yyc");
		Map(m => m.datetime_end).Name("datetime_end");
		Map(m => m.yyc_x_end).Name("yyc_x_end").ConvertUsing(row => TryConvertDecimal(row, "yyc_x_end", 0m));
		Map(m => m.yyc_y_end).Name("yyc_y_end").ConvertUsing(row => TryConvertDecimal(row, "yyc_y_end", 0m));
		Map(m => m.steplength).Name("steplength").ConvertUsing(row => TryConvertDouble(row, "steplength", 0d));
		Map(m => m.id).Name("id");
		Map(m => m.geom_start).Name("geom_start");
		Map(m => m.study_area).Name("study_area");
		Map(m => m.used).Name("used");
		Map(m => m.time_interval).Name("time_interval");
		Map(m => m.speed).Name("speed").ConvertUsing(row => TryConvertDouble(row, "speed", 0f));
		Map(m => m.objectid_1).Name("objectid_1");
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
