using System;
using System.Linq;
using UnityEngine;
using MLAgents;
using Random = UnityEngine.Random;

public class SilenceAgentSimplified : Agent
{
    public const int CollectionSize = ItemsTypes * (StoreSize + BagSize);
    private static readonly int Empty = -1;
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
    
    private int store = Empty;
    private int[] bag = new int[BagSize] {Empty,Empty};
    private int bagItems = 0;
    private int actionsNum = 0;

    public override void InitializeAgent()
    {
        academy = FindObjectOfType<SilenceAcademySimplified>();
    }

    public override void CollectObservations()
    {
        AddVectorObs(store);
        AddVectorObs(bag[0]);
        AddVectorObs(bag[1]);
    }

    private ACTIONS action;
    private int index;
    public override void AgentAction(float[] vectorAction)
    {
        actionsNum++;
 
        action = (ACTIONS) Mathf.RoundToInt(vectorAction[0]);
        if (action == ACTIONS.RETURN)
        {
            if (bagItems == 0)
            {
                SetReward(-0.1f);
                return;
            }

            bag[0] = Empty;
            bagItems--;
        }
        else if (action == ACTIONS.BUY)
        {
            bag[bagItems] = store;
            store = Random.Range(0, 2);
            bagItems++;
        }
        
        if (bagItems == BagSize)
        {
            var win = bag[0] == bag[1];
            PrintState(win);
            if (win)
            {
                Done();
                SetReward(1);
            }
            else
            {
                Done();
                SetReward(-1);
            }
        }
        else
        {
            SetReward(0);
        }
    }

    public override void AgentReset()
    {
        actionsNum = 0;
        bagItems = 0;
        store = Random.Range(0, 2);
        bag[0] = Empty;
        bag[1] = Empty;
    }

    public override float[] Heuristic()
    {
        return new float[] { Random.Range(0,(int)ACTIONS.COUNT) };
    }

    public override void AgentOnDone()
    {
    }

    private void PrintState(bool win)
    {
        if(!win)
            Debug.Log( "count:" + actionsNum + " shop:" + store + ", bag:" + bag[0] + "," + bag[1]);
    }
}
