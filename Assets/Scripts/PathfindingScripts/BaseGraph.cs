using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// En este script va a estar todo lo que tenga que ver con que el grafo funcione a un nivel básico.
public class Node
{
    public string ID;
    public Node parent;
    public float X;
    public float Y;

    public Node(string in_Id)
    {
        ID = in_Id;
        parent = null;
        X = Random.Range(-3f, 3f);
        Y = Random.Range(1f, 2f);
    }
}

public struct Edge
{
    public Node a;
    public Node b;

    public Edge(Node in_a, Node in_b)
    {
        a = in_a;
        b = in_b;
    }
}

public class BaseGraph : MonoBehaviour
{
    public List<Edge> Edges = new List<Edge>();
    public Dictionary<Node, HashSet<Node>> Neighbors = new Dictionary<Node, HashSet<Node>>();
    public HashSet<Node> Visited = new HashSet<Node>();

    private void GrafoDePrueba()
    {
        // Creación de nodos y conexiones
        Node A = new Node("A");
        Node B = new Node("B");
        Node C = new Node("C");
        Node D = new Node("D");
        Node E = new Node("E");
        Node F = new Node("F");
        Node G = new Node("G");
        Node H = new Node("H");

        // Construcción del grafo
        AddEdge(A, B);
        AddEdge(A, E);
        AddEdge(B, C);
        AddEdge(B, D);
        AddEdge(E, F);
        AddEdge(E, G);
        AddEdge(E, H);

        // DFS con origen en H y destino en D
        Node origen = H;
        Node destino = D;

        List<Node> pathToGoal = DepthFirstSearch(origen, destino);

        if (pathToGoal != null)
        {
            print("Sí hay un camino de H a D.");

            for (int i = pathToGoal.Count - 1; i >= 0; i--)
            {
                print("El nodo: " + pathToGoal[i].ID + " fue parte del camino a la meta. Coordenadas: (" + pathToGoal[i].X + ", " + pathToGoal[i].Y + ")");
            }
        }
        else
        {
            print("No hay camino de H a D.");
        }
    }

    private void AddEdge(Node a, Node b)
    {
        Edges.Add(new Edge(a, b));

        if (!Neighbors.ContainsKey(a))
            Neighbors[a] = new HashSet<Node>();
        if (!Neighbors.ContainsKey(b))
            Neighbors[b] = new HashSet<Node>();

        Neighbors[a].Add(b);
        Neighbors[b].Add(a);
    }

    public List<Node> DepthFirstSearch(Node origin, Node target)
    {
        Stack<Node> stack = new Stack<Node>();
        Dictionary<Node, Node> parentMap = new Dictionary<Node, Node>();

        stack.Push(origin);
        parentMap[origin] = null;

        while (stack.Count > 0)
        {
            Node currentNode = stack.Pop();

            if (currentNode == target)
            {
                // Reconstruir el camino desde el nodo destino hasta el origen
                List<Node> path = new List<Node>();
                while (currentNode != null)
                {
                    path.Add(currentNode);
                    currentNode = parentMap[currentNode];
                }
                path.Reverse();
                return path;
            }

            foreach (Node neighborNode in Neighbors[currentNode])
            {
                if (!Visited.Contains(neighborNode))
                {
                    stack.Push(neighborNode);
                    parentMap[neighborNode] = currentNode;
                    Visited.Add(neighborNode);
                }
            }
        }

        return null; // No se encontró un camino
    }

    // Start is called before the first frame update
    void Start()
    {
        GrafoDePrueba();
    }

    // Update is called once per frame
    void Update()
    {

    }
}