using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class SilenceManagerSimplified : MonoBehaviour
{
    public void SceneReset()
    {
        PrintState();
        ResetAll();
    }
    
    public void Step()
    {
        agent.RequestAction();
    }
    
    public static readonly int[] EmptyCard = new int[AgentsNum*ResourcesPerAgent]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
    public const int AgentsNum = 4;
    public const int ResourcesPerAgent = 4;
    public const int ResourcesPerCard = 4;
    public const int RiverSize = 4;
    public const int FirePitSize = 7;
    public const int MaxRiverSize = 100;

    [HideInInspector]
    public int[][] river = new int[RiverSize][] {EmptyCard,EmptyCard,EmptyCard,EmptyCard};
    [HideInInspector]
    public int[][] firePit = new int[FirePitSize][] {EmptyCard,EmptyCard,EmptyCard,EmptyCard,EmptyCard,EmptyCard,EmptyCard};
    
    public SilenceAgentSimplified agent;
    private int currentFirePit = 0;
    private int riverSize = 0;
    
    public void ResetAll()
    {
        currentFirePit = 0;
        riverSize = 0;

        for (var i = 0; i < river.Length; i++)
            river[i] = GetRandomCard();
        for (var i = 0; i < firePit.Length; i++)
            firePit[i] = EmptyCard;
    }

    public int[] GetRandomCard()
    {
        riverSize++;
        var vec = new int[AgentsNum * ResourcesPerAgent];
        for (var i = 0; i < ResourcesPerCard; i++)
            vec[Random.Range(0, vec.Length)]++;

        return vec;
    }

    public void FinishGame()
    {
        PrintState();
        if (agent.IsFirePitValid())
            agent.Win();
        else
            agent.Lose();
        SceneReset();
    }

    public void DoAction(float[] vectorAction)
    {
        var action = (SilenceAgentSimplified.ACTIONS) Mathf.RoundToInt(vectorAction[0]);
        var index = Mathf.RoundToInt(vectorAction[1]);
        switch (action)
        {
            case SilenceAgentSimplified.ACTIONS.RIVER:
                firePit[currentFirePit] = river[index];
                river[index] = GetRandomCard();
                agent.AddCard(firePit[currentFirePit]);
                currentFirePit++;
                if (currentFirePit == FirePitSize)
                {
                    FinishGame();
                    return;
                }
                break;
            case SilenceAgentSimplified.ACTIONS.FIREPIT:
                agent.RemoveCard(firePit[index]);
                for (var i = index; i < currentFirePit; i++)
                    firePit[i] = firePit[i + 1];
                firePit[currentFirePit] = EmptyCard;
                currentFirePit--;
                break;
            case SilenceAgentSimplified.ACTIONS.FINISH:
                FinishGame();
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (riverSize > MaxRiverSize)
            FinishGame();
    }

    private readonly int[][] firePitMasks =
    {
        new []{0,1,2,3,4,5,6},
        new []{1,2,3,4,5,6},
        new []{2,3,4,5,6},
        new []{3,4,5,6},
        new []{4,5,6},
        new []{5,6},
        new []{6},
    };
    public IEnumerable<int> GetFirePitMask()
    {
        return firePitMasks[currentFirePit];
    }

    private void OnDisable()
    {
        Debug.Log(SilenceAgentSimplified.CollectionSize);
    }

    public void PrintState()
    {
        var ages = agent.PrintState();
        var lastRiver = string.Join(" ", river.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        var lastFire = string.Join(" ", firePit.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        
        Debug.Log( "river[" + riverSize +"]:" + lastRiver + ", firePit[" + currentFirePit + "]:" + lastFire + ", agents:[" + ages + "]");
    }
}
