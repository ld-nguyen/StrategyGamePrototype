using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct RoadGeneratorParameters
{
    public int numberOfRoads;
    public int minimumDistance;
    public List<TerrainType> startTerrain;
    public List<TerrainType> endTerrain;
    public List<TerrainType> roadTerrains;
    public TerrainType desiredRoadTerrain;
    public RoadGenerator.ExternalCostFactorMethod externalCostFactorMethod;
    public RoadGenerator.EarlyExitCondition earlyExit;
 }

public class RoadGenerator {
    public enum ExternalCostFactorMethod{ DownwardsTerrain, EvenTerrain}
    public enum EarlyExitCondition { GoalTerrain,None }
    private static TerrainType[] grid;
    private static RoadGeneratorParameters parameters;

    private const int MAX_ATTEMPTS = 1000;

    public static TerrainType[] GenerateRoads(TerrainType[] originalMap, RoadGeneratorParameters param)
    {
        grid = originalMap;
        parameters = param;
        int attemptCounter = 0;
        for(int road = 0; road < param.numberOfRoads; road++)
        {
            Point startPoint;
            Point endPoint;
            do
            {
                //Select Start
                do
                {
                    attemptCounter++;
                    startPoint = Point.GetRandomPoint();
                } while (!CheckStart(startPoint) && attemptCounter < MAX_ATTEMPTS);
                if (attemptCounter >= MAX_ATTEMPTS) { Debug.LogWarning("RoadGen: No suitable start found!"); return originalMap; }
                else attemptCounter = 0;
                //Select End
                do
                {
                    attemptCounter++;
                    endPoint = Point.GetRandomPoint();
                } while (!CheckEnd(endPoint) && attemptCounter < MAX_ATTEMPTS);
                if (attemptCounter >= MAX_ATTEMPTS) { Debug.LogWarning("RoadGen: No suitable end found!"); return originalMap; }
            } while (Utility.ManhattanDistance(startPoint, endPoint) < param.minimumDistance);
            Debug.Log("Start " + startPoint.ToString() + " End " + endPoint.ToString()+" MHD: "+Utility.ManhattanDistance(startPoint,endPoint));
                //Find Path with A*
                AStarPathSearch.ExternalCostFactor CostFactor = GetExternalCostMethod(param.externalCostFactorMethod);
            AStarPathSearch.EarlyExitCondition EarlyExit = GetEarlyExitCondition(param.earlyExit);
                List<Point> path = new List<Point>();
                path = AStarPathSearch.FindPath(startPoint, endPoint, IsTraversableTerrain, CostFactor, EarlyExit);
                //Set desired terraintype of path on grid
                if (path.Count > 0)
                {
                    Debug.Log("Path found!");
                    foreach(Point p in path)
                    {
                        grid[p.gridIndex] = param.desiredRoadTerrain;
                    }
                }
        }
        return grid;
    }

    public static bool CheckStart(Point p)
    {
        return parameters.startTerrain.Contains(GetTerrainAtPoint(p)) && PointIsAtEdge(p);
    }

    public static bool CheckEnd(Point p)
    {
        return parameters.endTerrain.Contains(GetTerrainAtPoint(p)) && PointIsAtEdge(p);
    }

    public static bool PointIsAtEdge(Point p)
    {
        //This method checks the immediate neighbourhood of a point p and checks if there are at least 3 different points with a different terraintype. If there are at least 3 points then the inital Point is on an edge / corner
            int counter = 0;
            for (int xOffset = -1; xOffset <= 1; xOffset++)
            {
                for (int yOffset = -1; yOffset <= 1; yOffset++)
                {
                    Point offset = p + new Point(xOffset, yOffset);
                    if (offset.IsInsideGrid() && IsTraversableTerrain(offset) && IsDifferentTerrain(p, offset))
                    {
                        counter++;
                    }
                }
            }
            if (counter >= 3) return true;
            else return false;
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
        return grid[p.gridIndex];
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

    private static AStarPathSearch.EarlyExitCondition GetEarlyExitCondition(EarlyExitCondition condtion)
    {
        if (condtion == EarlyExitCondition.GoalTerrain)
        {
            return GoalTerrainEarlyExit;
        }
        else
        {
            return null;
        }
    }

    //Quit A* as soon you land on the goal terrain
    private static bool GoalTerrainEarlyExit(Point coord)
    {
        return parameters.endTerrain.Contains(GetTerrainAtPoint(coord)); 
    }

    private static AStarPathSearch.ExternalCostFactor GetExternalCostMethod(ExternalCostFactorMethod method)
    {
        switch (method)
        {
            case ExternalCostFactorMethod.DownwardsTerrain:
                return DownwardsSlopeCost;
            case ExternalCostFactorMethod.EvenTerrain:
                return EvenSlopeCost;
            default:
                return null;
        }
    }

    //Gives a penalty to the A* cost if end is higher in elvation than start
    private static int DownwardsSlopeCost(Point start,Point end)
    {
        //Assigning penalty values to negative elevation differences. If the end point i.e. 0.6f is higher than the start point 0.2 then the difference will always be negative i.e 0.2 - 0.6 = -0.4
        //Multiplying by -1000 to give this cost offset a meaningful impact in the A* heuristics
        float difference = Utility.GetElevationDifference(start, end) < 0 ? Utility.GetElevationDifference(start, end) * -1000 : 0;
        return Mathf.RoundToInt(difference);
    }

    //Gives Higher cost penalty for A* to differences in Elevation
    private static int EvenSlopeCost(Point start, Point end)
    {
        float difference = Utility.GetElevationDifference(start, end) * 1000;
        //Using the formula x² since the penalty at high elvation differences should be high and positive
        return Mathf.RoundToInt(Mathf.Pow(difference,2));
    }


}
