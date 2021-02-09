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
			Assert.Equal("_p~iF~ps|U", encoder.Encode(new Point(38.5, -120.2)));
		}

		[Fact]
		public void Test_Encode_Static_Point()
		{
			Assert.Equal("_p~iF~ps|U", PolyLineEncoder.Encode(new Point(38.5, -120.2), 5));
		}

		[Fact]
		public void Test_Encode_MultiPoint()
		{
			var multiPoint = new MultiPoint(new List<Point>()
			{
				new Point(38.5, -120.2),
				new Point(40.7, -120.95),
				new Point(43.252, -126.453),
			});

			var encoder = new PolyLineEncoder();
			Assert.Equal("_p~iF~ps|U_ulLnnqC_mqNvxq`@", encoder.Encode(multiPoint));
		}

		[Fact]
		public void Test_Decode()
		{
			var multiPoint = new MultiPoint(new List<Point>()
			{
				new Point(38.5, -120.2),
				new Point(40.7, -120.95),
				new Point(43.252, -126.453),
			});

			var decoder = new PolyLineEncoder();
			var points = decoder.Decode("_p~iF~ps|U_ulLnnqC_mqNvxq`@");
			Assert.NotStrictEqual(multiPoint, points);
		}
	}
}