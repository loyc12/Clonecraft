using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*	IDEA

	add recent max coord values

*/

public class DebugScreen : MonoBehaviour
{
	World	world;
	Player	player;
	Text	debugScreenText;

	float	frameRate;
	float	updateTimer;

    void Start()
    {
		world = GameObject.Find("World").GetComponent<World>();
		player = GameObject.Find("Player").GetComponent<Player>();
		debugScreenText = GetComponent<Text>();
    }

    void Update()
    {
        string debugText = "Debug Informations";

		debugText += "\n" + frameRate + " fps";

		debugText += "\n\n" + "Block Coords";
		debugText += "\n X " + player.transform.position.x;
		debugText += "\n Y " + player.transform.position.y;
		debugText += "\n Z " + player.transform.position.z;

		debugText += "\n\n" + "Relative Coords";
		debugText += "\n X " + (player.transform.position.x / WorldData.WorldBlockSize);
		debugText += "\n Y " + (player.transform.position.y / WorldData.WorldBlockHeight);
		debugText += "\n Z " + (player.transform.position.z / WorldData.WorldBlockSize);

		debugText += "\n\n" + "Chunk Coords";
		debugText += "\n X " + world.playerChunk.x;
		debugText += "\n Y " + world.playerChunk.y;
		debugText += "\n Z " + world.playerChunk.z;

		debugText += "\n\n" + "Camera Rotation";
		debugText += "\n H " + player.yRotation;
		debugText += "\n V " + player.xRotation;

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
