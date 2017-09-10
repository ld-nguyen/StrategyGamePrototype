using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/* 
* 
* A* Pathfinding script based on the following Materials:
* 
* Pseudocode from Sebastian Lague's series on the explanation of the A* algorithm
* https://youtu.be/-L-WgKMFuhE?t=8m9s
* 
* Amit Patel's blogs about pathfinding and the A* algorithm
* http://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html (20.07.2017)
* http://www.redblobgames.com/pathfinding/a-star/introduction.html (20.07.2017)
* 
* TODO: Factor in perlin heights in heuristics for Road Generation
*/

public class Node : System.IComparable
{
    public Point coords { get; private set; }
    public float g;
    public float h;
    public float f { get { return g + h + externalCost; } }
    public float externalCost;
    public Node parent;

    public Node(Point p, float weight = 0)
    {
        coords = p;
        g = 0;
        h = 0;
        externalCost = weight;
    }

    public Node(Point p, int gCost, int hCost, float weight = 0)
    {
        coords = p;
        g = gCost;
        h = hCost;
        externalCost = weight;
    }

    public Node(Point p, Point start, Point goal, float weight = 0)
    {
        coords = p;
        g = Utility.ManhattanDistance(start, p);
        h = Utility.ManhattanDistance(p, goal);
        externalCost = weight;
    }

    public int CompareTo(object obj)
    {
        Node nodeObject = (Node)obj;
        return f.CompareTo(nodeObject.f);
    }

    public override bool Equals(object o)
    {
        Node nodeObj = (Node)o;
        return coords.Equals(nodeObj.coords);
    }

    public bool IsSamePoint(Point p)
    {
        return coords.Equals(p);
    }

    public void CalculateCosts(Point start, Point goal, float nodeWeight)
    {
        g = Utility.ManhattanDistance(start, coords);
        h = Utility.ManhattanDistance(coords, goal);
        externalCost = nodeWeight;
    }
}

public class AStarPathSearch
{
    public delegate int ExternalCostFactor(Point currentNode, Point neighbourNode);
    public delegate bool TraversableCondition(Point p);
    public delegate bool EarlyExitCondition(Point p);
    private static Point startPoint;
    private static Point goalPoint;

    public static List<Point> FindPath(Point start,Point goal, TraversableCondition isTraversableCondition, ExternalCostFactor externalCostOffset = null, EarlyExitCondition earlyExit = null)
    {
        //Caching Parameters for the other methods
        startPoint = start;
        goalPoint = goal;
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        Node startNode = GetNodeFromPoint(start);
        openList.Add(startNode);

        while(openList.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openList);

            if(CheckForGoal(currentNode,earlyExit)) //Early Exit when found
            {
                //Reconstruct path
                return RetracePath(startNode,currentNode);
            }
                openList.Remove(currentNode);
                closedList.Add(currentNode);
                List<Node> neighbours = GetNeighbours(currentNode);

                foreach(Node neighbour in neighbours)
                {
                    if (closedList.Contains(neighbour) || !isTraversableCondition(neighbour.coords))
                    {
                        continue;
                    }

                float newCost = currentNode.g + Utility.ManhattanDistance(currentNode.coords,neighbour.coords);
                if (externalCostOffset != null)
                {
                    newCost += externalCostOffset(currentNode.coords,neighbour.coords);
                }

                    if(newCost < neighbour.g || !openList.Contains(neighbour))
                    {
                        neighbour.g = newCost;
                        neighbour.h = Utility.ManhattanDistance(neighbour.coords, goal);
                        neighbour.parent = currentNode;

                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
        }
        Debug.Log("No Road Found");
        return new List<Point>();
    }

    private static bool CheckForGoal(Node currentNode, EarlyExitCondition earlyExit)
    {
        if (earlyExit != null)
        {
            if (earlyExit(currentNode.coords) && Utility.ManhattanDistance(startPoint,currentNode.coords) > 10) Debug.Log("EarlyExit!");
            return (earlyExit(currentNode.coords) && Utility.ManhattanDistance(startPoint, currentNode.coords) > 10) || currentNode.coords.Equals(goalPoint);
        }
        else
        {
            return currentNode.coords.Equals(goalPoint);
        }
    }

    private static Node GetNodeFromPoint(Point p)
    {
        return new Node(p);
    }

    private static Node GetLowestFCostNode(List<Node> list)
    {
        list.Sort();
        Node lowestFCostNode = list[0];
        return lowestFCostNode;
    }

    //Gets all neighbours around a node
    private static List<Node> GetNeighboursAround(Node currentNode)
    {
        const int kernelSize = 1;
        List<Node> neighbours = new List<Node>();

        for(int xOffset = -kernelSize; xOffset <= kernelSize; xOffset++)
        {
            for (int yOffset = -kernelSize; yOffset <= kernelSize; yOffset++)
            {
                if (xOffset == 0 && yOffset == 0) continue;
                else
                {
                    Point neighbour = currentNode.coords.GetNeighbour(xOffset, yOffset);
                    if (Utility.IsInsideGrid(neighbour)) 
                    {
                        neighbours.Add(new Node(neighbour, startPoint, goalPoint));
                    }
                }
            }
        }
        return neighbours;
    }

    //Gets neighbours around a node, but ignores diagonal ones
    private static List<Node> GetNeighbours(Node currentNode)
    {
        List<Node> neighbours = new List<Node>();
        Point currentCoords = currentNode.coords;

        Point[] offsets = {
            currentCoords.GetNeighbour(1,0),
            currentCoords.GetNeighbour(-1,0),
            currentCoords.GetNeighbour(0,1),
            currentCoords.GetNeighbour(0,-1)
        };

        for(int i = 0; i < offsets.Length; i++)
        {
            if (Utility.IsInsideGrid(offsets[i]))
            {
                neighbours.Add(new Node(offsets[i], startPoint, goalPoint));
            }
        }

        return neighbours;
    }

    private static List<Point> RetracePath(Node start, Node end)
    {
        Debug.Log("Retracing");
        List<Point> path = new List<Point>();
        Node currentNode = end;
        while (currentNode.parent != null)
        {
            path.Add(currentNode.coords);
            currentNode = currentNode.parent;
        }
        Debug.Log("Returning Path of Length " + path.Count);
        return path; 
    }
}
