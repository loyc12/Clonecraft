using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
	public int width = 12;
	public int height = 8;

	public HexCell cellPrefab;

	HexCell[] cells;

    public Text cellLabelPrefab;

	Canvas gridCanvas;

	HexMesh hexMesh;

    public Color defaultColor = Color.white;

	void Awake ()
    {
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();
		cells = new HexCell[height * width];

		for (int z = 0, i = 0; z < height; z++) {
			for (int x = 0; x < width; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

    void Start ()
    {
		hexMesh.TriangulateCells(cells);
	}

	public void Refresh ()
	{
		hexMesh.TriangulateCells(cells);
	}
	
	void CreateCell (int x, int z, int i)
    {
		Vector3 position;
		position.x = x * (HexMetrics.outerRadius * 1.5f);
		position.y = 0f;
		position.z = (z + (x * 0.5f) - (x / 2)) * (HexMetrics.innerRadius * 2f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, false);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.index = i;
        cell.color = defaultColor;
		cell.neighborcount = 0;

		if (z > 0)
		{
			cell.SetNeighbor(HexDirection.S, cells[i - width]);
			cell.neighborcount += 1;
			cells[i - width].neighborcount += 1;
			if (x % 2 == 0)
			{
				if (x < width - 1)
				{
					cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
					cell.neighborcount += 1;
					cells[i - width + 1].neighborcount += 1;
				}
				if (x > 0)
				{
					cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
					cell.neighborcount += 1;
					cells[i - width - 1].neighborcount += 1;
				}
			}
		}

		if (x > 0)
		{
			if (x % 2 == 0)
			{
				cell.SetNeighbor(HexDirection.NW, cells[i - 1]);
				cell.neighborcount += 1;
				cells[i - 1].neighborcount += 1;
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, cells[i - 1]);
				cell.neighborcount += 1;
				cells[i - 1].neighborcount += 1;
			}
		}

		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;
	}
	
	public HexCell GetCell (Vector3 position)
    {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        int index = coordinates.X + ((coordinates.Z + (coordinates.X / 2)) * width);
		return cells[index];
	}
}