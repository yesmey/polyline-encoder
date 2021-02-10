# PolyLineEncoder

Coordinate compression based on https://developers.google.com/maps/documentation/utilities/polylinealgorithm
inspiration taken from https://github.com/mapbox/polyline

Supports GeoJSON.NET LineString and Position

```c#
// [38.5, -120.2]
new PolyLineEncoder().Encode(new Position(38.5, -120.2))
"_p~iF~ps|U"
```

```c#
// [[38.5, -120.2], [40.7, -120.95], [43.252, -126.453]]
var lineString = new LineString(new List<Position>()
{
    new Position(38.5, -120.2),
    new Position(40.7, -120.95),
    new Position(43.252, -126.453),
});
new PolyLineEncoder().Encode(lineString)
->
"_p~iF~ps|U_ulLnnqC_mqNvxq`@"
```
