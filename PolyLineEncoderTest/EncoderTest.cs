using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using Xunit;
using Yesmey;

namespace PolyLineEncoderTest;

public class EncoderTest
{
    [Fact]
    public void Test_Encode_Position_Error()
    {
        Assert.Throws<ArgumentNullException>(() => PolyLineEncoder.Encode((Position)null));
    }

    [Fact]
    public void Test_Encode_LineString_Error()
    {
        Assert.Throws<ArgumentNullException>(() => PolyLineEncoder.Encode((LineString)null));
    }

    [Fact]
    public void Test_Encode_Position()
    {
        Assert.Equal("_p~iF~ps|U", PolyLineEncoder.Encode(new Position(38.5, -120.2)));
        Assert.Equal("_oo|wufA~~whn{~E", PolyLineEncoder.Encode(new Position(38.5, -120.2), 9));
        Assert.Equal("kAnF", PolyLineEncoder.Encode(new Position(38.5, -120.2), 0));
    }

    [Fact]
    public void Test_Encode_LineString()
    {
        var multiPoint = new LineString(new List<Position>()
        {
            new Position(38.5, -120.2),
            new Position(40.7, -120.95),
            new Position(43.252, -126.453),
        });

        Assert.Equal("_p~iF~ps|U_ulLnnqC_mqNvxq`@", PolyLineEncoder.Encode(multiPoint));
        Assert.Equal("_oo|wufA~~whn{~E__jdcbC~vjouk@__vpbwC~zmc_gI", PolyLineEncoder.Encode(multiPoint, 9));
        Assert.Equal("kAnFE@CH", PolyLineEncoder.Encode(multiPoint, 0));
    }

    [Fact]
    public void Test_Decode_LineString()
    {
        var multiPoint = new LineString(new List<Position>()
        {
            new Position(38.5, -120.2),
            new Position(40.7, -120.95),
            new Position(43.252, -126.453),
        });

        var points = PolyLineEncoder.Decode("_p~iF~ps|U_ulLnnqC_mqNvxq`@");
        Assert.NotStrictEqual(multiPoint, points);
    }

    [Fact]
    public void Test_Decode_Error()
    {
        Assert.Throws<ArgumentNullException>(() => PolyLineEncoder.Decode(null));
    }
}
