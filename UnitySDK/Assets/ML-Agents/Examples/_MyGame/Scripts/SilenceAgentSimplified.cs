using System;
using System.Linq;
using UnityEngine;
using MLAgents;
using Random = UnityEngine.Random;

public class SilenceAgentSimplified : Agent
{
    public const int CollectionSize = StoreSize + BagSize;
    private static readonly int[] Empty = new int[ItemsTypes]{0,0};
    private const int ItemsTypes = 2;
    private const int StoreSize = 1;
    private const int BagSize = 2;
    private const int ItemsOrderLimit = 100;

    private enum ACTIONS
    {
        BUY,
        RETURN,
        COUNT
    };

    public SilenceAcademySimplified academy;
    
    [HideInInspector]
    public int[][] store = new int[StoreSize][] {Empty};
    [HideInInspector]
    public int[][] bag = new int[BagSize][] {Empty,Empty};
    private int bagItems = 0;
    private int itemsOrdered = 0;
    private int badItemsNum = 0;

    public override void InitializeAgent()
    {
        academy = FindObjectOfType<SilenceAcademySimplified>();
    }

    public override void CollectObservations()
    {
        AddVectorObs(store);
        AddVectorObs(bag);
    }

    private void AddVectorObs(int[][] items)
    {
        foreach (var item in items)
        foreach (var val in item)
            AddVectorObs(val);
    }

    public void Win()
    {
        Done();
        SetReward(1);
    }

    public void Lose()
    {
        Done();
        SetReward(-1);
    }
    
    public override void AgentAction(float[] vectorAction)
    {
        if (badItemsNum > 0)
            AddReward(-0.1f);
        
        var action = (ACTIONS) Mathf.RoundToInt(vectorAction[0]);
        var index = Mathf.RoundToInt(vectorAction[1]);
        switch (action)
        {
            case ACTIONS.BUY:
                bag[bagItems] = store[index];
                store[index] = OrderItem();
                AddItem(bag[bagItems]);
                bagItems++;
                if (bagItems == BagSize)
                {
                    FinishGame();
                    return;
                }
                break;
            case ACTIONS.RETURN:
                if (bagItems == 0)
                {
                    AddReward(-0.1f);
                    return;
                }

                RemoveItem(bag[index]);
                for (var i = index; i < bagItems; i++)
                    bag[i] = bag[i + 1];
                bag[bagItems] = Empty;
                bagItems--;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (itemsOrdered > ItemsOrderLimit)
            FinishGame();
        
    }

    public override void AgentReset()
    {
        badItemsNum = 0;
        bagItems = 0;
        itemsOrdered = 0;

        for (var i = 0; i < store.Length; i++)
            store[i] = OrderItem();
        for (var i = 0; i < bag.Length; i++)
            bag[i] = Empty;
    }

    private int[] OrderItem()
    {
        itemsOrdered++;
        var vec = new int[ItemsTypes]{0,0};
        vec[Random.Range(0, vec.Length)]++; 
        return vec;
    }

    private void FinishGame()
    {
        PrintState();
        if (badItemsNum == 0)
            Win();
        else
            Lose();
    }

    public override float[] Heuristic()
    {
        return new float[] { Random.Range(0,(int)ACTIONS.COUNT), 0 };
    }

    public override void AgentOnDone()
    {
    }

    private void AddItem(int[] item)
    {
        if (item[0] == 1)
            badItemsNum++;
    }

    private void RemoveItem(int[] item)
    {
        if (item[0] == 1)
            badItemsNum--;
    }

    private void PrintState()
    {
        var shopState = string.Join(" ", store.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        var bagState = string.Join(" ", bag.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        
        Debug.Log( "count:" + badItemsNum + " shop[" + itemsOrdered +"]:" + shopState + ", bag[" + bagItems + "]:" + bagState);
    }
}
