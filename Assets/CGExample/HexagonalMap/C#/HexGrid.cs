using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{

    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;

    HexCell[] cells;

    HexMesh hexMesh;

    public Text cellLablePrefab;
    Canvas gridCanvas;
  



    public Color defaultColor = Color.white;
    public Color touchedColor = Color.yellow;


  

    private void Awake()
    {

        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();
       

        cells = new HexCell[width * height];

        for (int z = 0 ,  i = 0 ; z < height; z++)
        {
            for (int x= 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void CreateCell(int x, int z, int i)
    {
        Vector3 pozition;
        pozition.x = (x + z * 0.5f - z/2) * (HexMetrics.innerRadius*2f);
        //Debug.Log("pozition.x=" + pozition.x);
        pozition.y = 0;
        pozition.z = z * (HexMetrics.outerRadius*1.5f);

        HexCell cell = cells[i] = Instantiate(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition  = pozition;
        cell.coordinatates = HexCoordinatates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        Text label = Instantiate<Text>(cellLablePrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(pozition.x, pozition.z);
        //label.text = x.ToString() + "\n" + z.ToString();
        label.text = cell.coordinatates.ToStringOnSepareteLines();
    }

    void Start()
    {
        hexMesh.Triangulate(cells);
    }


    public void ColorCell(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinatates coordinatates = HexCoordinatates.FromPosition(position);
        Debug.Log("touched at" + coordinatates.ToString());

        int index = coordinatates.X + coordinatates.Z * width + coordinatates.Z / 2;
        HexCell cell = cells[index];
        cell.color = color;
        hexMesh.Triangulate(cells);
    }

}
