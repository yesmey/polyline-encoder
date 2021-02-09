using System;
using System.Collections.Generic;
using System.Text;

namespace PolyLine
{
	public record Point(double Latitude, double Longitude);
	public record MultiPoint(List<Point> Points);

	public class PolyLineEncoder
	{
		private int _factor;
		private int _precision;

		public int Precision
		{
			get => _precision;
			set
			{
				_precision = value;
				_factor = CalculateFactor(_precision);
			}
		}

		public PolyLineEncoder() : this(5)
		{
		}

		public PolyLineEncoder(int precision)
		{
			Precision = precision;
		}

		public string Encode(Point point)
		{
			return EncodeInternal(point, _factor);
		}

		public static string Encode(Point point, int precision)
		{
			return EncodeInternal(point, CalculateFactor(precision));
		}

		private static string EncodeInternal(Point point, int factor)
		{
			return Encode(point.Latitude, 0.0, factor) + Encode(point.Longitude, 0.0, factor);
		}

		public string Encode(MultiPoint multiPoint)
		{
			return EncodeInternal(multiPoint, _factor);
		}

		public static string Encode(MultiPoint multiPoint, int precision)
		{
			return EncodeInternal(multiPoint, CalculateFactor(precision));
		}

		private static string EncodeInternal(MultiPoint multiPoint, int factor)
		{
			var points = multiPoint.Points;
			if (points.Count == 0)
				return string.Empty;

			var stringBuilder = new StringBuilder();
			stringBuilder.Append(Encode(points[0].Latitude, 0, factor));
			stringBuilder.Append(Encode(points[0].Longitude, 0, factor));

			for (var i = 1; i < points.Count; i++)
			{
				var (latitude, longitude) = points[i];
				var (prevLatitude, prevLongitude) = points[i - 1];

				stringBuilder.Append(Encode(latitude, prevLatitude, factor));
				stringBuilder.Append(Encode(longitude, prevLongitude, factor));
			}

			return stringBuilder.ToString();
		}

		private static string Encode(double current, double previous, int factor)
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

		public MultiPoint Decode(ReadOnlySpan<char> coords)
		{
			return DecodeInternal(coords, _factor);
		}

		public static MultiPoint Decode(ReadOnlySpan<char> coords, int precision)
		{
			return DecodeInternal(coords, CalculateFactor(precision));
		}

		private static MultiPoint DecodeInternal(ReadOnlySpan<char> coords, int factor)
		{
			var points = new List<Point>();
			for (var i = 0; i < coords.Length; i++)
			{
				double latitude = Decode(coords, factor, ref i);
				double longitude = Decode(coords, factor, ref i);

				if (points.Count > 0)
				{
					var prevPoint = points[^1];
					latitude += prevPoint.Latitude;
					longitude += prevPoint.Longitude;
				}

				points.Add(new Point(latitude, longitude));
			}

			return new MultiPoint(points);
		}

		private static double Decode(ReadOnlySpan<char> coords, int factor, ref int i)
		{
			if (i >= coords.Length)
			{
				return double.NaN;
			}

			int result = 0;
			int shift = 0;
			char c = coords[i++];
			while (c >= 0x20)
			{
				c -= (char)63;
				result |= (c & 0x1F) << shift;
				shift += 5;

				if (i + 1 < coords.Length)
				{
					c = coords[i++];
				}
				else
				{
					return double.NaN;
				}
			}

			if (result < 0)
			{
				result = ~result;
			}

			result >>= 1;
			return (double) result / factor;
		}

		private static int CalculateFactor(int precision) =>  (int)Math.Pow(10.0, precision);
	}
}
