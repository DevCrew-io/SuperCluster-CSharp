namespace DevCrew.SuperCluster;

public class ClusterJson : IGeoJsonFeature
{
    public ClusterJson(int id, ClusterProperties properties, GeoPoint coordinates, string type = "Feature")
    {
        Type = type;
        Id = id;
        Properties = properties;
        Coordinates = coordinates;
    }

    public string Type { get; }
    public int Id { get; }
    public ClusterProperties Properties { get; }
    public GeoPoint Coordinates { get; }
}