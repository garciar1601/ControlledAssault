using UnityEngine;
using System.Collections.Generic;

public class Node {
    public int id;
    public Vector3 position;
    public List<int> connections = new List<int>();
}
