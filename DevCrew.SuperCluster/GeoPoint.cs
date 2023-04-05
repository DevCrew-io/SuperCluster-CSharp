namespace DevCrew.SuperCluster;

public class GeoPoint : IGeoJsonFeature
{
    public double Longitude { get; }
    public double Latitude { get; }
    
    public GeoPoint(double longitude, double latitude)
    {
        if (longitude is not (>= -180 and <= 180))
        {
            throw new ArgumentException(message: "longitude value should be in range of [-180, 180]");
        }
        if (latitude is not (>= -90 and <= 90))
        {
            throw new ArgumentException(message: "latitude value should be in range of [-90, 90]");
        }
        
        Longitude = longitude;
        Latitude = latitude;
    }
}