using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour {
    private List<LevelRoom> rooms = new List<LevelRoom>();

    private Canvas twoDMap;

    public IntVector2 size;

    public Image twoDCellPrefab;

    public Image twoDVertPrefab;

    public Image twoDHorzPrefab;

    public LevelPassage passagePrefab;

    public LevelWall wallPrefab;

    public LevelBreakableWall breakableWallPrefab;

    public LevelCell cellPrefab;

    private LevelCell[,] cells;

    private LevelCell[,] postOneCells;
                         
    private LevelCell[,] postTwoCells;

    private LevelCell[,] hallways = new LevelCell[2, 1];

    public LevelRoomMaterials[] roomMats;

    public Material[] breakableMats;

    public float newRoomProbability;

    public float breakableWallProability;

    public float generationStepDelay;

    public List<Node> nodeSystem = new List<Node>();

    public List<Node> tempNodeList;

    private List<LevelBreakableWall> breakableWalls = new List<LevelBreakableWall>();

    public int seed;

    public float midX;

    public float midY;

    public bool finishGeneration = false;

    private void Update()
    {
        foreach (LevelBreakableWall breakable in breakableWalls)
        {
            if (breakable != null && breakable.health <= 0)
            {
                LevelCellEdge otherWall = breakable.otherCell.GetEdge(breakable.direction.GetOpposite());
                CreatePassage(breakable.cell, breakable.otherCell, breakable.direction, twoDMap, true);
                if (otherWall.image != null)
                {
                    Destroy(otherWall.image.gameObject);
                }
                if (breakable.image != null)
                {
                    Destroy(breakable.image.gameObject);
                }
                Destroy(otherWall.gameObject);
                Destroy(breakable.gameObject);
            }
        }
    }

    public IntVector2 RandomCoordinates(IntVector2 size)
    {
        return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));        
    }

    public bool ContainsCoordinates(IntVector2 coordinate, IntVector2 size, IntVector2 offset)
    {
        return coordinate.x >= 0 + offset.x && coordinate.x < size.x + offset.x && coordinate.z >= 0 + offset.z && coordinate.z < size.z + offset.z;
    }

    public void PostGenerateOne(Canvas twoDMap, int seed, int aiSeed)
    {
        Random.seed = seed;
        IntVector2 newSize = new IntVector2(size.x / 2, size.z / 2);
        IntVector2 offset = new IntVector2(-newSize.x - 1, (size.z - newSize.z) / 2);
        postOneCells = new LevelCell[newSize.x, newSize.z];
        List<LevelCell> activeCells = new List<LevelCell>();
        IntVector2 coordinates = new IntVector2(offset.x + newSize.x - 1, ((newSize.z - 1) / 2) + offset.z);
        IntVector2 roomCoordinates = new IntVector2(coordinates.x + 2, coordinates.z);
        DoFirstPostGenerationStep(postOneCells, activeCells, coordinates, roomCoordinates, twoDMap, offset, newSize);
        while (activeCells.Count > 0)
        {
            DoNextGenerationStep(postOneCells, activeCells, twoDMap, offset, newSize);
        }
        Destroy(postOneCells[coordinates.x - offset.x, coordinates.z - offset.z].GetEdge(LevelDirection.East).image.gameObject);
        Destroy(this.cells[roomCoordinates.x, roomCoordinates.z].GetEdge(LevelDirection.West).image.gameObject);
        Destroy(postOneCells[coordinates.x - offset.x, coordinates.z - offset.z].GetEdge(LevelDirection.East).gameObject);
        Destroy(this.cells[roomCoordinates.x, roomCoordinates.z].GetEdge(LevelDirection.West).gameObject);
        LevelCell cell = CreateHallwayCell(new IntVector2(roomCoordinates.x - 1, roomCoordinates.z), new IntVector2(1, 1), twoDMap);
        cell.InitializeCell(this.cells[roomCoordinates.x, roomCoordinates.z].room);
        CreateTwoDCell(cell, twoDMap);
        CreateWall(cell, null, LevelDirection.North, twoDMap);
        CreateWall(cell, null, LevelDirection.South, twoDMap);
        CreatePassageInSameRoom(cell, postOneCells[coordinates.x - offset.x, coordinates.z - offset.z], LevelDirection.West, twoDMap);
        CreatePassageInSameRoom(cell, this.cells[roomCoordinates.x, roomCoordinates.z], LevelDirection.East, twoDMap);
        CleanLevel(postOneCells, offset);
        Random.seed = aiSeed;
        StartCoroutine(CreatePostConnections());
    }

    public void PostGenerateTwo(Canvas twoDMap, int seed, int aiSeed)
    {
        Random.seed = seed;
        IntVector2 newSize = new IntVector2(size.x / 2, size.z / 2);
        IntVector2 offset = new IntVector2(size.x + 1, (size.z - newSize.z) / 2);
        postTwoCells = new LevelCell[newSize.x, newSize.z];
        List<LevelCell> activeCells = new List<LevelCell>();
        IntVector2 coordinates = new IntVector2(offset.x, ((newSize.z - 1) / 2) + offset.z);
        IntVector2 roomCoordinates = new IntVector2(coordinates.x - 2, coordinates.z);
        DoFirstPostGenerationStep(postTwoCells, activeCells, coordinates, roomCoordinates, twoDMap, offset, newSize);
        while (activeCells.Count > 0)
        {
            DoNextGenerationStep(postTwoCells, activeCells, twoDMap, offset, newSize);
        }
        Destroy(postTwoCells[coordinates.x - offset.x, coordinates.z - offset.z].GetEdge(LevelDirection.West).image.gameObject);
        Destroy(this.cells[roomCoordinates.x, roomCoordinates.z].GetEdge(LevelDirection.East).image.gameObject);
        Destroy(postTwoCells[coordinates.x - offset.x, coordinates.z - offset.z].GetEdge(LevelDirection.West).gameObject);
        Destroy(this.cells[roomCoordinates.x, roomCoordinates.z].GetEdge(LevelDirection.East).gameObject);
        LevelCell cell = CreateHallwayCell(new IntVector2(roomCoordinates.x + 1, roomCoordinates.z), new IntVector2(2, 1), twoDMap);
        cell.InitializeCell(this.cells[roomCoordinates.x, roomCoordinates.z].room);
        CreateTwoDCell(cell, twoDMap);
        CreateWall(cell, null, LevelDirection.North, twoDMap);
        CreateWall(cell, null, LevelDirection.South, twoDMap);
        CreatePassageInSameRoom(cell, postTwoCells[coordinates.x - offset.x, coordinates.z - offset.z], LevelDirection.East, twoDMap);
        CreatePassageInSameRoom(cell, this.cells[roomCoordinates.x, roomCoordinates.z], LevelDirection.West, twoDMap);
        CleanLevel(postTwoCells, offset);
        Random.seed = aiSeed;
        StartCoroutine(CreatePostConnections());
    }


    private IEnumerator CreatePostConnections()
    {
        yield return new WaitForSeconds(0.0f);
        CreateConnections();
        finishGeneration = true;
    }

    public LineRenderer linePrefab;

    public IEnumerator GenerateCinematically(Canvas twoDMap)
    {
        this.twoDMap = twoDMap;
        IntVector2 offset = new IntVector2(0, 0);
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);
        cells = new LevelCell[size.x, size.z];
        midX = (size.x - 1) / 2.0f;
        midY = (size.z - 1) / 2.0f;
        List<LevelCell> activeCells = new List<LevelCell>();
        DoFirstGenerationStep(cells, activeCells, RandomCoordinates(size), twoDMap, offset, size);
        while (activeCells.Count > 0)
        {
            yield return delay;
            DoNextGenerationStep(cells, activeCells, twoDMap, offset, size);
        }
    }

    public void Generate(Canvas twoDMap)
    {
        this.twoDMap = twoDMap;
        IntVector2 offset = new IntVector2(0, 0);
        cells = new LevelCell[size.x, size.z];
        midX = (size.x - 1) / 2.0f;
        midY = (size.z - 1) / 2.0f;
        List<LevelCell> activeCells = new List<LevelCell>();
        DoFirstGenerationStep(cells, activeCells, RandomCoordinates(size), twoDMap, offset, size);
        while (activeCells.Count > 0)
        {
            DoNextGenerationStep(cells, activeCells, twoDMap, offset, size);
        }
        CleanLevel(cells, offset);
        CreateConnections();
    }

    private void CleanLevel(LevelCell[,] areaCells, IntVector2 offset)
    {
        List<LevelRoom> deadRooms = new List<LevelRoom>();
        foreach (LevelRoom room in rooms)
        {
            if (room.cells.Count <= 4)
            {
                deadRooms.Add(room);
                bool roomCleaned = false;
                int cellIndex = 0;
                LevelCell cell = room.cells[0];
                LevelDirection direction = 0;
                while (!roomCleaned && cellIndex < room.cells.Count)
                {
                    cell = room.cells[cellIndex];
                    while ((int)direction < 4 && !roomCleaned)
                    {
                        if (cell.GetEdge(direction) is LevelPassage)
                        {
                            IntVector2 coordinates = cell.coordinates + direction.ToIntVector2();
                            LevelCell otherCell = areaCells[coordinates.x - offset.x, coordinates.z - offset.z];
                            if (otherCell.room != cell.room)
                            {
                                Assimilate(otherCell.room, cell.room);
                                roomCleaned = true;
                            }
                        }
                        direction++;
                    }
                    cellIndex++;
                }
            }
        }
        foreach(LevelRoom room in deadRooms)
        {
            rooms.Remove(room);
            Destroy(room);
        }
    }

    private void Assimilate(LevelRoom room, LevelRoom toAssimilate)
    {
        LevelRoomMaterials materials = room.mats;
        foreach(LevelCell cell in toAssimilate.cells)
        {
            cell.transform.GetChild(0).GetComponent<Renderer>().material = materials.floorMaterial;
            cell.twoDImage.color = materials.floorMaterial.color;
            foreach (LevelCellEdge edge in cell.edges)
            {
                if (edge is LevelPassage)
                {
                    if (edge.image != null)
                    {
                        edge.image.color = materials.floorMaterial.color;
                    }
                }
            }
            foreach (LevelCellEdge edge in cell.edges)
            {
                if (edge is LevelWall)
                {
                    ((LevelWall)(edge)).wall.GetComponent<Renderer>().material = materials.wallMaterial;
                }
                else if (edge is LevelBreakableWall)
                {
                    ((LevelBreakableWall)(edge)).wall.GetComponent<Renderer>().material = breakableMats[room.matIndex];
                }
            }
        }
        room.Join(toAssimilate);
    }

    public IEnumerator CleanLevelCinematically()
    {
        LevelCell[,] areaCells = cells;
        IntVector2 offset = new IntVector2(0, 0);
        List<LevelRoom> deadRooms = new List<LevelRoom>();
        foreach (LevelRoom room in rooms)
        {
            if (room.cells.Count <= 4)
            {
                deadRooms.Add(room);
                bool roomCleaned = false;
                int cellIndex = 0;
                LevelCell cell = room.cells[0];
                LevelDirection direction = 0;
                while (!roomCleaned && cellIndex < room.cells.Count)
                {
                    cell = room.cells[cellIndex];
                    while ((int)direction < 4 && !roomCleaned)
                    {
                        if (cell.GetEdge(direction) is LevelPassage)
                        {
                            IntVector2 coordinates = cell.coordinates + direction.ToIntVector2();
                            LevelCell otherCell = areaCells[coordinates.x - offset.x, coordinates.z - offset.z];
                            if (otherCell.room != cell.room)
                            {
                                StartCoroutine(AssimilateCinematically(otherCell.room, cell.room));
                                roomCleaned = true;
                            }
                        }
                        direction++;
                    }
                    cellIndex++;
                }
            }
            yield return new WaitForSeconds(0.0f);
        }
        foreach (LevelRoom room in deadRooms)
        {
            rooms.Remove(room);
            Destroy(room);
        }
    }

    private IEnumerator AssimilateCinematically(LevelRoom room, LevelRoom toAssimilate)
    {
        LevelRoomMaterials materials = room.mats;
        foreach (LevelCell cell in toAssimilate.cells)
        {
            cell.transform.GetChild(0).GetComponent<Renderer>().material = materials.floorMaterial;
            cell.twoDImage.color = materials.floorMaterial.color;
            foreach (LevelCellEdge edge in cell.edges)
            {
                if (edge is LevelPassage)
                {
                    if (edge.image != null)
                    {
                        edge.image.color = materials.floorMaterial.color;
                    }
                }
            }
            foreach (LevelCellEdge edge in cell.edges)
            {
                if (edge is LevelWall)
                {
                    ((LevelWall)(edge)).wall.GetComponent<Renderer>().material = materials.wallMaterial;
                }
                else if (edge is LevelBreakableWall)
                {
                    ((LevelBreakableWall)(edge)).wall.GetComponent<Renderer>().material = breakableMats[room.matIndex];
                }
            }
            yield return new WaitForSeconds(0.0f);
        }
        room.Join(toAssimilate);
    }

    private void CreateTwoDCell(LevelCell cell, Canvas twoDMap)
    {
        Image newImage = Instantiate(twoDCellPrefab) as Image;
        newImage.transform.SetParent(twoDMap.transform, false);
        newImage.transform.localPosition = new Vector3((cell.coordinates.x - midX) * 1.0f, (cell.coordinates.z - midY) * 1.0f, 0);
        newImage.color = cell.room.mats.floorMaterial.color;
        newImage.transform.SetAsFirstSibling();
        cell.twoDImage = newImage;
    }

    private void CreateTwoDPassage(LevelCell cell, Canvas twoDMap, LevelDirection direction)
    {
        IntVector2 positionMultiplier = direction.ToIntVector2();
        Vector3 cellPosition = new Vector3((cell.coordinates.x - midX) * 1.0f, (cell.coordinates.z - midY) * 1.0f, 0);
        Vector2 offset = new Vector2(positionMultiplier.x * 0.5f, positionMultiplier.z * 0.5f);
        Image newImage = null;
        if (positionMultiplier.x != 0)
        {
            newImage = Instantiate(twoDVertPrefab) as Image;
        }
        else
        {
            newImage = Instantiate(twoDHorzPrefab) as Image;
        }
        newImage.transform.SetParent(twoDMap.transform, false);
        newImage.transform.localPosition = new Vector3(cellPosition.x + offset.x, cellPosition.y + offset.y, 0);
        newImage.color = cell.room.mats.floorMaterial.color;
        newImage.transform.SetAsFirstSibling();
        cell.GetEdge(direction).image = newImage;

    }

    private void CreateTwoDWall(LevelCell cell, Canvas twoDMap, LevelDirection direction)
    {
        IntVector2 positionMultiplier = direction.ToIntVector2();
        Vector3 cellPosition = new Vector3((cell.coordinates.x - midX) * 1.0f, (cell.coordinates.z - midY) * 1.0f, 0);
        Vector2 offset = new Vector2(positionMultiplier.x * 0.5f, positionMultiplier.z * 0.5f);
        Image newImage = null;
        if (positionMultiplier.x != 0)
        {
            newImage = Instantiate(twoDVertPrefab) as Image;
        }
        else
        {
            newImage = Instantiate(twoDHorzPrefab) as Image;
        }
        newImage.transform.SetParent(twoDMap.transform, false);
        newImage.transform.localPosition = new Vector3(cellPosition.x + offset.x, cellPosition.y + offset.y, 0);
        newImage.color = Color.black;
        newImage.transform.SetAsLastSibling();
        cell.GetEdge(direction).image = newImage;
    }

    private void DoFirstGenerationStep(LevelCell [,] cells, List<LevelCell> activeCells, IntVector2 coordinates, Canvas twoDMap, IntVector2 offset, IntVector2 size)
    {
        tempNodeList = new List<Node>();
        coordinates = new IntVector2(coordinates.x + offset.x, coordinates.z + offset.z);
        LevelCell cell = CreateCell(cells, coordinates, twoDMap, offset);
        cell.InitializeCell(CreateRoom(-1));
        CreateTwoDCell(cell, twoDMap);
        activeCells.Add(cell);
    }

    private void DoFirstPostGenerationStep(LevelCell[,] cells, List<LevelCell> activeCells, IntVector2 coordinates, IntVector2 roomCoordinates, Canvas twoDMap, IntVector2 offset, IntVector2 size)
    {
        tempNodeList = new List<Node>();
        LevelCell cell = CreateCell(cells, coordinates, twoDMap, offset);
        cell.InitializeCell(this.cells[roomCoordinates.x, roomCoordinates.z].room);
        CreateTwoDCell(cell, twoDMap);
        activeCells.Add(cell);
    }

    private void DoNextGenerationStep(LevelCell [,] cells, List<LevelCell> activeCells, Canvas twoDMap, IntVector2 offset, IntVector2 size)
    {
        int currentIndex = activeCells.Count - 1;
        LevelCell currentCell = activeCells[currentIndex];
        if (currentCell.IsFullyInitialized)
        {
            activeCells.RemoveAt(currentIndex);
            return;
        }
        LevelDirection direction = currentCell.RandomUninitializedDirection;
        IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
        if (ContainsCoordinates(coordinates, size, offset))
        {
            LevelCell neighbor = cells[coordinates.x - offset.x, coordinates.z - offset.z];
            if (neighbor == null)
            {
                neighbor = CreateCell(cells, coordinates, twoDMap, offset);
                CreatePassage(currentCell, neighbor, direction, twoDMap, false);
                activeCells.Add(neighbor);
            }
            else if (currentCell.room == neighbor.room)
            {
                CreatePassageInSameRoom(currentCell, neighbor, direction, twoDMap);
            }
            else
            {
                if (currentCell.room.matIndex == neighbor.room.matIndex)
                {
                    CreatePassageInSameRoom(currentCell, neighbor, direction, twoDMap);
                }
                else
                {
                    if (Random.value < breakableWallProability)
                    {
                        CreateBreakableWall(currentCell, neighbor, direction, twoDMap);
                    }
                    else
                    {
                        CreateWall(currentCell, neighbor, direction, twoDMap);
                    }
                }
            }
        }
        else
        {
            CreateWall(currentCell, null, direction, twoDMap);
        }
    }

    private LevelCell CreateCell(LevelCell[,] cells, IntVector2 coordinates, Canvas twoDMap, IntVector2 offset)
    {
        LevelCell newCell = Instantiate(cellPrefab) as LevelCell;
        cells[coordinates.x - offset.x, coordinates.z - offset.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + newCell.coordinates.x + ", " + newCell.coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition =
            new Vector3(newCell.coordinates.x - size.x * 0.5f + 0.5f, 0f, newCell.coordinates.z - size.z * 0.5f + 0.5f);
        CreateNodes(newCell.coordinates);
        
        return newCell;
    }

    private LevelCell CreateHallwayCell(IntVector2 coordinates, IntVector2 arrayCoordinates, Canvas twoDMap)
    {
        LevelCell newCell = Instantiate(cellPrefab) as LevelCell;
        cells[arrayCoordinates.x, arrayCoordinates.z] = newCell;
        newCell.coordinates = coordinates;
        newCell.name = "Maze Cell " + newCell.coordinates.x + ", " + newCell.coordinates.z;
        newCell.transform.parent = transform;
        newCell.transform.localPosition =
            new Vector3(newCell.coordinates.x - size.x * 0.5f + 0.5f, 0f, newCell.coordinates.z - size.z * 0.5f + 0.5f);
        CreateNodes(newCell.coordinates);

        return newCell;
    }

    private void CreateNodes(IntVector2 coordinates)
    {
        CreateNode(coordinates, new Vector2(0.5f, 0.5f));
    }

    private void CreateNode(IntVector2 coordinates, Vector2 offset)
    {
        Node newNode = new Node();
        newNode.position = new Vector3(coordinates.x - size.x * 0.5f + offset.x, 0.01f, coordinates.z - size.z * 0.5f + offset.y);
        newNode.id = nodeSystem.Count + tempNodeList.Count;
        tempNodeList.Add(newNode);
    }

    private void CreateConnections()
    {
        foreach (Node node in tempNodeList)
        {
            for (int i = node.id + 1; i < tempNodeList.Count + nodeSystem.Count; i++)
            {
                Node checkNode = tempNodeList[i - nodeSystem.Count];
                if (Vector3.Distance(node.position, checkNode.position) < 3.0f && IsLineObstructed(node.position, checkNode.position))
                {
                    node.connections.Add(checkNode.id);
                    checkNode.connections.Add(node.id);
                    //Debug.DrawLine(node.position, checkNode.position, Color.red, 15.0f);
                }
            }
            for (int i = 0; i < nodeSystem.Count; i++)
            {
                Node checkNode = nodeSystem[i];
                if (Vector3.Distance(node.position, checkNode.position) < 3.0f && IsLineObstructed(node.position, checkNode.position))
                {
                    node.connections.Add(checkNode.id);
                    checkNode.connections.Add(node.id);
                    //Debug.DrawLine(node.position, checkNode.position, Color.red, 15.0f);
                }
            }
        }
        foreach (Node node in tempNodeList)
        {
            nodeSystem.Add(node);
        }
    }

    public IEnumerator CreateConnectionsCinematically()
    {
        WaitForSeconds delay = new WaitForSeconds(0.025f);
        foreach (Node node in tempNodeList)
        {
            for (int i = node.id + 1; i < tempNodeList.Count + nodeSystem.Count; i++)
            {
                Node checkNode = tempNodeList[i - nodeSystem.Count];
                if (Vector3.Distance(node.position, checkNode.position) < 3.0f && IsLineObstructed(node.position, checkNode.position))
                {
                    node.connections.Add(checkNode.id);
                    checkNode.connections.Add(node.id);
                    LineRenderer line = Instantiate(linePrefab) as LineRenderer;
                    line.SetPosition(0, node.position);
                    line.SetPosition(1, checkNode.position);
                }
            }
            for (int i = 0; i < nodeSystem.Count; i++)
            {
                Node checkNode = nodeSystem[i];
                if (Vector3.Distance(node.position, checkNode.position) < 3.0f && IsLineObstructed(node.position, checkNode.position))
                {
                    node.connections.Add(checkNode.id);
                    checkNode.connections.Add(node.id);
                    LineRenderer line = Instantiate(linePrefab) as LineRenderer;
                    line.SetPosition(0, node.position);
                    line.SetPosition(1, checkNode.position);
                }
            }
            yield return delay;
        }
        foreach (Node node in tempNodeList)
        {
            nodeSystem.Add(node);
        }
    }

    private bool IsLineObstructed(Vector3 pos1, Vector3 pos2)
    {
        float length = Vector3.Distance(pos1, pos2);
        Vector3 direction = pos2 - pos1;

        Vector3 leftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * 0.2f + pos1;
        Vector3 rightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * -0.2f + pos1;

        RaycastHit[] leftInfos = Physics.RaycastAll(leftPos1, direction, length);
        RaycastHit[] rightInfos = Physics.RaycastAll(rightPos1, direction, length);

        bool leftHit = false;
        bool rightHit = false;

        foreach(RaycastHit info in leftInfos)
        {
            if (info.collider.gameObject.tag != "Breakable" && info.collider.gameObject.tag != "Weapon" && info.collider.gameObject.tag != "Unit")
            {
                leftHit = true;
            }
        }
        foreach (RaycastHit info in rightInfos)
        {
            if (info.collider.gameObject.tag != "Breakable" && info.collider.gameObject.tag != "Weapon" && info.collider.gameObject.tag != "Unit")
            {
                rightHit = true;
            }
        }

        return !(leftHit || rightHit);
    }

    private void CreatePassage(LevelCell cell, LevelCell otherCell, LevelDirection direction, Canvas twoDMap, bool existingCell)
    {
        LevelPassage passage = Instantiate(passagePrefab) as LevelPassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as LevelPassage;
        if (!existingCell)
        {
            if (Random.value < newRoomProbability)
            {
                otherCell.InitializeCell(CreateRoom(cell.room.matIndex));
            }
            else
            {
                otherCell.InitializeCell(cell.room);
            }       
            CreateTwoDCell(otherCell, twoDMap);
        }
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        CreateTwoDPassage(cell, twoDMap, direction);
    }

    private void CreateBreakableWall(LevelCell cell, LevelCell otherCell, LevelDirection direction, Canvas twoDMap)
    {
        LevelBreakableWall wall = Instantiate(breakableWallPrefab) as LevelBreakableWall;
        wall.material = breakableMats[cell.room.matIndex];
        wall.Initialize(cell, otherCell, direction);
        breakableWalls.Add(wall);
        LevelBreakableWall otherWall = Instantiate(breakableWallPrefab) as LevelBreakableWall;
        otherWall.material = breakableMats[otherCell.room.matIndex];
        otherWall.Initialize(otherCell, cell, direction.GetOpposite());
        breakableWalls.Add(otherWall);
        CreateTwoDWall(cell, twoDMap, direction);
    }

    private void CreateWall(LevelCell cell, LevelCell otherCell, LevelDirection direction, Canvas twoDMap)
    {
        LevelWall wall = Instantiate(wallPrefab) as LevelWall;
        wall.Initialize(cell, otherCell, direction);
        if (otherCell != null)
        {
            wall = Instantiate(wallPrefab) as LevelWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
        }
        CreateTwoDWall(cell, twoDMap, direction);
    }

    private void CreatePassageInSameRoom(LevelCell cell, LevelCell otherCell, LevelDirection direction, Canvas twoDMap)
    {
        LevelPassage passage = Instantiate(passagePrefab) as LevelPassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as LevelPassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        if (cell.room != otherCell.room)
        {
            LevelRoom joinRoom = otherCell.room;
            cell.room.Join(joinRoom);
            rooms.Remove(joinRoom);
            Destroy(joinRoom);
        }
        CreateTwoDPassage(cell, twoDMap, direction);
    }

    private LevelRoom CreateRoom(int excludedIndex)
    {
        LevelRoom room = ScriptableObject.CreateInstance<LevelRoom>();
        room.matIndex = Random.Range(0, roomMats.Length);
        if (room.matIndex == excludedIndex)
        {
            room.matIndex = (room.matIndex + 1) % roomMats.Length;
        }
        room.mats = roomMats[room.matIndex];
        rooms.Add(room);
        return room;
    }
}
