namespace DevCrew.SuperCluster;

public class Options
{
    public Options
    (
        Func<dynamic, ClusterProperties> map,
        int minZoom = default, 
        int maxZoom = default,
        int minPoints = default, 
        int radius = default,
        int extent = default, 
        int nodeSize = default, 
        bool log = default, 
        bool generateId = default,
        Func<dynamic, dynamic, int>? reduce = null
    )
    {
        MinZoom = minZoom;
        MaxZoom = maxZoom;
        MinPoints = minPoints;
        Radius = radius;
        Extent = extent;
        NodeSize = nodeSize;
        Log = log;
        GenerateId = generateId;
        Reduce = reduce;
        Map = map;
    }

    public int MinZoom { get; set; }
    public int MaxZoom { get; set; }
    public int MinPoints { get; set; }
    public int Radius { get; set; }
    public int Extent { get; set; }
    public int NodeSize { get; set; }
    public bool Log { get; set; }
    public bool GenerateId { get; set; }
    public Func<dynamic, dynamic, int>? Reduce { get; set; }
    public Func<dynamic, ClusterProperties> Map { get; set; }
}