using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DecimalConvert : ITypeConverter
{
	public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
	{
		if (text == "NA")
			return 0m;

		decimal val = 0m;
		decimal.TryParse(text, out val);
		return val;
	}

	public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
	{
		return value.ToString();
	}
}

public class DoubleConvert : ITypeConverter
{
	public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
	{
		if (text == "NA")
			return 0d;

		double val = 0d;
		double.TryParse(text, out val);
		return val;
	}

	public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
	{
		return value.ToString();
	}
}

public class DateConverter : ITypeConverter
{
	public CultureInfo Culture { get; set; } = new CultureInfo("en-EN", false);

	public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
	{
		DateTime result = DateTime.MinValue;
		result = DateTime.Parse(text, Culture);

		return result;
	}

	public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
	{
		return value.ToString();
	}
}
