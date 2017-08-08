using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoadGeneratorParameters
{
    public int numberOfRoads;
    public int minimumDistance;
    public List<TerrainType> startAndEndTerrain;
    public List<TerrainType> roadTerrains;
    public TerrainType desiredRoadTerrain;
}

public class RoadGenerator {
    private static TerrainType[] grid;
    private static RoadGeneratorParameters parameters;

    public static TerrainType[] GenerateRoads(TerrainType[] originalMap, RoadGeneratorParameters param)
    {
        grid = originalMap;
        parameters = param;
        for(int road = 0; road < param.numberOfRoads; road++)
        {
            Point start;
            Point end;

            do
            {
                //Select Start
                do
                {
                    start = Point.GetRandomPoint();
                } while (!CheckPoints(start));
                //Select End
                do
                {
                    end = Point.GetRandomPoint();
                } while (!CheckPoints(end));
            } while (Utility.ManhattanDistance(start, end) < param.minimumDistance);

            Debug.Log("Start:" + start.ToString() + " End: " + end.ToString());

            //Find Path with A*
            List<Point> path = new List<Point>();
            path = AStarPathSearch.FindPath(start, end, IsTraversableTerrain);
            //Draw Path on grid
            if (path.Count > 0)
            {
                Debug.Log("Path found!");
                foreach(Point p in path)
                {
                    grid[p.y * LevelGenerator.Instance.mapDimensions.width + p.x] = param.desiredRoadTerrain;
                }
            }
            else { Debug.Log("Path not found!"); }
        }
        return grid;
    }

    public static bool CheckPoints(Point p)
    {
        if (!parameters.startAndEndTerrain.Contains(GetTerrainAtPoint(p))) return false;
        else
        {
            return Utility.CheckNeighbours(p, 1, IsDifferentTerrain);
        }
    }

    public static bool IsSameTerrain(Point initial,Point p)
    {
        return GetTerrainAtPoint(initial) == GetTerrainAtPoint(p);
    }

    public static bool IsDifferentTerrain(Point initial, Point p)
    {
        return GetTerrainAtPoint(initial) != GetTerrainAtPoint(p);
    }

    public static TerrainType GetTerrainAtPoint(Point p)
    {
        return grid[p.y * LevelGenerator.Instance.mapDimensions.width + p.x];
    }

    private static bool IsTraversableTerrain(Point p)
    {
        if (parameters.roadTerrains.Count <= 0)
        {
            return true;
        }
        else
        {
            if (parameters.roadTerrains.Contains(LevelGenerator.Instance.TerrainAtPoint(p)))
            {
                return true;
            }
            else return false;
        }
    }
}
