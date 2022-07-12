using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class RpcController : NetworkBehaviour
{
	public static RpcController rpcc;
	public List<MobilePlayerController> allMPCs = new List<MobilePlayerController>();
	public List<ColorSelect> colorBoxes = new List<ColorSelect>();

	void Start ()
    {
		rpcc = this;
        DontDestroyOnLoad(this);
	}

	void Update()
	{
		if (allMPCs.Count < 1 && SceneManager.GetActiveScene().name == "NewGameUI")
			allMPCs = new List<MobilePlayerController>(FindObjectsOfType<MobilePlayerController>());

		if (colorBoxes.Count < 1 && SceneManager.GetActiveScene().name == "NewGameUI")
		{
			colorBoxes.Add(GameObject.Find("PlayerOneSpawnPos").GetComponentInChildren<ColorSelect>());
			colorBoxes.Add(GameObject.Find("PlayerTwoSpawnPos").GetComponentInChildren<ColorSelect>());
			colorBoxes.Add(GameObject.Find("PlayerThreeSpawnPos").GetComponentInChildren<ColorSelect>());
			colorBoxes.Add(GameObject.Find("PlayerFourSpawnPos").GetComponentInChildren<ColorSelect>());
		}
	}

	public void AddLocalColor(PlayerColors pc, int pos)
	{
		allMPCs[pos].playerColor = pc;
		allMPCs[pos].myPosInt = pos;
		allMPCs[pos].isReady = true;
	}

	public void RemoveLocalColor(int pos)
	{
		allMPCs[pos].playerColor = PlayerColors.None;
		allMPCs[pos].myPosInt = -1;
		allMPCs[pos].isReady = false;
	}

    [ClientRpc]
    public void RpcAddColor(int c, int pos)
    {
        for (int i = 0; i < allMPCs.Count; i++)
		{
			allMPCs[i].possibleColors.Add((PlayerColors)c);
			allMPCs[i].possiblePositions.Add(pos);
		}
    }

    [ClientRpc]
    public void RpcRemoveColor(int c, int pos, bool unready)
    {
		for (int i = 0; i < allMPCs.Count; i++)
		{
			if (allMPCs[i].isReady && allMPCs[i].playerColor == (PlayerColors)c && unready)
			{
				allMPCs[i].CmdSetReady(false, -1, -1);
			}
			allMPCs[i].possibleColors.Remove((PlayerColors)c);
			allMPCs[i].possiblePositions.Remove(pos);
		}
    }

    [ClientRpc]
    public void RpcUpdateMyPlayer(int playerColor)
    {
        Debug.Log("Updating things on client");
        MobilePlayerController[] mpcs = FindObjectsOfType<MobilePlayerController>();

        MobilePlayerController myMPC = null;
        for(int i = 0; i < mpcs.Length; i++)
        {
            if (mpcs[i].playerColor == (PlayerColors)playerColor) myMPC = mpcs[i];
        }

        if (myMPC == null) return;
        Player p = new Player((PlayerColors)playerColor);
        myMPC.player = p;
    }

	public void ReadyUp(MobilePlayerController mpc, int c, int pos)
    {
		// MobilePlayerController tableMPC = allMPCs.Find(x => (int)(x.playerColor) == c && x.isOnTable);
		// if (tableMPC)
		// {
		// 	tableMPC.playerColor = PlayerColors.None;
		// 	tableMPC.myPosInt = -1;
		// }

		mpc.isReady = true;
		mpc.playerColor = (PlayerColors)c;
		mpc.myPosInt = pos;
        mpc.CmdSetReady(true, c, pos);
    }
}
