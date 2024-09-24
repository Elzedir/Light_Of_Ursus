using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
public enum MazeType { Standard, Chase, Collect, Doors }
public class Spawner_Maze : MonoBehaviour
{
    List<MazeType> _mousemazeTypes;

    public Cell_MouseMaze[,,] Cells { get; private set; }
    Transform _cellParent;
    int _cellCount = 0;
    Cell_MouseMaze _playerLastCell;
    Cell_MouseMaze _furthestCell;
    int _maxDistance;

    int _width = 10;
    int _height = 2;
    int _depth = 10;
    //int _subVoxelScale = 10;

    int _visibility = 5;
    int _wallBreaks = 100;
    float _newPathChance = 0.8f;

    public bool Background = false;
    public static bool Initialised { get; private set; } = false;
    Vector3 _startPosition;

    Controller_Puzzle_MouseMaze _player;

    #region Chaser
    Transform _chaserParent;
    List<Chaser> _chasers = new();
    int _chaserCount = 1;
    float _chaserSpawnDelay = 2f;
    float _chaserSpawnInterval = 2f;
    (float, float) _chaserSpeeds = (0.75f, 2);
    #endregion
    #region Collect
    Transform _collectParent;
    Dictionary<Collectable_MouseMaze, bool> _collectables = new();
    int _collectableMinimumDistance = 1;
    int _collectableSpawnChance = 1;
    int _collectableCount = 5;
    #endregion
    #region Door
    Transform _doorParent;
    Dictionary<Door_Base, bool> _doors = new();
    Dictionary<Door_Key, bool> _keys = new();
    int _keysMinimumDistance = 1;
    int _doorCount = 1;
    #endregion
    void Start()
    {
        _mousemazeTypes = new List<MazeType>() {
        MazeType.Standard,
        MazeType.Collect,
        MazeType.Chase,
        MazeType.Collect,
        // MazeType.Doors
        } ;

        InitialisePuzzle();
    }

    void InitialisePuzzle()
    {
        Controller_Camera.Instance.SetOffset(new Vector3(0, 30, 0), Quaternion.Euler(90, 0, 0));
        Controller_Sun.Instance.SetLightPositionAndRotation(new Vector3(0, 30, 0), Quaternion.Euler(90, 0, 0));

        _cellParent = GameObject.Find("CellParent").transform;
        _chaserParent = GameObject.Find("ChaserParent").transform;
        _collectParent = GameObject.Find("CollectParent").transform;
        _doorParent = GameObject.Find("DoorParent").transform;
        _player = GameObject.Find("Focus").GetComponent<Controller_Puzzle_MouseMaze>();
        _player.OnBreakWall += BreakWall;

        if (Manager_Puzzle.Instance.Puzzle.PuzzleData.PuzzleState.PuzzleType == PuzzleType.Fixed) SpawnFixedPuzzle();
        else StartCoroutine(SpawnRandomPuzzle());
    }

    void SpawnFixedPuzzle()
    {
        
    }

    IEnumerator SpawnRandomPuzzle()
    {
        Cells = new Cell_MouseMaze[_width, _height, _depth];

        for (int wid = 0; wid < _width; wid++)
        {
            for (int hei = 0; hei < _height; hei++)
            {
                for (int dep = 0; dep < _depth; dep++)
                {
                    yield return Cells[wid, hei, dep] = CreateCell(wid, hei, dep);
                }
            }
        }

        _startPosition = Cells[0, 0, 0].Position;

        VoxelGrid.InitializeVoxelGrid(width: _width, height: _height, depth: _depth, offset: new Vector3(0.5f, 0, 0.5f));

        yield return StartCoroutine(CreateMaze(null, Cells[0, 1, 0], 0));

        yield return StartCoroutine(clearDoubleWalls());

        yield return StartCoroutine(processMazeTypes());

        if (_mousemazeTypes.Contains(MazeType.Doors))
        {
            for (int i = 0; i < _doorCount; i++)
            {
                // Change this to account for multiple doors
                SpawnKey(SpawnDoor(_furthestCell));
            }
        }

        Initialised = true;

        IEnumerator clearDoubleWalls()
        {
            foreach(Cell_MouseMaze cell in Cells)
            {
                if (checkExistingWall(cell.transform, cell.Position + new Vector3(0, 0, 0.5f))) cell.ClearWall(Wall.Top);

                if (checkExistingWall(cell.transform, cell.Position + new Vector3(0, 0, -0.5f))) cell.ClearWall(Wall.Bottom);

                if (checkExistingWall(cell.transform, cell.Position + new Vector3(0.5f, 0, 0))) cell.ClearWall(Wall.Right);

                if (checkExistingWall(cell.transform, cell.Position + new Vector3(-0.5f, 0, 0))) cell.ClearWall(Wall.Left);

                yield return null;
            }

            bool checkExistingWall(Transform cellTransform, Vector3 position)
            {
                return Physics.OverlapSphere(position, 0.1f).Any(hit => hit.gameObject.layer == LayerMask.NameToLayer("Wall") && hit.transform.parent != cellTransform);
            }
        }

        IEnumerator processMazeTypes()
        {
            foreach (var type in _mousemazeTypes)
            {
                switch (type)
                {
                    case MazeType.Standard:
                        List<Wall> walls = new();
                        foreach(Cell_MouseMaze cell in GetNeighbours(_furthestCell))
                        {
                            walls.Add(ClearWalls(_furthestCell, cell, true).Item2);
                        }
                        _furthestCell.RecreateWalls(walls, Resources.GetBuiltinResource<Mesh>("Cube.fbx"), Resources.Load<Material>("Materials/Material_White"));
                        _furthestCell.MarkCell(Resources.Load<Material>("Materials/Material_Red"));
                        Cells[(int)_furthestCell.Position.x, (int)_furthestCell.Position.y - 1, (int)_furthestCell.Position.z].MarkCell(Resources.Load<Material>("Materials/Material_Red"));
                        break;
                    case MazeType.Chase:
                        StartCoroutine(SpawnChasers());
                        break;
                    case MazeType.Collect:
                        SpawnCollectables();
                        break;
                    case MazeType.Doors:
                        SpawnDoors();
                        break;
                }
                yield return null;
            }
        }
    }

    void SpawnCollectables()
    {
        while (_collectables.Count < _collectableCount)
        {
            var cell = Cells[Random.Range(0, _width), Random.Range(0, _height), Random.Range(0, _depth)];

            if (Vector3.Distance(cell.Position, _startPosition) >= _collectableMinimumDistance) SpawnCollectable(cell.transform.position);
        }
    }

    void SpawnDoors()
    {
        for (int i = 0; i < _doorCount; i++)
        {
            var cell = Cells[Random.Range(0, _width), Random.Range(0, _height), Random.Range(0, _depth)];
            var door = SpawnDoor(cell);
            SpawnKey(door);
        }
    }


    Cell_MouseMaze CreateCell(int wid, int hei, int dep)
    {
        GameObject cellGO = new GameObject($"cell{wid}_{hei}_{dep}");
        cellGO.transform.position = new Vector3(wid, hei, dep);
        cellGO.transform.rotation = Quaternion.identity;
        cellGO.transform.parent = _cellParent;
        Cell_MouseMaze cell = cellGO.AddComponent<Cell_MouseMaze>();
        cell.InitialiseCell(new Vector3Int(wid, hei, dep), this);

        return cell;
    }

    IEnumerator CreateMaze(Cell_MouseMaze previousCell, Cell_MouseMaze currentCell, int distanceFromStart)
    {
        currentCell.Visited = true;
        _cellCount++;

        currentCell.Disable();

        ClearWalls(previousCell, currentCell, true);

        if (_mousemazeTypes.Contains(MazeType.Collect) && _collectables.Count < _collectableCount && _cellCount > _collectableMinimumDistance && Random.Range(0, 100) <= _collectableSpawnChance)
        {
            SpawnCollectable(currentCell.transform.position);
        }

        if (distanceFromStart > _maxDistance)
        {
            _maxDistance = distanceFromStart;
            _furthestCell = currentCell;
        }

        var unvisitedNeighbors = GetNeighbours(currentCell, true).OrderBy(_ => Random.Range(1, 10)).ToList();

        if (unvisitedNeighbors.Count <= 0) yield break;

        if (Random.Range(0, 1f) < _newPathChance && unvisitedNeighbors.Count > 0)
        {
            foreach (Cell_MouseMaze neighbour in unvisitedNeighbors)
            {
                if (neighbour != null && !neighbour.Visited)
                {
                    yield return StartCoroutine(CreateMaze(currentCell, unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)], distanceFromStart + 1));
                }
            }
        }
        else
        {
            foreach (Cell_MouseMaze neighbour in unvisitedNeighbors)
            {
                if (neighbour != null && !neighbour.Visited)
                {
                    yield return StartCoroutine(CreateMaze(currentCell, neighbour, distanceFromStart + 1));
                }
            }
        }
    }

    (bool, Wall) ClearWalls(Cell_MouseMaze currentCell, Cell_MouseMaze nextCell, bool initialisation = false)
    {
        if (currentCell == null) return (false, Wall.None);

        if (currentCell.transform.position.x < nextCell.transform.position.x)
        {
            VoxelGrid.RemoveVoxelAtPosition(currentCell.ClearWall(Wall.Right, initialisation));
            VoxelGrid.RemoveVoxelAtPosition(nextCell.ClearWall(Wall.Left, initialisation));
            return (true, Wall.Left);
        }

        if (currentCell.transform.position.x > nextCell.transform.position.x)
        {
            VoxelGrid.RemoveVoxelAtPosition(currentCell.ClearWall(Wall.Left, initialisation));
            VoxelGrid.RemoveVoxelAtPosition(nextCell.ClearWall(Wall.Right, initialisation));
            return (true, Wall.Right);
        }

        if (currentCell.transform.position.z < nextCell.transform.position.z)
        {
            VoxelGrid.RemoveVoxelAtPosition(currentCell.ClearWall(Wall.Top, initialisation));
            VoxelGrid.RemoveVoxelAtPosition(nextCell.ClearWall(Wall.Bottom, initialisation));
            return (true, Wall.Bottom);
        }

        if (currentCell.transform.position.z > nextCell.transform.position.z)
        {
            VoxelGrid.RemoveVoxelAtPosition(currentCell.ClearWall(Wall.Bottom, initialisation));
            VoxelGrid.RemoveVoxelAtPosition(nextCell.ClearWall(Wall.Top, initialisation));
            return (true, Wall.Top);
        }

        return (false, Wall.None);
    }

    IEnumerable<Cell_MouseMaze> GetNeighbours(Cell_MouseMaze currentCell, bool onlyUnvisited = false)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        if (x + 1 < _width)
        {
            if (!onlyUnvisited || (onlyUnvisited && !Cells[x + 1, 1, z].Visited))
            {
                yield return Cells[x + 1, 1, z];
            }
        }

        if (x - 1 >= 0)
        {
            if (!onlyUnvisited || (onlyUnvisited && !Cells[x - 1, 1, z].Visited))
            {
                yield return Cells[x - 1, 1, z];
            }
        }

        if (z + 1 < _depth)
        {
            if (!onlyUnvisited || (onlyUnvisited && !Cells[x, 1, z + 1].Visited))
            {
                yield return Cells[x, 1, z + 1];
            }
        }

        if (z - 1 >= 0)
        {
            if (!onlyUnvisited || (onlyUnvisited && !Cells[x, 1, z - 1].Visited))
            {
                yield return Cells[x, 1, z - 1];
            }
        }
    }

    void SpawnCollectable(Vector3 position)
    {
        GameObject collectableGO = new GameObject($"Collectable_{_collectables.Count}");
        collectableGO.transform.parent = _collectParent;
        collectableGO.transform.position = position;
        Collectable_MouseMaze collectable = collectableGO.AddComponent<Collectable_MouseMaze>();
        _collectables.Add(collectable, false);
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        Material material = Resources.Load<Material>("Materials/Material_Blue");
        collectable.SpawnCollectable(this, mesh, material);
    }

    IEnumerator SpawnChasers()
    {
        Mesh mesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
        Material material = Resources.Load<Material>("Materials/Material_Red");

        yield return new WaitForSeconds(_chaserSpawnDelay);

        for (int i = 0; i < _chaserCount; i++)
        {
            SpawnChaser(mesh, material);

            yield return new WaitForSeconds(_chaserSpawnInterval);
        }
    }

    void SpawnChaser(Mesh mesh, Material material)
    {
        GameObject chaserGO = new GameObject($"Chaser_{_chasers.Count}");
        chaserGO.transform.parent = _chaserParent;
        chaserGO.transform.localPosition = new Vector3(0, 1, 0);
        Chaser chaser = chaserGO.AddComponent<Chaser>();
        _chasers.Add(chaser);
        chaser.InitialiseChaser(Cells[0,1,0], this, mesh, material, Random.Range(_chaserSpeeds.Item1, _chaserSpeeds.Item2));
        chaser.Pathfinder.SetPath(chaser.CurrentCell.Position, _playerLastCell.Position, chaser,  PuzzleSet.MouseMaze);
    }

    Door_Base SpawnDoor(Cell_MouseMaze cell)
    {
        GameObject doorGO = new GameObject($"Door_{_doors.Count}");
        doorGO.transform.parent = _doorParent;
        Door_Base door = doorGO.AddComponent<Door_Base>();
        door.InitialiseDoor(MouseMazeColour.Blue, Color.blue, cell);
        _doors.Add(door, true);
        return door;
    }

    void SpawnKey(Door_Base door)
    {
        GameObject keyGO = new GameObject($"Key_{_doors.Count}");
        keyGO.transform.parent = door.transform;

        keyGO.transform.localPosition = Cells[
            Random.Range(_keysMinimumDistance, _width), 
            Random.Range(_keysMinimumDistance, _height), 
            Random.Range(_keysMinimumDistance, _depth)
            ].transform.position;

        Door_Key key = keyGO.AddComponent<Door_Key>();
        key.InitialiseDoorKey(door.MouseMazeDoorColour, door.DoorColor);
        _keys.Add(key, false);
    }

    public void RefreshMaze(Cell_MouseMaze playerCell)
    {
        if (!Initialised) return;

        _playerLastCell = playerCell;

        for (int wid = 0; wid < _width; wid++)
        {
            for (int hei = 0; hei < _height; hei++)
            {
                for (int dep = 0; dep < _depth; dep++)
                {
                    if (Cells[wid, hei, dep] == null)
                    {
                        Debug.Log($"{wid}_{hei}_{dep}");
                        break;
                    }

                    if (Mathf.Abs(playerCell.Position.x - wid) + Mathf.Abs(playerCell.Position.y - hei) + Mathf.Abs(playerCell.Position.z - dep) <= _visibility)
                    {
                        Cells[wid, hei, dep].Show();
                    }

                    else Cells[wid, hei, dep].Hide();
                }
            }
        }

        if (_chasers != null && _chasers.Count > 0) UpdateChaserPaths();
    }

    void BreakWall()
    {
        if (_wallBreaks <= 0) { return; }

        Cell_MouseMaze closestCell = null; float minDistance = float.MaxValue;

        CheckNeighbor((int)_playerLastCell.Position.x + 1, (int)_playerLastCell.Position.y, (int)_playerLastCell.Position.z);
        CheckNeighbor((int)_playerLastCell.Position.x - 1, (int)_playerLastCell.Position.y, (int)_playerLastCell.Position.z);
        CheckNeighbor((int)_playerLastCell.Position.x, (int)_playerLastCell.Position.y, (int)_playerLastCell.Position.z + 1);
        CheckNeighbor((int)_playerLastCell.Position.x, (int)_playerLastCell.Position.y, (int)_playerLastCell.Position.z - 1);

        void CheckNeighbor(int neighborWid, int neighborHei, int neighbourDep)
        {
            if (neighborWid < 0 || neighborWid >= _width || neighborHei < 0 || neighborHei >= _height) return;

            Cell_MouseMaze cell = Cells[neighborWid, neighborHei, neighbourDep];

            float distance = Vector3.Distance(cell.transform.position, _player.transform.position);

            if (distance > minDistance) return;

            minDistance = distance;

            closestCell = Cells[neighborWid, neighborHei, neighbourDep];
        }

        if (!ClearWalls(_playerLastCell, closestCell).Item1)
        {
            Debug.LogError($"Could not clear walls between Player: {_playerLastCell.Position} and ClosestCell: {closestCell.Position}");
            return;
        }

        _wallBreaks--;

        if (_mousemazeTypes.Contains(MazeType.Chase)) UpdateChaserPaths();
    }

    public void UpdateChaserPaths(Chaser singleChaser = null)
    {
        if (_chasers.Count == 0) return;

        if (singleChaser == null)
        {
            foreach (Chaser chaser in _chasers)
            {
                chaser.Pathfinder.SetPath(chaser.CurrentCell.Position, _playerLastCell.Position, chaser, PuzzleSet.MouseMaze);
            }
        }
        else
        {
            singleChaser.Pathfinder.UpdatePath(singleChaser, singleChaser.CurrentCell.Position, _playerLastCell.Position);
        }
    }
    public void GetNewRoute(Chaser chaser)
    {
        chaser.Pathfinder.SetPath(chaser.CurrentCell.Position, _playerLastCell.Position, chaser, PuzzleSet.MouseMaze);
    }

    void OnDestroy()
    {
        _player.OnBreakWall -= BreakWall;
    }

    public void CollectableCollected(Collectable_MouseMaze collectable)
    {
        if (!_collectables.ContainsKey(collectable)) return;

        _collectables[collectable] = true;
        collectable.gameObject.SetActive(false);

        if (_collectables.Count > 0) return;

        // Collectible objective completed.
    }

    public Cell_MouseMaze GetCell(int wid, int hei, int dep)
    {
        if (wid >= 0 && wid < Cells.GetLength(0) && hei >= 0 && hei < Cells.GetLength(1)) return Cells[wid, hei, dep];
        
        return null;
    }

    public List<Vector3> GetAllObstacles(Vector3 moverPosition)
    {
        List<Vector3> obstaclePositions = new();

        GetObstacles(moverPosition, "Wall", obstaclePositions);
        GetObstacles(moverPosition, "Water", obstaclePositions);

        return obstaclePositions;
    }

    void GetObstacles(Vector3 moverPosition, string layerName, List<Vector3> obstaclePositions)
    {
        int layerMask = 1 << LayerMask.NameToLayer(layerName);
        Collider[] colliders = Physics.OverlapSphere(moverPosition, 10, layerMask);

        foreach (Collider collider in colliders)
        {
            obstaclePositions.Add(collider.transform.position);
        }
    }
}
