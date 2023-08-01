using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpotFunctions : PlayerFunctions
{
    public int curPlayer;   
    public int dieRoll;
    public bool riggedDice;
    public int riggedDie1 = 1;
    public int riggedDie2 = 1;
    

    private void Start()
    {
        gm = GameManager.gm;
        players = gm.playerFunctions.players;
    }

    public void AdvancePlayer()
    {
        curPlayer++;
        if (curPlayer >= gm.numPlayers)
        {
            curPlayer = 0;
        }
    }
    public void RollDice()
    {
        gm.board.RollDice();
        StartCoroutine(WaitForDiceResult());
    }
    void EvaluateRollDice(int _die1, int _die2)
    {
        
        if (players[curPlayer].wasMovedByCard) // skip the actual movement part since we just need the dieRoll to figure out Utility rent
        {
            dieRoll = _die1 + _die2;
            return;
        }

        players[curPlayer].areDoublesRolled = false;
        int dieRoll1 = _die1;
        int dieRoll2 = _die2;

        if (riggedDice)
        {
            dieRoll1 = riggedDie1;
            dieRoll2 = riggedDie2;
        }
        if (players[curPlayer].inJail && dieRoll1 != dieRoll2)
        {
            return;
        }
        Debug.Log("Roll = " + dieRoll1 + dieRoll2);
        // Check for doubles
        if ((dieRoll1 == dieRoll2) && !players[curPlayer].inJail)
        {
            // Doubles rolled
            players[curPlayer].amtDoublesRolled++;
            if (players[curPlayer].amtDoublesRolled > 2)  // Break out of this method and go to jail
            {    
                players[curPlayer].GoToJail();
                Debug.Log("Go To Jail - doubles rolled");
                return;
            }
            else
            {
                players[curPlayer].areDoublesRolled = true;
                gm.audioManager.PlaySound(gm.board.die[0].audioGetLucky);
            }
        }
        else if ((dieRoll1 == dieRoll2) && gm.players[curPlayer].inJail)
        {
            players[curPlayer].inJail = false;
        }

        
        
        int spacesToMove = dieRoll1 + dieRoll2;        
        int newPos = (int)players[curPlayer].playerPos + spacesToMove;
        GoToSpot((ePos)newPos, false);
    }
    IEnumerator WaitForDiceResult()
    {
        int die1 = -1;
        int die2 = -2;

        while (die1 <= 0 || die2 <= 0)
        {
            die1 = gm.board.die[0].diceValue;
            die2 = gm.board.die[1].diceValue;
            yield return new WaitForSeconds(.1f);
        }
        Debug.Log("Die 1 " + die1 + "Die 2 " + die2);
        EvaluateRollDice(die1, die2);
    }

    public void GoToSpot(ePos _spot, bool _isJail)
    {
        ePos startingPos = players[curPlayer].playerPos;
        int newPos = (int)_spot;
        bool isPassGo = false;

        if (newPos > (int)ePos.boardwalk)
        {
            isPassGo = true;
            newPos -= 40;
            if (!_isJail) players[curPlayer].AdjustCash(200);
            
        }
        players[curPlayer].playerPos = (ePos)newPos; //set new position of player
        gm.board.MoveCamera(eCam.mid, players[curPlayer].pieceObj);
        //TODO: Add animation
        StartCoroutine(MoveToSpot(startingPos, (ePos)newPos, isPassGo));
    }

    IEnumerator MoveToSpot(ePos _curPos, ePos _finalPos, bool _passGo)
    {
        // Setup
        int steps = (int)_finalPos - (int)_curPos;

        // Do we pass Go?
        if (_passGo) steps += 40;
        int counter = 0;
        int nextPos = (int)_curPos++;

        if (nextPos == 40) nextPos = 0;

        Transform pieceObj = players[curPlayer].pieceObj;
        // Loop
        while (steps >= counter) // Test for whether we reached our spot
        {
            Vector3 currentPos = pieceObj.position;
            Vector3 newPos = gm.board.spots[nextPos].transform.position;
            Vector3 o = new Vector3(offset[curPlayer].x, 0, offset[curPlayer].y);
            newPos = newPos + o;
            Quaternion newRot = gm.board.spots[nextPos].transform.rotation;

            float elapsedTime = 0f;
            float waitTime = .25f;

            while (elapsedTime < waitTime) // Moves piece a little bit at a time between spots
            {
                pieceObj.transform.position = Vector3.Lerp(currentPos, newPos, (elapsedTime / waitTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            yield return null;

            pieceObj.transform.rotation = newRot;
            nextPos++;
            if (nextPos == 40) nextPos = 0;
            counter++;
        }
        LandOnSpot(_finalPos);
    }

    public void LandOnSpot(ePos _curSpot)
    {
        
        soSpot so_Spot = gm.board.spots[(int)_curSpot].so_Spot;
        GameManager.gm.audioManager.PlaySound(so_Spot.audioLandOn);
        if (!(so_Spot.typeSpot == eTypeSpot.chance) && !(so_Spot.typeSpot == eTypeSpot.commChest))
        {
            gm.canvasManager.canvasHUD.ShowSpotInfo(so_Spot);
        }
        else gm.canvasManager.canvasHUD.ShowCardInfo(so_Spot);
        if (!players[curPlayer].areDoublesRolled) // Checking whether the player will be able to Go Again after they've rolled and moved
        {
            gm.canvasManager.canvasHUD.bottomButtons[1].gameObject.SetActive(false); // Disables the Roll button
            gm.canvasManager.canvasHUD.bottomButtons[0].gameObject.SetActive(true); // Enables the End Turn button
            
        }
        
    }
}
