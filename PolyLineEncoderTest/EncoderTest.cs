using GeoJSON.Net.Geometry;
using PolyLine;
using System.Collections.Generic;
using Xunit;

namespace PolyLineEncoderTest
{
	public class EncoderTest
	{
		[Fact]
		public void Test_Encode_Point()
		{
			var encoder = new PolyLineEncoder();
			Assert.Equal("_p~iF~ps|U", encoder.Encode(new Position(38.5, -120.2)));
		}

		[Fact]
		public void Test_Encode_Static_Point()
		{
			var encoder = new PolyLineEncoder();
			Assert.Equal("_p~iF~ps|U", encoder.Encode(new Position(38.5, -120.2)));
		}

		[Fact]
		public void Test_Encode_MultiPoint()
		{
			var multiPoint = new LineString(new List<Position>()
			{
				new Position(38.5, -120.2),
				new Position(40.7, -120.95),
				new Position(43.252, -126.453),
			});

			var encoder = new PolyLineEncoder();
			Assert.Equal("_p~iF~ps|U_ulLnnqC_mqNvxq`@", encoder.Encode(multiPoint));
		}

		[Fact]
		public void Test_Decode()
		{
			var multiPoint = new LineString(new List<Position>()
			{
				new Position(38.5, -120.2),
				new Position(40.7, -120.95),
				new Position(43.252, -126.453),
			});

			var decoder = new PolyLineEncoder();
			var points = decoder.Decode("_p~iF~ps|U_ulLnnqC_mqNvxq`@");
			Assert.NotStrictEqual(multiPoint, points);
		}
	}
}