using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EverydayDevup.Framework;

public class MyUnits : MonoBehaviour
{
    const int playerIndex = 6;

    public static void SetColliderEnabled(bool value)
    {
        foreach (GameObject unit in Game.Instance.dataManager.lobbyUnits)
        {
            if (unit)
            {
                unit.GetComponent<BoxCollider2D>().enabled = value;
            }
        }
    }
    public static void SetMyDeckUnitPrafab()
    {
        Game.Instance.dataManager.HeroePrefabs.Clear();
        Game.Instance.dataManager.SoldierPrefabs.Clear();
        Game.Instance.dataManager.HeroePrefabs.Add(Resources.Load<GameObject>("Prefabs/01Heroes/" + LunaDataTable.Instance.playerData.UnitNamesInDeck[0]));
        Game.Instance.dataManager.HeroePrefabs.Add(Resources.Load<GameObject>("Prefabs/01Heroes/" + LunaDataTable.Instance.playerData.UnitNamesInDeck[1]));
        Game.Instance.dataManager.HeroePrefabs.Add(Resources.Load<GameObject>("Prefabs/01Heroes/" + LunaDataTable.Instance.playerData.UnitNamesInDeck[2]));
        Game.Instance.dataManager.HeroePrefabs.Add(Resources.Load<GameObject>("Prefabs/01Heroes/" + LunaDataTable.Instance.playerData.UnitNamesInDeck[3]));
        Game.Instance.dataManager.SoldierPrefabs.Add(Resources.Load<GameObject>("Prefabs/02Soldiers/" + LunaDataTable.Instance.playerData.UnitNamesInDeck[4]));
        Game.Instance.dataManager.SoldierPrefabs.Add(Resources.Load<GameObject>("Prefabs/02Soldiers/" + LunaDataTable.Instance.playerData.UnitNamesInDeck[5]));
    }
    public static void InstantiateLobby()
    {
        int idx = 0;
        foreach (GameObject unit in Game.Instance.dataManager.lobbyUnits)
        {
            Destroy(unit);
            Game.Instance.dataManager.lobbyUnits[idx] = null;
        }

        LunaDataTable.Instance.playerData.SetCurPlayer();
        GameObject luna = Instantiate(LunaDataTable.Instance.playerData.CurPlayer());
        Game.Instance.dataManager.lobbyUnits[playerIndex] = luna;
        Game.Instance.dataManager.LunaPlayer = luna;

        for (int i = 0; i < Defined.MaxHeroCnt; i++)
        {
            if (Game.Instance.dataManager.HeroePrefabs.Count == 0)
                break;

            if (Game.Instance.dataManager.HeroePrefabs[i] != null)
                Game.Instance.dataManager.lobbyUnits[i] 
                    = Instantiate(Game.Instance.dataManager.HeroePrefabs[i]);
        }

        for (int i = Defined.MaxHeroCnt; i < Defined.MaxUnitCnt; i++)
        {
            if (Game.Instance.dataManager.SoldierPrefabs.Count == 0)
                break;

            if (Game.Instance.dataManager.SoldierPrefabs[i - Defined.MaxHeroCnt] != null)
                Game.Instance.dataManager.lobbyUnits[i] 
                    = Instantiate(Game.Instance.dataManager.SoldierPrefabs[i - Defined.MaxHeroCnt]);
        }

        for (int i = 0; i < 7; i++)
        {
            if (Game.Instance.dataManager.lobbyUnits[i] != null)
            {
                Status status = Game.Instance.dataManager.lobbyUnits[i].GetComponent<Status>();
                status.unitState.IsLobbyMove = true;

                Game.Instance.dataManager.lobbyUnits[i].GetComponent<BoxCollider2D>().enabled = true;

                if (i != 6)
                    Game.Instance.dataManager.lobbyUnits[i].transform.position 
                        = new Vector3(i - 5, 0.1f, 0);
                else
                    Game.Instance.dataManager.lobbyUnits[i].transform.position 
                        = new Vector3(1.5f, 0.1f, 0);

            }
        }
    }




}
