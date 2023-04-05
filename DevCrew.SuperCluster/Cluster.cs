namespace DevCrew.SuperCluster;

internal class Cluster
{
    public Cluster(double x , double y, int id = default, int index = default, int numPoints = default, ClusterProperties properties = default, int zoom = int.MaxValue, int parentId = -1)
    {
        X = x;
        Y = y;
        Zoom = zoom;
        Id = id;
        Index = index;
        ParentId = parentId;
        NumPoints = numPoints;
        Properties = properties;
    }

    public double X { get; }
    public double Y { get; }
    public int Zoom { get; set; }
    public int Id { get; }
    public int Index { get; }
    public int NumPoints { get; }
    public int ParentId { get; set; }
    public ClusterProperties Properties { get; }
}