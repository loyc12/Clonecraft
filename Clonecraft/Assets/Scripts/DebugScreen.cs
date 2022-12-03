using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{
	World	world;
	Text	debugScreenText;

	float	frameRate;
	float	updateTimer;

    void Start()
    {
		world = GameObject.Find("World").GetComponent<World>();
		debugScreenText = GetComponent<Text>();
    }

    void Update()
    {
        string debugText = "Debug Informations";

		debugText += "\n" + frameRate + " fps";

		debugText += "\n\n" + "Block Coords";
		debugText += "\n X " + world.player.transform.position.x;
		debugText += "\n Y " + world.player.transform.position.y;
		debugText += "\n Z " + world.player.transform.position.z;

		debugText += "\n\n" + "Chunk Coords";
		debugText += "\n X " + world.playerChunk.x;
		debugText += "\n Y " + world.playerChunk.y;
		debugText += "\n Z " + world.playerChunk.z;


		if (updateTimer > 1f)
		{
			frameRate = (int)(1f / Time.unscaledDeltaTime);
			updateTimer = 0f;
		}
		else
			updateTimer += Time.deltaTime;

		debugScreenText.text = debugText;
    }
}
