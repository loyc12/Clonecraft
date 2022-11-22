using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
	[SerializeField]
	HexCell[] neighbors;
    public int neighborcount;	// for debbuging
    public HexCoordinates coordinates;
    public int index;
	public Color color;
	int elevation;
	public RectTransform uiRect;

	public HexCell GetNeighbor (HexDirection direction)
    {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell)
    {
		neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
	}

	public int Elevation {
		get {
			return elevation;
		}
		set {
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = elevation * -HexMetrics.elevationStep;
			uiRect.localPosition = uiPosition;
		}
	}

	public HexEdge GetEdgeType (HexDirection direction) {
		return HexEdgeExtensions.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);
	}

	public HexEdge GetEdgeType (HexCell otherCell) {
		return HexEdgeExtensions.GetEdgeType(
			elevation, otherCell.elevation
		);
	}
}
