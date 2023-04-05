namespace DevCrew.SuperCluster;

public class ClusterProperties
{
    public ClusterProperties(int clusterId, int pointCount, String pointCountAbrv, bool cluster = true)
    {
        Cluster = cluster;
        ClusterId = clusterId;
        PointCount = pointCount;
        PointCountAbrv = pointCountAbrv;
    }

    public bool Cluster { get; }
    public int ClusterId { get; }
    public int PointCount { get; }

    public String PointCountAbrv { get; }

    public ClusterProperties Clone()
    {
        return new(
            clusterId: ClusterId,
            pointCount: PointCount,
            pointCountAbrv: PointCountAbrv,
            cluster: Cluster
        );
    }
}