using GeoJSON.Net.Geometry;

using System;
using System.Collections.Generic;
using System.Text;

namespace PolyLine
{
	public class PolyLineEncoder
	{
		private int _factor;
		private int _precision;

		/// <summary>
		/// Initializes a new <see cref="PolyLineEncoder" /> used to lossy compress/decompress a series of coordinates as a single string
		/// </summary>
		public PolyLineEncoder() : this(5)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PolyLineEncoder" /> class.
		/// </summary>
		/// <param name="precision">Decimal precision</param>
		public PolyLineEncoder(int precision)
		{
			Precision = precision;
		}

		/// <summary>
		/// Gets or sets the decimal precision
		/// </summary>
		/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="Precision" /> is negative or higher than 9</exception> 
		public int Precision
		{
			get => _precision;
			set
			{
				if (value < 0 || value > 9)
				{
					throw new ArgumentOutOfRangeException(nameof(Precision), "Value must be between 1 and 9");
				}

				_precision = value;
				_factor = CalculateFactor(_precision);
			}
		}

		/// <summary>
		/// Encodes the values in a <see cref="IPosition" />
		/// </summary>
		/// <param name="position">Position to encode</param>
		/// <returns>Encoded value</returns>
		/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="position" /> is a <see langword="null" /> pointer.</exception> 
		/// <exception cref="T:System.ArgumentOutOfRangeException">The coordinates in <paramref name="position" /> must be valid</exception> 
		public string Encode(IPosition position)
		{
			if (position is null)
				throw new ArgumentNullException(nameof(position));

			if (!IsValidCoordinate(position.Latitude) || !IsValidCoordinate(position.Longitude))
				throw new ArgumentOutOfRangeException(nameof(Precision), "Invalid coordinate must be > -180 and < 180");

			return Compress(position.Latitude, 0.0, _factor) + Compress(position.Longitude, 0.0, _factor);
		}

		/// <summary>
		/// Encodes all coordinates from a GeoJSON <see cref="LineString" /> geometry
		/// </summary>
		/// <param name="lineString">LineString to encode</param>
		/// <returns>Encoded value</returns>
		/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="lineString" /> is a <see langword="null" /> pointer.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">All coordinates in <paramref name="lineString" /> must be valid</exception> 
		public string Encode(LineString lineString)
		{
			if (lineString is null)
				throw new ArgumentNullException(nameof(lineString));

			return EncodeLineString(lineString, _factor);
		}

		/// <summary>
		/// Decodes all coordinates in a string to a GeoJSON <see cref="LineString" /> geometry
		/// </summary>
		/// <param name="coords">string to decode</param>
		/// <returns>GeoJSON representation of coordinates from input</returns>
		/// <exception cref="T:System.ArgumentNullException">The address of <paramref name="coords" /> is a <see langword="null" /> pointer.</exception> 
		public LineString Decode(ReadOnlySpan<char> coords)
		{
			if (coords == null)
				throw new ArgumentNullException(nameof(coords));

			return DecodeString(coords, _factor);
		}

		private static string EncodeLineString(LineString line, int factor)
		{
			var coordinates = line.Coordinates;
			if (coordinates.Count == 0)
				return string.Empty;

			if (!IsValidCoordinate(coordinates[0].Latitude) || !IsValidCoordinate(coordinates[0].Longitude))
				throw new ArgumentOutOfRangeException(nameof(Precision), "Invalid coordinate must be > -180 and < 180");

			var stringBuilder = new StringBuilder();
			stringBuilder.Append(Compress(coordinates[0].Latitude, 0, factor));
			stringBuilder.Append(Compress(coordinates[0].Longitude, 0, factor));

			for (var i = 1; i < coordinates.Count; i++)
			{
				var latitude = coordinates[i].Latitude;
				var longitude = coordinates[i].Longitude;

				if (!IsValidCoordinate(latitude) || !IsValidCoordinate(longitude))
					throw new ArgumentOutOfRangeException(nameof(Precision), "Invalid coordinate must be > -180 and < 180");

				var prevLatitude = coordinates[i - 1].Latitude;
				var prevLongitude = coordinates[i - 1].Longitude;

				stringBuilder.Append(Compress(latitude, prevLatitude, factor));
				stringBuilder.Append(Compress(longitude, prevLongitude, factor));
			}

			return stringBuilder.ToString();
		}

		private static string Compress(double current, double previous, int factor)
		{
			current = Math.Round(current * factor);
			previous = Math.Round(previous * factor);

			var coordinate = (int)(current - previous);
			coordinate <<= 1;
			if (current - previous < 0)
				coordinate = ~coordinate;

			Span<char> chars = stackalloc char[5 + factor];
			var pos = 0;
			while (coordinate >= 0x20)
			{
				chars[pos++] = (char)((0x20 | (coordinate & 0x1F)) + 63);
				coordinate >>= 5;
			}

			chars[pos++] = (char)(coordinate + 63);
			return chars.Slice(0, pos).ToString();
		}

		private static LineString DecodeString(ReadOnlySpan<char> coords, int factor)
		{
			var points = new List<Position>();
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
					var prevPoint = points[^1];
					latitude += prevPoint.Latitude;
					longitude += prevPoint.Longitude;
				}

				points.Add(new Position(latitude, longitude));
			}

			return new LineString(points);
		}

		private static double Decompress(ReadOnlySpan<char> coords, int factor, ref int i)
		{
			int result = 0;
			int shift = 0;
			char c;
			do
			{
				if (i < coords.Length)
				{
					c = coords[i++];
					c -= (char)63;
					result |= (c & 0x1f) << shift;
					shift += 5;
				}
				else
				{
					return double.NaN;
				}
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
}
