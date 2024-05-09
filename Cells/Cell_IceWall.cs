using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Cell_IceWall : Cell_Base
{
    Spawner_IceWall _spawner;
    public TextMeshPro CellText;
    public bool Broken { get; private set; }
    public int CellHealth { get; private set; }
    bool _onCooldown = false;
    Coroutine _autoBreak;

    bool _staminaFinishCell = false;

    public void InitialiseCell(Vector3Int position, Spawner_IceWall spawner, int cellHealth)
    {
        Position = position;
        _spawner = spawner;
        CellHealth = cellHealth;

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = Resources.Load<Material>("Meshes/Material_White");

        _boxCollider = gameObject.AddComponent<BoxCollider>();
        _boxCollider.size = new Vector2(0.75f, 0.75f);
        _boxCollider.isTrigger = true;

        GameObject textGO = new GameObject();
        textGO.transform.parent = transform;
        textGO.transform.localPosition = Vector3.zero;
        CellText = textGO.AddComponent<TextMeshPro>();
        CellText.text = CellHealth.ToString();
        CellText.alignment = TextAlignmentOptions.Center;
        CellText.sortingLayerID = -967159649;
        CellText.fontSize = 2; 
        CellText.color = Color.red;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name == "Focus") 
        { 
            _spawner.RefreshWall(this);

            if (_staminaFinishCell) Manager_Puzzle.Instance.PuzzleEnd(true);
        }
    }

    public bool DecreaseHealth(int maxHealth)
    {
        if (CellHealth == 0) return false;
        if (_onCooldown) return true;

        _onCooldown = true;
        CellHealth --;
        CellText.text = $"{CellHealth}->{(CellHealth / maxHealth).ToString("F2")}";
        ChangeColour((float)CellHealth / maxHealth);
        if (CellHealth == 1) _autoBreak = StartCoroutine(AutoBreak());
        
        StartCoroutine(_healthCooldown());
        return true;
    }

    public void Break()
    {
        if (_autoBreak != null) StopCoroutine(_autoBreak);
        Broken = true;
        CellText.text = "";
    }

    IEnumerator AutoBreak()
    {
        ChangeColour(1);
        List<Material> cracks = new();

        cracks.Add(Resources.Load<Material>("Sprites/Grid"));
        cracks.Add(Resources.Load<Material>("Sprites/Grid_OpenOneSide"));
        cracks.Add(Resources.Load<Material>("Sprites/Grid_OpenTwoSides"));
        cracks.Add(Resources.Load<Material>("Sprites/Grid_OpenThreeSides"));
        cracks.Add(Resources.Load<Material>("Sprites/Grid_OpenAllSides"));

        foreach (Material material in cracks)
        {
            _meshRenderer.material = material;
            yield return new WaitForSeconds(1f);
        }

        _meshRenderer.material = Resources.Load<Material>("Meshes/Material_Black");

        Break();
    }

    public void ChangeColour(float colourScale)
    {
        _meshRenderer.material = Resources.Load<Material>("Meshes/Material_Test");
        _meshRenderer.material.color = new Color(colourScale, colourScale, colourScale);
    }

    IEnumerator _healthCooldown()
    {
        yield return new WaitForSeconds(1);
        _onCooldown = false;
    }

    public void StaminaFinishCell()
    {
        MarkCell(Resources.Load<Material>("Meshes/Material_Red"));
        _staminaFinishCell = true;
    }
}
