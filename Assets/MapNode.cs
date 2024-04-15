using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour  {
    public List<MapNode> NextNodes { get; private set; }
    public int Round { get; private set; }
    public int Road { get; private set; }

    public void Init(int round, int road) {
        NextNodes = new List<MapNode>();
        Round = round;
        Road = road;
        gameObject.name = $"MapNode({round}-{road})";
    }
    
    public void AddConnection(MapNode nextNode) {
        NextNodes.Add(nextNode);
    }
}
