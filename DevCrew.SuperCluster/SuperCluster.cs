using Shrulik.NKDBush;

namespace DevCrew.SuperCluster;

public class SuperCluster
{

    private Options Options { get; }

    private readonly KDBush<Cluster>? [] _trees;
    private List<GeoPoint> _points = new();

    public SuperCluster(Options options)
    {
        Options = options;
        _trees = new KDBush<Cluster>[options.MaxZoom + 1];
    }

    public void Load(List<GeoPoint> points)
    {
        var log = Options.Log;
        var minZoom = Options.MinZoom;
        var maxZoom = Options.MaxZoom;
        var nodeSize = Options.NodeSize;

        if (log)
        {
            Console.WriteLine("total time!");
        }

        var timerId = "prepare " + points.Count + " points";
        if (log)
        {
            Console.WriteLine(timerId);
        }

        _points = points;

        // generate a cluster object for each point and index input points into a KD-tree
        var clusters = new List<Cluster>();
        for (int i = 0; i < points.Count; i++)
        {
            clusters.Add(CreatePointCluster(p: points[i], id: i));
        }

        _trees[maxZoom] = new KDBush<Cluster>(clusters.ToArray(), GetX, GetY, nodeSize: nodeSize);

        if (log)
        {
            Console.WriteLine("Finished " + timerId);
        }

        // cluster points on max zoom, then cluster the results on previous zoom, etc.;
        // results in a cluster hierarchy across zoom levels
        for (int z = maxZoom; z >= minZoom; z--)
        {
            //TODO: get current date 
            //const now = +Date.now();
            // create a new set of clusters for the zoom and index them with a KD-tree
            
            clusters = CreateClusters(clusters, z);
            _trees[z] = new KDBush<Cluster>(clusters.ToArray(), GetX, GetY);

            //TODO: print logs
            //if (log) console.log('z%d: %d clusters in %dms', z, clusters.length, +Date.now() - now);
        }
    }

    /// <summary>
    /// returns clusters list a specific zoom value
    /// </summary>
    /// <param name="bbox">an array of [westLng, southLat, eastLng, northLat]</param>
    /// <param name="zoom">zoom level</param>
    public List<IGeoJsonFeature> GetClusters(double[] bbox, int zoom)
    {
        var minLng = ((bbox[0] + 180) % 360 + 360) % 360 - 180;
        var minLat = Math.Max(-90, Math.Min(90, bbox[1]));
        var isMaxLng180 = Math.Abs(bbox[2] - 180) < double.Epsilon;
        var maxLng = isMaxLng180 ? 180 : ((bbox[2] + 180) % 360 + 360) % 360 - 180;
        var maxLat = Math.Max(-90, Math.Min(90, bbox[3]));

        if (bbox[2] - bbox[0] >= 360)
        {
            minLng = -180;
            maxLng = 180;
        }
        else if (minLng > maxLng)
        {
            var easternHem = GetClusters(new[] { minLng, minLat, 180, maxLat }, zoom);
            var westernHem = GetClusters(new[] { -180, minLat, maxLng, maxLat }, zoom);
            return easternHem.Concat(westernHem).ToList();
        }

        var tree = _trees[limitZoom(zoom)];
        var ids = tree?.Range(LngX(minLng), LatY(maxLat), LngX(maxLng), LatY(minLat)) ?? new List<int>();
        var clusters = new List<IGeoJsonFeature>();
        foreach (var id in ids)
        {
            var c = tree?.points[id];
            IGeoJsonFeature item;
            if (c?.NumPoints > 0)
            {
                item = GetClusterJson(c);
                clusters.Add(item);
            }
            else if(c != null)
            {
                item = _points[c.Index];
                clusters.Add(item);
            }
            
        }

        return clusters;
    }
    
    public List<IGeoJsonFeature> GetChildren(int clusterId)
    {
        var originId = GetOriginId(clusterId);
        var originZoom = GetOriginZoom(clusterId);
        var errorMsg = "No cluster with the specified id.";
        
        var index = _trees[originZoom];
        if (index != null) throw new Exception(errorMsg);
        
        var origin = index.points[originId];
        if (origin == null) throw new Exception(errorMsg);
        
        var r = Options.Radius / (Options.Extent * Math.Pow(2, originZoom - 1));
        var ids = index.Within(origin.X, origin.Y, r);
        List<IGeoJsonFeature> children = new();

        //TODO: can be changed to Linq-Exp
        foreach (var id in ids)
        {
            var c = index.points[id];
            if (c.ParentId == clusterId) {
                children.Add(c.NumPoints > 0 ? GetClusterJson(c) : _points[c.Index]);
            }
        }
        
        if (children.Count == 0) throw new Exception(errorMsg);

        return children;
    }

    private int limitZoom(int z)
    {
        return Math.Max(Options.MinZoom, Math.Min((int)Math.Floor((double)z), Options.MaxZoom));
    }

    private List<Cluster> CreateClusters(List<Cluster> points, int zoom)
    {
        List<Cluster> clusters = new();

        var radius = Options.Radius;
        var extent = Options.Extent;
        var reduce = Options.Reduce;
        var minPoints = Options.MinPoints;
        var r = radius / (extent * Math.Pow(2, zoom));
        
        // loop through each point
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            // if we've already visited the point at this zoom level, skip it
            if (p.Zoom <= zoom) continue;
            
            p.Zoom = zoom;

            // find all nearby points
            var tree = _trees[limitZoom(zoom+1)];
            var neighborIds = tree?.Within(p.X, p.Y, r) ?? new List<int>();
            
            var numPointsOrigin = p.NumPoints > 0 ? p.NumPoints : 1;
            var numPoints = numPointsOrigin;

            // count the number of points in a potential cluster
            foreach (var neighborId in neighborIds)
            {
                var b = tree?.points[neighborId];
                // filter out neighbors that are already processed
                if (b?.Zoom > zoom) numPoints += b.NumPoints > 0 ? b.NumPoints : 1;
            }
            
            // if there were neighbors to merge, and there are enough points to form a cluster
            if (numPoints > numPointsOrigin && numPoints >= minPoints)
            {
                var wx = p.X * numPointsOrigin;
                var wy = p.Y * numPointsOrigin;

                //TODO: uncomment
                //var clusterProperties = numPointsOrigin > 1 ? this._map(p, true) : null;
                var clusterProperties = p.Properties;

                // encode both zoom and point index on which the cluster originated -- offset by total length of features
                var id = (i << 5) + (zoom + 1) + _points.Count;
                
                foreach (var neighborId in neighborIds) {
                    var b = tree?.points[neighborId];

                    if (b?.Zoom <= zoom) continue;
                    b.Zoom = zoom; // save the zoom (so it doesn't get processed twice)

                    var numPoints2 = b.NumPoints > 0 ? b.NumPoints : 1;
                    wx += b.X * numPoints2; // accumulate coordinates for calculating weighted center
                    wy += b.Y * numPoints2;

                    b.ParentId = id;

                    //TODO: uncomment when we want to test with non null reduce
                    // if (reduce) {
                    //     if (!clusterProperties) clusterProperties = this._map(p, true);
                    //     reduce(clusterProperties, this._map(b));
                    // }
                }
                
                p.ParentId = id;
                clusters.Add(CreateCluster(wx / numPoints, wy / numPoints, id, numPoints, clusterProperties));

            }
            else
            {
                clusters.Add(p);
                if (numPoints <= 1) continue;
                foreach (var b in neighborIds.Select(neighborId => tree.points[neighborId]).Where(b => b.Zoom > zoom))
                {
                    b.Zoom = zoom;
                    clusters.Add(b);
                }
            }

        }

        return clusters;
    }
    
    // get index of the point from which the cluster originated
    private int GetOriginId(int clusterId) {
        return (clusterId - _points.Count) >> 5;
    }
    
    // get zoom of the point from which the cluster originated
    private int GetOriginZoom(int clusterId) {
        return (clusterId - _points.Count) % 32;
    }

    //TODO: need modification after understanding map function in js
    private ClusterProperties? Map(Cluster point, bool clone)
    {
        if (point.NumPoints < 1)
        {
            return clone ? point.Properties.Clone() : point.Properties;
        }

        return null;
    }

    private Cluster CreatePointCluster(GeoPoint p, int id) => new(
        x: LngX(p.Longitude),
        y: LatY(p.Latitude),
        index: id
    );

    private Cluster CreateCluster(double x, double y, int id, int numPoints, ClusterProperties properties) => new(
        x: x,
        y: y,
        id: id,
        numPoints: numPoints,
        properties: properties
    );

    private ClusterProperties GetClusterProperties(Cluster cluster)
    {
        var count = cluster.NumPoints;
        var abbrev =
            count >= 10000 ? $"{Math.Round(count / 1000.0)}k" :
            count >= 1000 ? $"{Math.Round(count / 100.0) / 10.0}k" : count.ToString();

        return new ClusterProperties(
            clusterId: cluster.Id,
            pointCount: count,
            pointCountAbrv: abbrev
        );
    }

    private ClusterJson GetClusterJson(Cluster cluster)
    {
        return new ClusterJson(
            id: cluster.Id,
            properties: GetClusterProperties(cluster),
            coordinates: new GeoPoint(longitude: XLng(cluster.X), latitude: YLat(cluster.Y))
        );
    }

    // longitude/latitude to spherical mercator in [0..1] range
    private double LngX(double lng)
    {
        return lng / 360 + 0.5;
    }


    private double LatY(double lat)
    {
        var sin = Math.Sin(lat * Math.PI / 180);
        
        var y = 0.5 - 0.25 * Math.Log((1 + sin) / (1 - sin)) / Math.PI;

        return y < 0 ? 0 : y > 1 ? 1 : y;
    }

    // spherical mercator to longitude/latitude

    private double XLng(double x)
    {
        return (x - 0.5) * 360;
    }
    
    private double YLat(double y)
    {
        var y2 = (1 - y * 2) * Math.PI;
        return 360 * Math.Atan(Math.Exp(y2)) / Math.PI - 90;
    }

    private double GetX(Cluster p) => p.X;
    private double GetY(Cluster p) => p.Y;
    
    
    public static readonly Options DefaultOptions = new(
        minZoom: 0,   // min zoom to generate clusters on
        maxZoom: 16,  // max zoom level to cluster the points on
        minPoints: 2, // minimum points to form a cluster
        radius: 40,   // cluster radius in pixels
        extent: 512,  // tile extent (radius is calculated relative to it)
        nodeSize: 64, // size of the KD-tree leaf node, affects performance
        log: true,   // whether to log timing info

        // whether to generate numeric ids for input features (in vector tiles)
        generateId: false,
        // a reduce function for calculating custom cluster properties
        reduce: null, // (accumulated, props) => { accumulated.sum += props.sum; }

        // properties to use for individual points when running the reducer
        map: props => props // props => ({sum: props.my_value})
    );
}