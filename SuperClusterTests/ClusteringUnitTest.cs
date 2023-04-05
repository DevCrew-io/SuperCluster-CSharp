using DevCrew.SuperCluster;
using Newtonsoft.Json;

namespace SuperClusterTests;

[TestClass]
public class ClusteringUnitTest
{
    private readonly List<GeoPoint> _points = new List<GeoPoint>
    {
        new(-112.8394738, 49.6950781),
            
        new(-112.8384399, 49.696595),
        new(-112.8412346, 49.694496),
        new(-112.805587,49.666781),
        new(-112.7887407,49.6940248),
        new(-112.837379,49.695007),
        new(-112.8394807,49.695062),
        new(-112.8331197,49.6952073),
        new(-112.8340887,49.694089),
        new(-112.8426617,49.691265),
        new(-112.838563,49.696189),
        new(-112.861613,49.6776939),
        new(-112.8431332,49.6976012),
        new(-112.8588769,49.686772),
        new(-112.834161,49.6939781)
    };
    private readonly SuperCluster _superCluster = new(options: SuperCluster.DefaultOptions);

    [TestMethod]
    public void TestClusteringAtMinZoom()
    {
        _superCluster.Load(points: _points);
        var ar = new double[] { -180, -85, 180, 85 };
        var clusterData = _superCluster.GetClusters(bbox: ar, 4);
        Console.WriteLine("clusterData.count: " + clusterData.Count);
        foreach (var output in clusterData.Select(JsonConvert.SerializeObject))
        {
            Console.WriteLine(output);
        }
        Assert.IsTrue(clusterData.Count == 1);
    }
    
    [TestMethod]
    public void TestClusteringAtMaxZoom()
    {
        _superCluster.Load(points: _points);
        var ar = new double[] { -180, -85, 180, 85 };
        var clusterData = _superCluster.GetClusters(bbox: ar, 14);
        Console.WriteLine("clusterData.count: " + clusterData.Count);
        foreach (var output in clusterData.Select(JsonConvert.SerializeObject))
        {
            Console.WriteLine(output);
        }
        Assert.IsTrue(clusterData.Count >= 12);
    }
}