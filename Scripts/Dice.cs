using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    Rigidbody rb;
    bool hasLanded;
    bool isRolled;
    Vector3 startPos;

    public int diceValue;
    public DiceValue[] diceValues;
    public AudioClip audioGetLucky;

    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        startPos = transform.position;
        rb.useGravity = false;
        RollDice();
    }

    public void RollDice()
    {
        if (!isRolled && !hasLanded)
        {
            isRolled = true;
            rb.useGravity = true;
            rb.AddForce(-transform.up * 10, ForceMode.Impulse);
            rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
            


        }
        else if(isRolled && hasLanded)
        {
            ResetDice();
        }
    }

    public void GetDiceValue()
    {
        diceValue = 0;
        foreach  (DiceValue side in diceValues)
        {
            if (side.OnBoard())
            {
                diceValue = side.sideValue;
                Debug.Log("You rolled a " + diceValue);
            }
        }
    }

    public void ResetDice()
    {
        transform.position = startPos;
        isRolled = false;
        hasLanded = false;
        rb.useGravity = false;
        rb.isKinematic = false;
        
    }
    void RollAgain()
    {
        ResetDice();
        isRolled = true;
        rb.useGravity = true;
        rb.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));

    }

    public void Update()
    {
        if (rb.IsSleeping() && !hasLanded && isRolled)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            GetDiceValue();
        }
        else if (rb.IsSleeping() && hasLanded && diceValue == 0)
        {
            RollAgain();
        }
    }
}