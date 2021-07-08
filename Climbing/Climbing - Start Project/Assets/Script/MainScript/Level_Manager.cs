using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level_Manager : MonoBehaviour
{
    public int spawnPos;
    public Transform spawnHolder;
    public Transform[] spawnPoints;

    public GameObject playerCharacterPrefab;
    public GameObject cameraPrefab;

    GameObject playerGo;
    public bool dummy;
    EnemyPlayer ep;
    IEnumerator Start()
    {
        ep = GetComponent<EnemyPlayer>();
        if (!dummy)
        {
            yield return InitializePlayer();
        }
    }

    IEnumerator InitializePlayer()
    {
        spawnPoints = spawnHolder.GetComponentsInChildren<Transform>();

        playerGo = Instantiate(playerCharacterPrefab,
            spawnPoints[spawnPos].position,
            spawnPoints[spawnPos].rotation) as GameObject;

        Instantiate(cameraPrefab, spawnPoints[spawnPos].position, Quaternion.identity);

        FreeCameraLook.GetInstance().target = playerGo.transform;

        float startingAngle = Vector3.Angle(Vector3.forward, spawnPoints[spawnPos].forward);
        FreeCameraLook.GetInstance().lookAngle = startingAngle;
        //ep.Scanner_OnScanReady();
        

        yield return null;
    }
}
