using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// En este script va a estar todo lo que tenga que ver con que el grafo funcione a un nivel básico.
public class Node
{

    public string ID;
    // Una que no me sabía yo de Unity C#: Las structs no pueden tener objetos de su propia struct, 
    // pero las classes sí. Probablemente se debe a que las Classes son siempre referencias (punteros) en C#.
    public Node parent;

    //Agrego las coordenadas X y Y
    public float X;
    public float Y;


    public Node(string in_Id)
    {
        ID = in_Id;
        parent = null;
        //Les establecemos un rango aleeatorio entre 0 y 10
        X = Random.Range(0, 10);
        Y = Random.Range(0, 10);
    }



    //== es el operador de igualdad.
    //public static bool operator ==(Node lhs, Node rhs)
    //{
    //    if (rhs.ID == lhs.ID)
    //        return true;
    //    return false;
    //}

    //// != es el operador de inigualdad.
    //public static bool operator !=(Node lhs, Node rhs)
    //{
    //    if (rhs.ID != lhs.ID)
    //        return true;
    //    return false;
    //}

    // !
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

// Templates o plantillas.


public enum NodeState
{
    Unknown = 0,
    Open,
    Closed,
    MAX
}

public class BaseGraph : MonoBehaviour
{
    // MyStruct 
    // Esta clase es la que administra nuestros nodos y aristas.
    // List<Node> Nodes = new List<Node>();
    public List<Edge> Edges = new List<Edge>();
    public List<Node> Nodes = new List<Node>();

    // Ponemos el prefab del objeto que servira de visualizador para los nodos
    public GameObject nodePrefab;
    // La lista abierta nos permite guardar a cuáles nodos ya hemos llegado pero no hemos terminado de visitar a sus vecinos
    public Dictionary<Node, NodeState> NodeStateDict = new Dictionary<Node, NodeState>();
    // La lista cerrada nos permite guardar a los nodos que ya terminamos de explor
    // es decir, en cuáles ya no hay nada más que hacer.


    // 1) Necesitamos que sí respete el orden, específicamente que el último elemento en añadirse sea el primero en salir
    // 2) Tenemos que poder checar si el nodo a meter YA está en la lista abierta.
    // 3) Que agregar y quitar elementos de la estructura de datos sea rápido.
    public Stack<Node> OpenList = new Stack<Node>();
    // public List<Node> ClosedList = new List<Node>();
    // public Stack<Node> ClosedList = new Stack<Node>();

    // Las propiedades que queremos de la estructura de datos para nuestra lista cerrada son:
    // 1) No necesitamos que respete el orden en que se añadieron los nodos
    // 2) Tiene que agregar nodos lo más rápido posible
    // 3) Tiene que poder checar si contiene o no a un nodo dado rápidamente.
    public HashSet<Node> ClosedSetList = new HashSet<Node>();

    private void GrafoDePrueba()
    {

        // Me faltaba ponerle el "public" al constructor!
        // Ponemos todos los nodos de nuestro diagrama.
        Node A = new Node("A");
        Node B = new Node("B");
        Node C = new Node("C");
        Node D = new Node("D");
        Node E = new Node("E");
        Node F = new Node("F");
        Node G = new Node("G");
        Node H = new Node("H");

        Nodes.Add(A);
        Nodes.Add(B);
        Nodes.Add(C);
        Nodes.Add(D);
        Nodes.Add(E);
        Nodes.Add(F);
        Nodes.Add(G);
        Nodes.Add(H);

        // Por defecto, nuestro Diccionario (que tiene tanto la lista de abiertos como de cerrados)
        // Va a tener a todos nuestros nodos.
        NodeStateDict.Add(A, NodeState.Unknown);
        NodeStateDict.Add(B, NodeState.Unknown);
        NodeStateDict.Add(C, NodeState.Unknown);
        NodeStateDict.Add(D, NodeState.Unknown);
        NodeStateDict.Add(E, NodeState.Unknown);
        NodeStateDict.Add(F, NodeState.Unknown);
        NodeStateDict.Add(G, NodeState.Unknown);
        NodeStateDict.Add(H, NodeState.Unknown);


        // Ahora ponemos los Edges/Aristas, es decir, conexiones entre nodos.
        Edge AB = new Edge(A, B);
        Edge AE = new Edge(A, E);

        Edge BC = new Edge(B, C);
        Edge BD = new Edge(B, D);

        Edge EF = new Edge(E, F);
        Edge EG = new Edge(E, G);
        Edge EH = new Edge(E, H);

        // Metemos nuestras aristas en la lista de Aristas.
        Edges.Add(EH);
        Edges.Add(EG);
        Edges.Add(EF);
        Edges.Add(BD);
        Edges.Add(BC);
        Edges.Add(AE);
        Edges.Add(AB);

        // Ahora que ya tenemos nuestro grafo, nos falta aplicar algún algoritmo sobre él.
        // Por ejemplo, Búsqueda en profundidad (Depth-First Search).

        // Mi prueba será que inicie en H y me diga si puede llegar a D.
        NodeStateDict[H] = NodeState.Open;
        bool pathExists = ItDepthFirstSearch(H, D);
        if (pathExists)
        {
            print("Sí hay un camino de H a D. (DepthFirstSearch)");
            List<Node> pathToGoal = new List<Node>();
            Node currentNode = D;
            while (currentNode != null)
            {
                pathToGoal.Add(currentNode);
                currentNode = currentNode.parent;
            }
            foreach (Node node in pathToGoal)
            {
                print("El nodo: " + node.ID + " fue parte del camino a la meta (DepthFirstSearch)");
            }
        }
        else
            print("No hay camino de H a D. (DepthFirstSearch");

        //----------------------------------------------------------------------------------------------------
        NodeStateDict[H] = NodeState.Open;
        bool pathExistsBFS = ItBreadthFirstSearch(H, D);
        if (pathExistsBFS)
        {
            print("Sí hay un camino de H a D (BreadthFirstSearch).");
        }
        else
        {
            print("No hay camino de H a D (BreadthFirstSerch).");
        }

        //Por cada nodo en los nodos
        foreach (Node node in Nodes)
        {
            // Instanciar el prefab de nodo
            GameObject nodeObject = Instantiate(nodePrefab, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);

            // Llamamos al script de NodeVisualizer y encontramos a todos los objetos con ese componente
            NodeVisualizer visualizer = nodeObject.GetComponent<NodeVisualizer>();
            if (visualizer != null)
            {
                //Ponemos la funcion para ponerles una ubicacion en x y Y
                visualizer.SetPosition(node.X, node.Y);
            }
            else
            {
                Debug.Log("No se encontró el componente NodeVisualizer en el prefab de nodo.");
            }
        }
    }

    public bool ItDepthFirstSearch(Node Origin, Node Target)
    {
        // empezamos en el nodo Origen.
        // Ponemos al nodo origen en la lista abierta.
        OpenList.Push(Origin);

        Node currentNode = Origin;

        // Otra posibilidad es while(currentNode != null),
        while (OpenList.Count != 0)  // Mientras haya otros nodos por visitar, sigue ejecutando el algoritmo.
        {
            // ya que sabemos cual es el nodo actual, podemos empezar a meter a sus vecinos a la lista abierta.

            // Necesitamos quitar elementos de la lista abierta en algún punto de este ciclo while.
            // El truco está en saber dónde.
            // Puede que en Breadth first search no sea igual la ubicación!

            // Tenemos que tener una forma de saber quiénes son los vecinos del nodo actual.
            // Hay que ver cuál de las aristas está conectada con nuestro nodo CurrentNode.
            // Lo hicimos a través del método FindNeighbors.
            List<Edge> currentNeighbors = FindNeighbors(currentNode);
            // Si esta bandera queda true al terminar el foreach de las aristas, mete a currentNode a la lista cerrada.
            bool sendToClosedList = true;
            // Visita a cada uno de ellos, hasta que se acaben o hasta que encontremos el objetivo.
            foreach (Edge e in currentNeighbors)
            {
                // Checamos cuál de los dos nodos que esta arista conecta no es el CurrentNode.
                Node NonCurrentNode = currentNode != e.a ? e.a : e.b;
                // Primero checamos si ya está en la lista abierta, y si lo está, no mandamos a llamar el algoritmo.
                // también tenemos que checar que no esté en la lista cerrada!
                if (OpenList.Contains(NonCurrentNode) || ClosedSetList.Contains(NonCurrentNode))
                    continue;
                if (NonCurrentNode == Target)
                {
                    // Entonces ya tenemos una ruta de origin a target.
                    // nada más le ponemos que target.parent es igual a currentNode y listo, podemos salir de la función.
                    NonCurrentNode.parent = currentNode;
                    return true;
                }
                else
                {
                    // Si no, lo agregamos a lista abierta.
                    OpenList.Push(NonCurrentNode);
                    // Cuando tú (Nodo Current) metes a otro nodo a la lista abierta, pones a currentNode como su parent node.
                    NonCurrentNode.parent = currentNode;
                    // En esta versión iterativa, cuando currentNode mete a alguien más a la lista abierta, 
                    // ese nuevo nodo se convierte en currentNode, y vuelves a empezar el ciclo.
                    sendToClosedList = false;
                    currentNode = NonCurrentNode;
                    break; // Esto hace que el ciclo vaya a la siguiente iteración, sin llegar al código debajo de este continue.
                }
            }

            // Cuando el currentNode no mete a nadie a la lista abierta, quiere decir que ya visitó a todos sus vecinos
            // y por lo tanto, él se sale de la lista abierta, y se mete a la lista cerrada.
            if (sendToClosedList)
            {
                Node poppedNode = OpenList.Pop();
                ClosedSetList.Add(poppedNode);
                currentNode = OpenList.Peek(); //Peek es "dame el elemento de hasta arriba pero sin sacarlo de la pila".
            }
            // el else ya no es necesario, porque ya nos encargamos justo antes del "break;" del foreach.

        }

        // no hay camino de origin a target.
        return false;
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------
    public bool ItBreadthFirstSearch(Node Origin, Node Target)
    {
        // Usamos una cola o lista por decirlo de alguna manera para BreadthFirstSearch
        Queue<Node> queue = new Queue<Node>();
        // Empezamos la lista desde el nodo origen
        queue.Enqueue(Origin);

        //Mientras la lista tenga un contador mayor a 0...
        while (queue.Count > 0)
        {
            // Sacamos un nodo de la cola
            Node currentNode = queue.Dequeue();

            // Si llegamos al nodo objetivo, encontramos un camino
            if (currentNode == Target)
            {
                //Mandamos a llamar a nuestra funcion de Grafo de prueba 2 para que se pueda imprimir (No lo llame en el start por que me salia error) 
                GrafoDePureba2(Origin, Target);
                return true;
            }

            // Marcamos el nodo como cerrado dado a que ya lo revisamos
            NodeStateDict[currentNode] = NodeState.Closed;

            // Actualizamos los nodos vecinos del nodo actual
            List<Edge> currentNeighbors = FindNeighbors(currentNode);
            // Visita a cada uno de ellos, hasta que se acaben o hasta que encontremos el objetivo.
            foreach (Edge e in currentNeighbors)
            {
                // Checamos cuál de los dos nodos que esta arista conecta no es el CurrentNode.
                Node neighborNode = (currentNode == e.a) ? e.b : e.a;

                // Si el vecino no ha sido visitado aún, lo añadimos a la cola para visitarlo
                if (NodeStateDict[neighborNode] == NodeState.Unknown)
                {
                    // Añadimos al nodo vecino a la cola que nos falta por visitar
                    queue.Enqueue(neighborNode);
                    // Marcamos el vecino como que esta abierto justamente para que lo podamos visitar
                    NodeStateDict[neighborNode] = NodeState.Open;
                    // Establecemos el nodo actual como el padre del vecino para visitarlo
                    neighborNode.parent = currentNode;
                }
            }
        }

        // No se encontró un camino
        return false;
    }

    private void GrafoDePureba2(Node origin, Node target)
    {
        // Prueba de BreathFirstSearch donde debería haber un camino
        List<Node> path = new List<Node>();
        Node currentNode = target;

        // Reconstruimos el camino recorrido desde el nodo objetivo hasta el origen
        while (currentNode != null)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        //Como en el ejercicio nos piden que el camino este invertido lo invertimos
        path.Reverse(); // Invertimos el camino para imprimirlo en el orden correcto

        // Imprimimos el camino
        foreach (Node node in path)
        {
            print("El nodo: " + node.ID + " fue parte del camino a la meta. (BreathFirstSerch)");
        }
    }

    //------------------------------------------------------------------------------------------------------------------------
    public bool DepthFirstSearch(Node Current, Node Target)
    {
        // Cuando tú te paras en un nodo, lo primero que tienes que hacer es si ya está en la lista cerrada.
        // Si ya está en la cerrada, no hay nada más que hacer.
        if (NodeStateDict[Current] == NodeState.Closed)
            return false;

        // Si el nodo donde estoy parado ahorita no es el nodo al que quiero llegar, entonces todavía no acabo.
        if (Current == Target)
            return true;
        // SI no son iguales, tenemos que seguir buscando.
        // Primero vamos al primer vecino de este nodo.

        // Tenemos que tener una forma de saber quiénes son los vecinos del nodo actual.
        // Hay que ver cuál de las aristas está conectada con H.
        // Vamos a hacer un método que nos diga con quién está conectado el nodo X.
        List<Edge> currentNeighbors = FindNeighbors(Current);
        // Visita a cada uno de ellos, hasta que se acaben o hasta que encontremos el objetivo.
        foreach (Edge e in currentNeighbors)
        {
            Node NonCurrentNode = Current != e.a ? e.a : e.b;
            // Primero checamos si ya está en la lista abierta, y si lo está, no mandamos a llamar el algoritmo.
            if (NodeStateDict[NonCurrentNode] == NodeState.Open)
                continue;
            else
            {
                // Si no, lo ponemos como que ya está en la lista abierta.
                NodeStateDict[NonCurrentNode] = NodeState.Open;
                // Cuando tú (Nodo Current) metes a otro nodo a la lista abierta, te pones como su parent node.
                NonCurrentNode.parent = Current;
            }

            // Marcamos el nodo como que está en la lista abierta.
            bool TargetFound = DepthFirstSearch(NonCurrentNode, Target);
            if (TargetFound)
            {
                print("El nodo: " + Current.ID + " fue parte del camino a la meta.");
                return true;
            }
        }

        NodeStateDict[Current] = NodeState.Closed;

        // Cuando ninguno de estos vecinos nos llevó al objetivo, regresamos False.
        return false;

    }

    // Método que nos dice quienes son los vecinos de un nodo dado.
    public List<Edge> FindNeighbors(Node in_node)
    {
        List<Edge> out_list = new List<Edge>(); // empieza vacío.
        // Checar todas las aristas que hay, y meter a este out_vector todas las aristas que referencien al nodo dado.
        foreach (Edge myEdge in Edges)
        {
            if (myEdge.a == in_node || myEdge.b == in_node)
            {
                out_list.Add(myEdge);
            }
        }

        return out_list;
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
    //REFERENCIAS:
    // Codigo usado en clase
    // https://docs.unity3d.com/355/Documentation/ScriptReference/Array.Reverse.html
    //
}