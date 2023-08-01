using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ePlayerType { none, human, AI, terminator}

public enum ePiece { cat, car, hat, dog, battleship, thimble, shoe, wheelbarrow, terminator }
public enum ePos {
    go, mediterranean, commChest1, baltic, incomeTax, readingRR, oriental, chance1, vermont, connecticut, justVisiting,
    stCharles, electricCompany, states, virginia, pennRR, stJames, commChest2, tennessee, newYork, freeParking,
    kentucky, chance2, indiana, illinois, boRR, atlantic, ventnor, waterWorks, marvinGardens, goToJail,
    pacific, northCarolina, commChest3, pennsylvania, shortLineRR, chance3, parkPlace, luxuryTax, boardwalk, terminator
}

public enum eColorSet { none, brown, lightBlue, pink, orange, red, yellow, green, blue, utility, railRoad, terminator}
[System.Serializable]
public class PropertyOwnership
{
    public soSpot so_Spot;
    public string propName;    
    public bool isMortgaged;
    public int houseAmt;
    public int hotelAmt;

    public PropertyOwnership(soSpot _soSpot, bool _isMortgaged, int _houseAmt)
    {
        so_Spot = _soSpot;
        propName = so_Spot.spotName;
        isMortgaged = _isMortgaged;
        houseAmt = _houseAmt;        
    } 
}
public class Player : MonoBehaviour
{
    public int playerIndex;
    public string playerName;
    public ePlayerType playerType;    
    public ePos playerPos;
    public ColorBlock playerColor;
    public int cashOnHand;
    public soPiece playerPiece; // This holds the template for the player piece
    public List<bool> hasGOOJailCard;
    public bool inJail = false;
    public bool wasMovedByCard = false;
    public bool areDoublesRolled;
    public int amtDoublesRolled;
    public Transform pieceObj;  // This is the instantiated player piece
    public Vector2 offset;
    public WidgetPlayerHUD widgetPlayerHUD;
    public bool isBankrupt = false;

    public List<PropertyOwnership> listPropertiesOwned;

    private void Awake()
    {
        listPropertiesOwned = new List<PropertyOwnership>();
    }
    public void IncrementPlayerPiece()
    {
        int curSelectedPiece = (int)playerPiece.piece; //enum index of pieces
        curSelectedPiece++; //advance var by one
        if ((ePiece) curSelectedPiece == ePiece.terminator)
        {
            curSelectedPiece = 0;
        }
        
        playerPiece = GameManager.gm.so_Rep.pieces[curSelectedPiece];
    }    

    public void AdjustCash(int _value)
    {
        int cashToEval = cashOnHand;
        cashToEval += _value;
        if (cashToEval < cashOnHand && !isBankrupt)
        {
            WidgetMessage w = Instantiate(Resources.Load("Widgets/" + "Widget_Message") as GameObject, this.transform).GetComponent<WidgetMessage>();
            w.Init("Ok", "", "You lack the requried funds! Manage properties to remove your negative balance or press End Turn to declare Bankruptcy.");
            cashOnHand += _value;
            isBankrupt = true;
            widgetPlayerHUD.SetPlayerCash();
            return;
        }
        cashOnHand += _value;
        widgetPlayerHUD.SetPlayerCash();

        
    }

    public void BuyProperty(soSpot _soSpot) // Subtract the cost of the property from this player's cashOnHand and adds a PropertyOwnership reference to the list of properties owned
    {
        AdjustCash(-_soSpot.cost);
        listPropertiesOwned.Add(new PropertyOwnership(_soSpot, false, 0));
    }
   
    public void PayRent(Player _player, int _rent) // Subtracts cash from this player and adds it to the cashOnHand of the player we passed to this method
    {
        _player.AdjustCash(+_rent);
        AdjustCash(-_rent);
    }

    public bool IsPropertyOwned(soSpot _soSpot)  // Steps through all the properties owned by this player and returns a true value if they own the property that we passed to this method
    {
        for (int i = 0; i < listPropertiesOwned.Count; i++)
        {
            if (listPropertiesOwned[i].so_Spot == _soSpot)
            {
                return true;
            }
        }
        return false;
    }
    public bool GetMonopolyOwned(soSpot _soSpot)  // Steps through an array containing the other properties needed for a monopoly and if any of them are not owned, returns a false value
    {
        for (int i = 0; i < _soSpot.otherPropertiesInGroup.Length; i++)
        {
            if (!IsPropertyOwned(_soSpot.otherPropertiesInGroup[i]))
            {
                return false;
            }
        }
        return true;
    }

    public int GetNumHouses(soSpot _soSpot) // Steps through all properties owned by this player and returns the amount of Houses owned
    {
        for(int i = 0; i < listPropertiesOwned.Count; i++)
        {
            if (listPropertiesOwned[i].so_Spot == _soSpot)
            {
                return listPropertiesOwned[i].houseAmt;
            }
        }
        return 99;
    }

    public int GetNumRR(soSpot _soSpot)  // Steps through all properties owned by this player and returns the amount of Railroads owned
    {
        int rrOwned = 0;
        for (int i = 0; i < listPropertiesOwned.Count; i++)
        {
            if (listPropertiesOwned[i].so_Spot.typeSpot == eTypeSpot.railroad)
            {
                rrOwned++;
            }
            
        }
        return rrOwned;
    }
    public bool IsPropertyMortgaged(soSpot _soSpot) // Steps through all properties owned by this player and checks whether the passed property is mortgaged (returns a true value if so)
    {
        for (int i = 0; i < listPropertiesOwned.Count; i++)
        {
            if (listPropertiesOwned[i].isMortgaged)
            {
                return true;
            }
        }
        return false;
    }
    public void GoToJail() // Moves the player and does end of turn cleanup
    {   
        GameManager.gm.spotFunctions.GoToSpot(ePos.justVisiting, true);
        inJail = true;
        amtDoublesRolled = 0;
        areDoublesRolled = false;
        wasMovedByCard = false;
        GameManager.gm.canvasManager.canvasHUD.bottomButtons[1].gameObject.SetActive(false); //Roll dice turned off
        GameManager.gm.canvasManager.canvasHUD.bottomButtons[0].gameObject.SetActive(true);  // End turn turned on
        if (hasGOOJailCard[0])
        {
            GameManager.gm.canvasManager.canvasHUD.bottomButtons[4].gameObject.SetActive(false); // Log turned off
            GameManager.gm.canvasManager.canvasHUD.bottomButtons[6].gameObject.SetActive(true);  // Replaced with GOOJ
        }
    }
    public void AvoidJail()
    {
        GameManager.gm.canvasManager.canvasHUD.bottomButtons[6].gameObject.SetActive(false); // GOOJ turned off
        GameManager.gm.canvasManager.canvasHUD.bottomButtons[4].gameObject.SetActive(true); // Log turned back on
        hasGOOJailCard.Remove(hasGOOJailCard[0]);
        inJail = false;
        bool isMissing = true;
        foreach (soCard card in GameManager.gm.so_Rep.cardsChance)  // Convoluted means of checking to see which List is missing a GOOJ card and then adding it back
        {
            if (card.cardAction == eCardAction.getOutOfJail)
            {
                isMissing = false;
                GameManager.gm.so_Rep.cardsChance.Add(GameManager.gm.so_Rep.GetOutOfJail[0]);
                
            }
        }
        foreach (soCard card in GameManager.gm.so_Rep.cardsChest)
        {
            if (card.cardAction == eCardAction.getOutOfJail)
            {
                isMissing = false;
                GameManager.gm.so_Rep.cardsChest.Add(GameManager.gm.so_Rep.GetOutOfJail[1]);
                
            }
        }
        
    }

    public void GoBankrupt()
    {
        listPropertiesOwned.Clear();
        if (hasGOOJailCard[0])
        {
            hasGOOJailCard.Clear();
            bool isMissing = true;
            foreach (soCard card in GameManager.gm.so_Rep.cardsChance)  // Convoluted means of checking to see which List is missing a GOOJ card and then adding it back
            {
                if (card.cardAction == eCardAction.getOutOfJail)
                {
                    isMissing = false;
                    GameManager.gm.so_Rep.cardsChance.Add(GameManager.gm.so_Rep.GetOutOfJail[0]);

                }
            }
            foreach (soCard card in GameManager.gm.so_Rep.cardsChest)
            {
                if (card.cardAction == eCardAction.getOutOfJail)
                {
                    isMissing = false;
                    GameManager.gm.so_Rep.cardsChest.Add(GameManager.gm.so_Rep.GetOutOfJail[1]);

                }
            }
        }
        
    }

}

