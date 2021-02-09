# PolyLineEncoder

Coordinate compression based on https://developers.google.com/maps/documentation/utilities/polylinealgorithm
inspiration taken from https://github.com/mapbox/polyline

```c#
// [38.5, -120.2]
PolyLineEncoder.Encode(new Point(38.5, -120.2), 5)
"_p~iF~ps|U"
```

```c#
// [[38.5, -120.2], [40.7, -120.95], [43.252, -126.453]]
var multiPoint = new MultiPoint(new List<Point>()
{
    new Point(38.5, -120.2),
    new Point(40.7, -120.95),
    new Point(43.252, -126.453),
});
PolyLineEncoder.Encode(multiPoint, 5)
->
"_p~iF~ps|U_ulLnnqC_mqNvxq`@"
```
