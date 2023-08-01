using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;


public class CanvasHUD : MonoBehaviour
{
    GameManager gm;
    public Transform panelPlayerInfo;
    public Button[] bottomButtons;
    public WidgetPlayerHUD[] playerIcons;
   
    public void Start()
    {
        gm = GameManager.gm;
        ShowPlayerInfo();        
        
    }

    void ShowPlayerInfo()
    {
        playerIcons = new WidgetPlayerHUD[gm.numPlayers];
        for (int i = 0; i < gm.numPlayers; i++)
        {
            if (gm.playerFunctions.players[i].playerType != ePlayerType.none)
            {
                WidgetPlayerHUD widget = Instantiate(Resources.Load("Widgets/" + "Widget_PlayerHUD") as GameObject, panelPlayerInfo).GetComponent<WidgetPlayerHUD>(); 
                // Initializing the player icons at the top of the screen and putting them in an array so we can access their HiLites
                playerIcons[i] = widget; 
                widget.Init(i);
                widget.SetHilite(i == gm.spotFunctions.curPlayer);  // highlights first player
            }
           
        }
    }  

    public void HiLiteCurPlayer()
    {
        foreach (WidgetPlayerHUD p in playerIcons)
        {
            p.SetHilite(gm.spotFunctions.curPlayer == p.playerIdx);
        }
    }

    public void ShowSpotInfo(soSpot _soSpot)
    {
        WidgetPurchase widget = Instantiate(Resources.Load("Widgets/" + "Widget_Purchase") as GameObject, this.transform).GetComponent<WidgetPurchase>();
        widget.Init(_soSpot);
    }

    public void ShowCardInfo(soSpot _soSpot)
    {
        WidgetSpotCard widget = Instantiate(Resources.Load("Widgets/" + "Widget_SpotCard") as GameObject, this.transform).GetComponent<WidgetSpotCard>();
        widget.Init(_soSpot);
    }
    public void OnEndTurn()
    {
        Debug.Log("Ending Turn");        
        WidgetMessage widget = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
        widget.Init(this.gameObject, "HandleEndOfTurn", "No", "Yes");
       
    }

    public void OnRollPressed()
    {
        
        DisableButtons();
        gm.spotFunctions.RollDice();
        gm.PlaySound(eSounds.click);

    }
    public void OnQuitPressed()
    {
        gm.PlaySound(eSounds.click);
        WidgetMessage widget = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
        widget.Init(this.gameObject, "QuitGame", "No", "Yes");

    }
    public void OnManagePressed()
    {
        DisableButtons();
        if (gm.playerFunctions.players[gm.spotFunctions.curPlayer].listPropertiesOwned.Count == 0)
        {
            WidgetMessage w = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
            w.Init("Ok", "", "You are without properties!");
            return;
        }
        soSpot so_Spot = gm.playerFunctions.players[gm.spotFunctions.curPlayer].listPropertiesOwned[0].so_Spot;
        WidgetSpotBase widget = Instantiate(Resources.Load("Widgets/" + "Widget_Manage") as GameObject, this.transform).GetComponent<WidgetSpotBase>();
        widget.Init(so_Spot);
        gm.PlaySound(eSounds.click);

    }
    public void OnLogPressed()
    {
        gm.PlaySound(eSounds.click);
        ShowSpotInfo(gm.board.spots[(int)ePos.terminator].so_Spot);
        gm.audioManager.PlaySound(gm.board.spots[(int)ePos.terminator].so_Spot.audioLandOn);

    }
    public void OnPausePressed()
    {
        DisableButtons();
        gm.PlaySound(eSounds.click);
        gm.canvasManager.ShowCanvasOptions();

    }

    public void OnGOOJPressed()
    {
        WidgetMessage widget = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
        widget.Init(this.gameObject, "GetOutOfJail", "No", "Yes");
        
    }
    
    public void GetOutOfJail()
    {
        gm.playerFunctions.players[gm.spotFunctions.curPlayer].AvoidJail();
    }

    public void StartTurn()
    {
        foreach (Player p in gm.playerFunctions.players)
        {
            if (p.isBankrupt)
            {
                p.GoBankrupt();
            }
        }
        gm.playerFunctions.players.RemoveAll(item => item.isBankrupt == true);
        EnableButtons();
        HiLiteCurPlayer();
        bottomButtons[1].gameObject.SetActive(true); // Failsafe to make sure Rolling is enabled
        bottomButtons[0].gameObject.SetActive(false);

    }
    public void HandleEndOfTurn()
    {
        Debug.Log("TURN OVER");
        gm.playerFunctions.players[gm.spotFunctions.curPlayer].amtDoublesRolled = 0;
        gm.playerFunctions.players[gm.spotFunctions.curPlayer].areDoublesRolled = false;
        gm.playerFunctions.players[gm.spotFunctions.curPlayer].wasMovedByCard = false;
        if (gm.playerFunctions.players[gm.spotFunctions.curPlayer].isBankrupt)
        {
            WidgetMessage w = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
            w.Init("Ok", "", "You are bankrupt and forced to spend eternity trapped in the Black Lodge!");
        }
        gm.spotFunctions.AdvancePlayer();
        WidgetMessage widget = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
        widget.Init(this.gameObject, "StartTurn", "", "OK");
        gm.canvasManager.canvasHUD.bottomButtons[0].gameObject.SetActive(false);
        gm.canvasManager.canvasHUD.bottomButtons[1].gameObject.SetActive(true);

    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void DisableButtons()
    {
        foreach (Button b in bottomButtons)
        {
            b.interactable = false;
        }
    }
    public void EnableButtons()
    {
        foreach (Button b in bottomButtons)
        {
            b.interactable = true;
        }
    }
    
}
