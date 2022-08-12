using GeoJSON.Net.Geometry;

using System;
using System.Collections.Generic;
using System.Text;

namespace Yesmey;

public static class PolyLineEncoder
{
	public const int DefaultPrecision = 5;
	private static readonly int _defaultFactor = CalculateFactor(DefaultPrecision);

	/// <summary>
	/// Encodes the values in a <see cref="IPosition" />
	/// </summary>
	/// <param name="position">Position to encode</param>
	/// <returns>Encoded value</returns>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="position" /> is a <see langword="null" /> pointer.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The coordinates in <paramref name="position" /> must be valid</exception>
	public static string Encode(IPosition position)
	{
		if (position is null)
			throw new ArgumentNullException(nameof(position));

		if (!IsValidCoordinate(position.Latitude) || !IsValidCoordinate(position.Longitude))
			throw new ArgumentOutOfRangeException(nameof(position), "Invalid coordinate must be > -180 and < 180");

		var factor = _defaultFactor;
		return Compress(position.Latitude, 0.0, factor, DefaultPrecision) + Compress(position.Longitude, 0.0, factor, DefaultPrecision);
	}

	/// <summary>
	/// Encodes the values in a <see cref="IPosition" /> with a specified precision
	/// </summary>
	/// <param name="position">Position to encode</param>
	/// <param name="precision">Decimal precision</param>
	/// <returns>Encoded value</returns>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="position" /> is a <see langword="null" /> pointer.</exception> 
	/// <exception cref="T:System.ArgumentOutOfRangeException">The coordinates in <paramref name="position" /> must be valid</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="precision" /> is negative or higher than 9</exception>
	public static string Encode(IPosition position, int precision)
	{
		if (precision < 0 || precision > 9)
			throw new ArgumentOutOfRangeException(nameof(precision), "Value must be between 1 and 9");

		if (position is null)
			throw new ArgumentNullException(nameof(position));

		if (!IsValidCoordinate(position.Latitude) || !IsValidCoordinate(position.Longitude))
			throw new ArgumentOutOfRangeException(nameof(position), "Invalid coordinate must be > -180 and < 180");

		var factor = CalculateFactor(precision);
		return Compress(position.Latitude, 0.0, factor, precision) + Compress(position.Longitude, 0.0, factor, precision);
	}

	/// <summary>
	/// Encodes all coordinates from a GeoJSON <see cref="LineString" /> geometry
	/// </summary>
	/// <param name="lineString">LineString to encode</param>
	/// <returns>Encoded value</returns>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="lineString" /> is a <see langword="null" /> pointer.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">All coordinates in <paramref name="lineString" /> must be valid</exception>
	public static string Encode(LineString lineString)
	{
		if (lineString is null)
			throw new ArgumentNullException(nameof(lineString));

		return EncodeLineString(lineString, _defaultFactor, DefaultPrecision);
	}

	/// <summary>
	/// Encodes all coordinates from a GeoJSON <see cref="LineString" /> geometry with a specified precision
	/// </summary>
	/// <param name="lineString">LineString to encode</param>
	/// <param name="precision">Decimal precision</param>
	/// <returns>Encoded value</returns>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="lineString" /> is a <see langword="null" /> pointer.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">All coordinates in <paramref name="lineString" /> must be valid</exception> 
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="precision" /> is negative or higher than 9</exception>
	public static string Encode(LineString lineString, int precision)
	{
		if (lineString is null)
			throw new ArgumentNullException(nameof(lineString));

		return EncodeLineString(lineString, CalculateFactor(precision), precision);
	}

	/// <summary>
	/// Decodes all coordinates in a string to a GeoJSON <see cref="LineString" /> geometry
	/// </summary>
	/// <param name="coords">string to decode</param>
	/// <returns>GeoJSON representation of coordinates from input</returns>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="coords" /> is a <see langword="null" /> pointer.</exception> 
	public static LineString Decode(ReadOnlySpan<char> coords)
	{
		if (coords == null)
			throw new ArgumentNullException(nameof(coords));

		return DecodeString(coords, _defaultFactor);
	}

	/// <summary>
	/// Decodes all coordinates in a string to a GeoJSON <see cref="LineString" /> geometry
	/// </summary>
	/// <param name="coords">string to decode</param>
	/// <param name="precision">Decimal precision</param>
	/// <returns>GeoJSON representation of coordinates from input</returns>
	/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="coords" /> is a <see langword="null" /> pointer.</exception> 
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="precision" /> is negative or higher than 9</exception>
	public static LineString Decode(ReadOnlySpan<char> coords, int precision)
	{
		if (coords == null)
			throw new ArgumentNullException(nameof(coords));

		return DecodeString(coords, CalculateFactor(precision));
	}

	private static string EncodeLineString(LineString line, int factor, int precision)
	{
		var coordinates = line.Coordinates;
		if (coordinates.Count == 0)
			return string.Empty;

		if (!IsValidCoordinate(coordinates[0].Latitude) || !IsValidCoordinate(coordinates[0].Longitude))
			throw new ArgumentOutOfRangeException(nameof(line), "Invalid coordinate must be > -180 and < 180");

		var stringBuilder = new StringBuilder();
		stringBuilder.Append(Compress(coordinates[0].Latitude, 0, factor, precision));
		stringBuilder.Append(Compress(coordinates[0].Longitude, 0, factor, precision));

		for (var i = 1; i < coordinates.Count; i++)
		{
			var latitude = coordinates[i].Latitude;
			var longitude = coordinates[i].Longitude;

			if (!IsValidCoordinate(latitude) || !IsValidCoordinate(longitude))
				throw new ArgumentOutOfRangeException(nameof(line), "Invalid coordinate must be > -180 and < 180");

			var prevLatitude = coordinates[i - 1].Latitude;
			var prevLongitude = coordinates[i - 1].Longitude;

			stringBuilder.Append(Compress(latitude, prevLatitude, factor, precision));
			stringBuilder.Append(Compress(longitude, prevLongitude, factor, precision));
		}

		return stringBuilder.ToString();
	}

	private static string Compress(double current, double previous, int factor, int precision)
	{
		current = Math.Round(current * factor);
		previous = Math.Round(previous * factor);

		ulong coordinate = (ulong)(current - previous);
		coordinate <<= 1;
		if (current - previous < 0)
			coordinate = ~coordinate;

		Span<char> chars = stackalloc char[precision + 5];
		var pos = 0;
		while (coordinate >= 0x20)
		{
			chars[pos++] = (char)((0x20 | (coordinate & 0x1F)) + 63);
			coordinate >>= 5;
		}

		chars[pos++] = (char)(coordinate + 63);
		return chars[..pos].ToString();
    }

	private static LineString DecodeString(ReadOnlySpan<char> coords, int factor)
	{
		var points = new List<Position>(coords.Length);
		for (var i = 0; i < coords.Length; i++)
		{
			double latitude = Decompress(coords, factor, ref i);
			double longitude;
			if (i < coords.Length)
				longitude = Decompress(coords, factor, ref i);
			else
				longitude = double.NaN;

			if (points.Count > 0)
			{
				Position prevPoint = points[^1];
				latitude += prevPoint.Latitude;
				longitude += prevPoint.Longitude;
			}

			points.Add(new Position(latitude, longitude));
		}

		return new LineString(points);
	}

	private static double Decompress(ReadOnlySpan<char> coords, int factor, ref int i)
	{
		ulong result = 0;
		int shift = 0;
		char c;
		do
		{
			if (i >= coords.Length)
				return double.NaN;

			c = coords[i++];
			c -= (char)63;
			result |= ((ulong)(c & 0x1f) << shift);
			shift += 5;
		} while (c >= 0x20);

		if (result < 0)
		{
			result = ~result;
		}

		result >>= 1;
		return (double)result / factor;
	}

	private static int CalculateFactor(int precision) => (int)Math.Pow(10.0, precision);
	private static bool IsValidCoordinate(double value) => value is <= 180.0 and >= -180.0;
}
