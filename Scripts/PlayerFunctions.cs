using System.Collections.Generic;
using UnityEngine;

public class PlayerFunctions : MonoBehaviour
{
    //public Player[] players;
    public List<Player> players;
    protected GameManager gm;
    protected Vector2[] offset = {new Vector2(0,0), new Vector2(0, 2), new Vector2(2, -2), new Vector2(2,0) };
    
    public  void CreatePlayers() // This is the "stock" list of players created at startup. It will always start with 4 players for the purposes of choosing pieces/player types in Setup.
                                 // SpawnPlayers will run at actual game start and adjust the player List based on how many players are actually playing
    {   
        gm = GameManager.gm;

        if (players != null && players.Count > 0)
        {
            return;
        }
        //players = new Player[4];
        players = new List<Player>();
        for (int i = 0; i < 4; i++)
        {
            GameObject go = new GameObject("Player " + (i + 1));
            go.transform.SetParent(this.transform);
            players.Add(go.AddComponent<Player>());
            players[i].playerName = "PLAYER " + (i + 1);
            players[i].playerColor = gm.so_Rep.playerColors[i];
            players[i].playerPiece = gm.so_Rep.pieces[i];
            players[i].cashOnHand = gm.set.startingCash;
            players[i].playerIndex = i;
        }
        players[0].playerType = ePlayerType.human;
        players[1].playerType = ePlayerType.AI;
    }

    public bool CheckForDupePiece (int _playerIdx) // Ensures two players can't be the same piece
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i != _playerIdx && players[i].playerType != ePlayerType.none)
            {
                if (players[_playerIdx].playerPiece == players[i].playerPiece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SpawnPlayers() // Instantiates the players at Go
    {
        gm = GameManager.gm;
        players.RemoveAll(item => item.playerType == ePlayerType.none); // Removes all non-existant players from the game before we spawn
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerType != ePlayerType.none)
            {
                Transform t = gm.board.spots[(int)ePos.go].transform;
                Vector3 o = new Vector3(offset[i].x, 0, offset[i].y);
                players[i].offset = o;
                players[i].pieceObj = Instantiate(players[i].playerPiece.pPiece, t.position + o, t.rotation).transform;
            }
        }
    }
    
    public Player GetPropertyOwner(soSpot _soSpot) // Used for determing rent payouts (among other things)
    {
        foreach (Player p in players)
        {
            if (p.IsPropertyOwned(_soSpot))
            {
                return p;
            }
        }
        return null;
    }

}
