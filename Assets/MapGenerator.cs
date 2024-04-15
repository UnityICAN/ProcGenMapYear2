using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour {
    [SerializeField] private Transform nodesParent;
    [SerializeField] private Transform mapStartHighTransform;
    [SerializeField] private Transform mapStartMidTransform;
    [SerializeField] private Transform mapStartLowTransform;
    [SerializeField] private Transform mapEndTransform;
    [SerializeField] private GameObject mapNode;
    [SerializeField] private GameObject mapConnector;
    [SerializeField] private float additionalConnectorProbability = 0.33f;

    private const int NUMBER_OF_ROUNDS = 10;
    private const int NUMBER_OF_ROADS = 3;

    private Vector2 mapBottomLeftPosition;
    private Vector2 mapTopRightPosition;
    private float averageHorizontalSpaceBetweenNodes;

    private List<List<MapNode>> nodesByRound;
    private List<GameObject> connectors;
    
    private void Start() {
        // Store map bounds
        mapBottomLeftPosition = mapStartLowTransform.position;
        mapTopRightPosition = new Vector2(mapEndTransform.position.x, mapStartHighTransform.position.y);
        
        averageHorizontalSpaceBetweenNodes = (mapTopRightPosition.x - mapBottomLeftPosition.x) / (NUMBER_OF_ROUNDS - 1);
        
        GenerateMap();
    }

    private void Update() {
        if (Input.GetButtonDown("Jump")) {
            // Replace current map with new map
            foreach (Transform children in nodesParent) {
                Destroy(children.gameObject);
            }
            nodesByRound.Clear();
            connectors.Clear();
            GenerateMap();
        }
    }

    private void GenerateMap() {
        GenerateNodes();
        GenerateConnectors();
    }

    private void GenerateNodes() {
        nodesByRound = new List<List<MapNode>>();
        for (int round = 0; round < NUMBER_OF_ROUNDS; round++) {
            // Initialize temp list for current round
            List<MapNode> roundNodes = new List<MapNode>();
            
            for (int road = 0; road < NUMBER_OF_ROADS; road++) {
                // Find base position for node
                Vector2 basePosition = new Vector2(
                    GetRoundHorizontalPosition(round),
                    GetRoadVerticalPosition(road));
                
                // Add some randomness around base position
                Vector2 actualPosition =
                    basePosition + Random.insideUnitCircle * averageHorizontalSpaceBetweenNodes / 4f;
                
                // Instantiate node on position
                GameObject instantiatedNodeObject = Instantiate(mapNode, actualPosition, Quaternion.identity, nodesParent);
                
                // Retrieve Map Node component
                MapNode instantiatedNode = instantiatedNodeObject.GetComponent<MapNode>();
                instantiatedNode.Init(round, road);
                
                // Add Map Node to temp list
                roundNodes.Add(instantiatedNode);
            }
            
            // Add nodes triplet to main list
            nodesByRound.Add(roundNodes);
        }
    }

    private void GenerateConnectors() {
        connectors = new List<GameObject>();
        // Generate main connections
        for (int road = 0; road < NUMBER_OF_ROADS; road++) {
            for (int round = 0; round < NUMBER_OF_ROUNDS - 1; round++) {
                MapNode startNode = nodesByRound[round][road];
                MapNode endNode = nodesByRound[round + 1][road];
                GenerateConnector(startNode, endNode);
            }
        }
        
        // Generate transversal connections
        List<int> rounds = Enumerable.Range(0, NUMBER_OF_ROUNDS - 1).ToList();
        rounds.Shuffle();
        
        foreach (int round in rounds) {
            List<int> roads = Enumerable.Range(0, NUMBER_OF_ROADS).ToList();
            roads.Shuffle();
            
            foreach (int road in roads) {
                if (road > 0) {
                    if (!nodesByRound[round][road - 1].NextNodes.Contains(nodesByRound[round + 1][road])
                        && Random.Range(0f, 1f / additionalConnectorProbability) <= 1f) {
                        MapNode startNode = nodesByRound[round][road];
                        MapNode endNode = nodesByRound[round + 1][road - 1];
                        GenerateConnector(startNode, endNode);
                    }
                }
                
                if (road < NUMBER_OF_ROADS - 1) {
                    if (!nodesByRound[round][road + 1].NextNodes.Contains(nodesByRound[round + 1][road])
                        && Random.Range(0f, 1f / additionalConnectorProbability) <= 1f) {
                        MapNode startNode = nodesByRound[round][road];
                        MapNode endNode = nodesByRound[round + 1][road + 1];
                        GenerateConnector(startNode, endNode);
                    }
                }
            }
        }
    }

    private void GenerateConnector(MapNode startNode, MapNode endNode) {
        startNode.AddConnection(endNode);
                
        // Instantiate connector
        GameObject connector = Instantiate(mapConnector,
            startNode.transform.position,
            Quaternion.identity,
            nodesParent);
                
        // Set connector line vertices
        connector.GetComponent<LineRenderer>().SetPositions(new Vector3[] {
            startNode.transform.position,
            endNode.transform.position
        });
        connector.name = $"Connector({startNode.Round}-{startNode.Road})-({endNode.Round}-{endNode.Road})";
    }

    private float GetRoundHorizontalPosition(int round) {
        return mapBottomLeftPosition.x + averageHorizontalSpaceBetweenNodes * round;
    }

    private float GetRoadVerticalPosition(int road) {
        return road switch {
            0 => mapStartHighTransform.position.y,
            1 => mapStartMidTransform.position.y,
            2 => mapStartLowTransform.position.y,
            _ => mapStartMidTransform.position.y
        };
    }
}
