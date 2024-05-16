using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum IceWallType { None, Mirror, Stamina, Shatter }
public class Spawner_IceWall : MonoBehaviour
{
    List<IceWallType> _icewallTypes;

    // Mirror chessboard, on one side is the floor where you can see where you can go, and on the other board, you can see the holes in the floor that you have to avoid, but
    // because you are looking at it from underneath, it's mirrorred, and so you have to do it the opposite direcition.

    public Cell_IceWall[,,] Cells { get; private set; }
    Transform _cellParent;
    //int _cellCount = 0;
    Cell_IceWall _playerLastCell;

    int _width;
    int _height;
    int _depth;
    int _scale;

    int _maxDistance = 0;
    Cell_IceWall _furthestCell;

    int _subVoxelScale = 10;

    Vector3 _startPosition;

    Controller_Puzzle_IceWall _player;

    (int, int) _cellHealthRange;
    int _maxCellHealth;

    int _playerExtraStamina;

    bool _disableRefresh = false;

    void Start()
    {
        //List<IceWallType> gameModes = new List<IceWallType>();

        //gameModes.Add(IceWallType.Stamina);
        //gameModes.Add(IceWallType.Shatter);
        //gameModes.Add(IceWallType.Mirror);

        //InitialisePuzzle(gameModes);
    }

    public void InitialisePuzzle(List<IceWallType> gameModes, IceWallData iceWallData)
    {
        Controller_Camera.Instance.SetOffset(new Vector3(0, 30, 0), Quaternion.Euler(90, 0, 0));
        Controller_Sun.Instance.SetLightPositionAndRotation(new Vector3(0, 30, 0), Quaternion.Euler(90, 0, 0));

        _width = iceWallData.Width;
        _height = iceWallData.Height;
        _depth = iceWallData.Depth;

        _startPosition = iceWallData.StartPosition;
        _playerExtraStamina = iceWallData.PlayerExtraStaminaPercentage;
        _cellHealthRange = (iceWallData.CellHealthMin, iceWallData.CellHealthMax);

        _cellParent = GameObject.Find("CellParent").transform;
        _player = GameObject.Find("Focus").GetComponent<Controller_Puzzle_IceWall>();
        _player.Initialise(this, _playerExtraStamina);

        VoxelGrid.InitializeVoxelGrid(_width, _height, _depth, _subVoxelScale, new Vector3(0.5f, 0, 0.5f));

        _icewallTypes = new List<IceWallType>(gameModes);
        if (Manager_Puzzle.Instance.Puzzle.PuzzleData.PuzzleState.PuzzleType == PuzzleType.Fixed) SpawnFixedPuzzle();
        else SpawnRandomPuzzle();
    }

    void SpawnFixedPuzzle()
    {

    }

    void SpawnRandomPuzzle()
    {
        Cells = new Cell_IceWall[_width, _height, _depth];

        for (int wid = 0; wid < _width; wid++)
        {
            for (int hei = 0; hei < _height; hei++)
            {
                for (int dep = 0; dep < _depth; dep++)
                {
                    Cells[wid, hei, dep] = CreateCell(wid, hei, dep);

                    if ((wid + hei + dep) > _maxDistance)
                    {
                        _maxDistance = wid + hei + dep;
                        _furthestCell = Cells[wid, hei, dep];
                    }
                }
            }
        }

        _playerLastCell = Cells[0, 0, 0];
        _player.SetCurrentCell(_playerLastCell);
        _startPosition = _playerLastCell.Position;

        //if (_icewallTypes.Contains(IceWallType.Shatter))
        //{
        foreach (Cell_IceWall cell in Cells)
            {
                cell.ChangeColour((float)cell.CellHealth / _maxCellHealth);
            }
        //}
        
        if (_icewallTypes.Contains(IceWallType.Stamina))
        {
            _furthestCell.StaminaFinishCell();
            _calculatePlayerMaxStamina();
        }
    }

    Cell_IceWall CreateCell(int width, int height, int depth)
    {
        GameObject cellGO = new GameObject($"cell{width}_{height}_{depth}");
        cellGO.transform.position = new Vector3(width, height, depth);
        cellGO.transform.rotation = Quaternion.identity;
        cellGO.transform.parent = _cellParent;
        Cell_IceWall cell = cellGO.AddComponent<Cell_IceWall>();
        int cellHealth = Random.Range(_cellHealthRange.Item1, _cellHealthRange.Item2);
        if (cellHealth > _maxCellHealth) _maxCellHealth = cellHealth;
        VoxelGrid.GetVoxelAtPosition(new Vector3(width, height, depth)).UpdateMovementCost(cellHealth);
        cell.InitialiseCell(new Vector3(width, height, depth), this, cellHealth);

        return cell;
    }

    void _calculatePlayerMaxStamina()
    {
        _player.Pathfinder.SetPath(_playerLastCell.Position, _furthestCell.Position, _player, PuzzleSet.IceWall);
    }

    void Update()
    {
        OnStayInCell(_playerLastCell);
    }

    public void OnStayInCell(Cell_IceWall cell)
    {
        if (_icewallTypes.Contains(IceWallType.Mirror))
        {
            // move mirror
        }
        if (_icewallTypes.Contains(IceWallType.Stamina))
        {
            if (!_player.DecreaseStamina(cell))
            {
                _player.Fall();

                // Show an animation first.

                // _player.transform.position = Cells[_startPosition.x, _startPosition.y, _startPosition.z].transform.position;
            }
        }
        if (_icewallTypes.Contains(IceWallType.Shatter))
        {
            if (!cell.DecreaseHealth(_maxCellHealth))
            {
                cell.Break();
            }
        }
    }

    public bool PlayerCanMove(IceWallDirection direction)
    {
        switch (direction)
        {
            case IceWallDirection.Up: if (_playerLastCell.Position.x + 1 < _width && !Cells[(int)_playerLastCell.Position.x + 1, 0, (int)_playerLastCell.Position.z].Broken) return true; break;
            case IceWallDirection.Down: if (_playerLastCell.Position.x - 1 >= 0 && !Cells[(int)_playerLastCell.Position.x - 1, 0, (int)_playerLastCell.Position.z].Broken) return true; break;
            case IceWallDirection.Left: if (_playerLastCell.Position.z - 1 >= 0 && !Cells[(int)_playerLastCell.Position.x, 0, (int)_playerLastCell.Position.z - 1].Broken) return true; break;
            case IceWallDirection.Right: if (_playerLastCell.Position.z + 1 < _depth && !Cells[(int)_playerLastCell.Position.x, 0, (int)_playerLastCell.Position.z + 1].Broken) return true; break;
        }

        return false;
    }

    public void RefreshWall(Cell_IceWall cell)
    {
        _player.SetCurrentCell(cell);
        _playerLastCell = cell;
    }
}
