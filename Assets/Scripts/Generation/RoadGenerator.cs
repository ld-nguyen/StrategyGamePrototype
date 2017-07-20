using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoadGeneratorParameters
{
    public int numberOfRoads;
    public int minimumDistance;
}

public class RoadGenerator {
    public static TerrainType[] grid;

    public static TerrainType[] GenerateRoads(TerrainType[] originalMap, RoadGeneratorParameters param)
    {
        grid = originalMap;
        
        for(int road = 0; road < param.numberOfRoads; road++)
        {
            Point start;
            Point end;

            do //TODO: Better selection and rules of start and end points aka. No start on Water, or Beaches
            {
                //Select Start
                start = Point.GetRandomPoint();
                //Select End
                end = Point.GetRandomPoint();
            } while (Utility.ManhattanDistance(start, end) < param.minimumDistance);
            //Find Path with A*
            List<Point> path = AStarPathSearch.FindPath(start, end);

            foreach(Point p in path)
            {
                grid[p.y * LevelGenerator.Instance.mapDimensions.width + p.x] = TerrainType.Road;
            }
        }
        return grid;
    }
}
