using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemSlot
{
	public BlockID	itemID;
	public Image	icon;
}

public class	Hotbar : MonoBehaviour
{
	World 					world;
	public Player			player;

	public RectTransform	highlight;
	public ItemSlot[]		slotMap;	//itemSlots

	int						slotID = 0;	//slotIndex

	private void	Start()
	{
		world = GameObject.Find("World").GetComponent<World>();

		foreach (ItemSlot slot in slotMap)
		{
			slot.icon.sprite = world.blocktypes[(int)slot.itemID].icon;
			slot.icon.enabled = true;										//doesn't work, enabled them by default
		}
		//SetSelectedSlot(0);											//hardcoded in player because broken
	}

	private void	Update()
	{
		float	scroll = Input.GetAxis("Mouse ScrollWheel");

		if (scroll != 0)
		{
			if (scroll < 0)
				slotID++;
			else
				slotID--;

			if (slotID < 0)
				slotID = 7;	//slotMap.Lenght - 1;
			if (slotID > 7)	//slotMap.Lenght - 1)
				slotID = 0;

			SetSelectedSlot(slotID);
		}
	}

	private void	SetSelectedSlot(int value)
	{
		slotID = value;
		highlight.position = slotMap[slotID].icon.transform.position;
		player.UpdateSelectedBlockID(slotMap[slotID].itemID);
	}
}