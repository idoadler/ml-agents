using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class SilenceManager : MonoBehaviour
{
    public void SceneReset()
    {
        PrintState();
        ResetAll();
    }
    
    public void Step()
    {
        agents[currentAgent].RequestAction();
    }
    
    public static readonly int[] EmptyCard = new int[AgentsNum*ResourcesPerAgent];
    public const int AgentsNum = 4;
    public const int ResourcesPerAgent = 4;
    public const int ResourcesPerCard = 4;
    public const int RiverSize = 4;
    public const int FirePitSize = 7;
    public const int MaxRiverSize = 100;

    [HideInInspector]
    public int[][] river = new int[RiverSize][];
    [HideInInspector]
    public int[][] firePit = new int[FirePitSize][];
    
    public SilenceAgent[] agents = new SilenceAgent[AgentsNum];
    private int currentAgent = 0;
    private int currentFirePit = 0;
    private int riverSize = 0;
    
    public void ResetAll()
    {
        currentAgent = 0;
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

    public void FinishGame(int id)
    {
        PrintState();
        if (agents.All(agent => agent.IsFirePitValid()))
            foreach (var agent in agents)
                agent.Win(id);
        else
            foreach (var agent in agents)
                agent.Lose(id);
        SceneReset();
    }

    public void DoAction(int id, float[] vectorAction)
    {
        Assert.AreEqual(id, currentAgent);
        var action = (SilenceAgent.ACTIONS) Mathf.RoundToInt(vectorAction[0]);
        var index = Mathf.RoundToInt(vectorAction[1]);
        switch (action)
        {
            case SilenceAgent.ACTIONS.NONE:
                throw new NotImplementedException();
            case SilenceAgent.ACTIONS.RIVER:
                currentAgent++;
                firePit[currentFirePit] = river[index];
                river[index] = GetRandomCard();
                foreach (var agent in agents)
                    agent.AddCard(firePit[currentFirePit]);
                currentFirePit++;
                if (currentFirePit == FirePitSize)
                {
                    FinishGame(id);
                    return;
                }
                break;
            case SilenceAgent.ACTIONS.FIREPIT:
                currentAgent++;
                foreach (var agent in agents)
                    agent.RemoveCard(firePit[index]);
                agents[id].AddToGraveyard(firePit[index]);
                for (var i = index; i < currentFirePit; i++)
                    firePit[i] = firePit[i + 1];
                firePit[currentFirePit] = EmptyCard;
                currentFirePit--;
                break;
            case SilenceAgent.ACTIONS.FLUSH:
                currentAgent++;
                for(var i = 0; i < river.Length; i++)
                    river[i] = GetRandomCard();
                break;
            case SilenceAgent.ACTIONS.FINISH:
                FinishGame(id);
                return;
            case SilenceAgent.ACTIONS.EXPOSE:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (riverSize > MaxRiverSize)
            FinishGame(id);
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
        Debug.Log(SilenceAgent.CollectionSize);
    }

    public void PrintState()
    {
        var ages = "";
        foreach (var agent in agents)
        {
            ages += agent.PrintState() + " ";
        }

        var lastRiver = string.Join(" ", river.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        var lastFire = string.Join(" ", firePit.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        
        Debug.Log( "river[" + riverSize +"]:" + lastRiver + ", firePit[" + currentFirePit + "]:" + lastFire + ", agents:[" + ages + "]");
    }
}
