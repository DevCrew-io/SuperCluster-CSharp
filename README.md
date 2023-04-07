# SuperCluster-CSharp
[![license](https://img.shields.io/badge/license-MIT-green)](https://github.com/DevCrew-io/SuperCluster-CSharp/blob/main/LICENSE)
[![](https://img.shields.io/badge/MapBox-informational?style=flat&logo=Mapbox&color=000000)](https://www.mapbox.com/)
![](https://img.shields.io/badge/NuGet-informational?style=flat&logo=NuGet&color=004880)
![](https://img.shields.io/badge/Unity-webGL-informational?style=flat&logo=Unity&color=990000)
![](https://img.shields.io/badge/CSharp-.NetCore-informational?style=flat&logo=CSharp&color=512BD4)

A C# library that provides an efficient way to cluster large point datasets. This library is based on the popular SuperCluster JavaScript library by [Mapbox](https://www.mapbox.com/) and has been optimized for performance in .NET environments.
It helps to implement clustering feature in Unity3d WebGL and in any .NET framework.

## Installation
### Install using NuGet
We'll soon publish our library to [**NuGet**](https://www.nuget.org/packages) Packages Gallery
and then you can search for DevCrew.SuperCluster or SuperCluster on NuGet package manager in Visual Studio and add it to your project.
### Install as open source
The second option is to add SuperCluster as an open source module to your .NET or Unity project if you don't use NuGet.
* Clone the repo `https://github.com/DevCrew-io/SuperCluster-CSharp.git`
* Make sure to checkout `main` branch
* Copy the `DevCrew.SuperCluster` folder and remove `DevCrew.SuperCluster.csproj` file
* Now Paste the whole DevCrew.SuperCluster package into your project and you are ready to use

## Usage 

### GeoPoint 
Map your data set into list of `GeoPoint`
```c#
private readonly List<GeoPoint> _points = new List<GeoPoint>();

//here we have added some dummy geo coordinates 
_points.Add(new GeoPoint(longitude: -112.8384399, latitude: 49.696595));
_points.Add(new GeoPoint(longitude: -112.838563, latitude: 49.694496));
_points.Add(new GeoPoint(longitude: -112.8412346, latitude: 49.696189));
```

### Options
Create a new object of `SuperCluster` with clustering options
```c#
//superCluster with default clustering options
private readonly SuperCluster _superCluster = new(options: SuperCluster.DefaultOptions);

//or you can provide your own clustering options like this:
Options customOptions = new(
    minZoom: 0,
    maxZoom: 16,
    minPoints: 2,
    radius: 30,
    extent: 512,
    nodeSize: 64,
    log: true,
    generateId: false
);
```
| Option     | Default | Description                                                      |
|------------|---------|------------------------------------------------------------------|
| minZoom    | 0       | Minimum zoom level at which clusters are generated               |
| maxZoom    | 16      | Maximum zoom level at which clusters are generated               |
| minPoints  | 2       | Minimum number of points to form a cluster                       |
| radius     | 40      | Cluster radius, in pixels                                        |
| extent     | 512     | (Tiles) Tile extent. Radius is calculated relative to this value |
| generateId | false   | Whether to generate ids for input features in vector tiles       |
| nodeSize   | 64      | Size of the KD-tree leaf node. Affects performance               |
| log        | false   | Whether timing info should be logged                             |

### Methods

#### **`Load(points)`** 
Once we have create a super cluster object then we can cluster our points using `Load()` method. It creates all possible clusters for different zoom levels at once.
```c#
//This method should be called only once 
_superCluster.Load(points: _points);
```

#### **`GetClusters(bbox, zoom)`** 
For the given bbox array `[westLng, southLat, eastLng, northLat]` and integer `zoom`, returns an array of clusters and points.
```c#
var bbox = new double[] { -180, -85, 180, 85 };
//This method should be called on zoom in and zoom out events.
var clusterData = _superCluster.GetClusters(bbox: bbox, zoom: 16);

Console.WriteLine("clusterData.count: " + clusterData.Count);

//print clustered data
foreach (var output in clusterData.Select(JsonConvert.SerializeObject))
{
    Console.WriteLine(output);
}   
```
```c#

//you can filter between clusters and individual points from clusterData output like this
foreach (var geoJsonFeature in clusterData)
{
    if (geoJsonFeature is GeoPoint)
    {
        //This is single geo point
    }
    else
    {
        //This is a cluster
    }
}
```

## Authors
[**DevCrew.IO**](https://devcrew.io/)

### Connect with Us
[![](https://img.shields.io/badge/Website-DevCrew.IO-informational?style=flat&color=710F15)](https://devcrew.io)
[![](https://img.shields.io/badge/DevCrew.IO-informational?style=flat&logo=linkedin&color=0A66C2)](https://www.linkedin.com/company/devcrew-io)
[![](https://img.shields.io/badge/DevCrew.IO-informational?style=flat&logo=github&color=181717)](https://github.com/DevCrew-io)

## Contributing
Contributions, issues, and feature requests are welcome!

## Show your Support
Give a star if this project helped you.

## Copyright & License
Code copyright 2023–2024 [DevCrew I/O](https://devcrew.io/).
Code released under the [MIT license](https://github.com/DevCrew-io/SuperCluster-CSharp/blob/main/LICENSE).
