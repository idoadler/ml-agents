using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using Random = UnityEngine.Random;

public class SilenceAgent : Agent
{
    public const int CollectionSize =
        SilenceAcademy.RiverSize * SilenceAcademy.ResourcesPerCard
        + SilenceAcademy.FirePitSize * SilenceAcademy.ResourcesPerCard
        + SilenceAcademy.AgentsNum * GraveyardMemory * SilenceAcademy.ResourcesPerCard
        + SilenceAcademy.AgentsNum * ActionsMemory * ActionSize
        + /*id*/ SilenceAcademy.AgentsNum
        + /*rules*/ /*SilenceManager.AgentsNum * */SilenceAcademy.ResourcesPerAgent; // redundancy - resources are known from id
//        res += /*count*/ SilenceManager.AgentsNum * SilenceManager.ResourcesPerAgent; // redundancy - known from firepit + rules + id

    public static readonly float[] EmptyAction = new float[ActionSize];
    public const int ActionSize = 2;
    public const int GraveyardMemory = 1;
    public const int ActionsMemory = 3;
    
    public enum RULES
    {
        NONE,
        MUST1,
        MUST2,
        MUSTNOT
    };

    public enum ACTIONS
    {
        NONE,
        RIVER,
        FIREPIT,
        FLUSH,
        FINISH,
        EXPOSE
    };

    [Header("Specific to Silence")] 
    public SilenceAcademy academy;
    public int id;
    [HideInInspector]
    public Dictionary<int, Sacrifice> sacrifices = new Dictionary<int, Sacrifice>();
    [HideInInspector]
    public int[][] graveyard = new int[GraveyardMemory][];
    [HideInInspector]
    public float[][] history = new float[ActionsMemory][];

    private int[] order = {0,1,2,3}; 
    private RULES[] rulesSet = {RULES.NONE,RULES.MUST1,RULES.MUST2,RULES.MUSTNOT}; 
    private float[] rulesOrder = new float[SilenceAcademy.ResourcesPerAgent];  
    
    public bool IsFirePitValid()
    {
        return sacrifices.Values.All(s => s.IsValid());
    }

    public override void InitializeAgent()
    {
        AgentReset();
    }

    public override void CollectObservations()
    {
        AddVectorObs(academy.river);
        AddVectorObs(academy.firePit);
        foreach (var agent in academy.agents)
        {
            AddVectorObs(agent.graveyard);
            AddVectorObs(agent.history);
        }

        AddVectorObs(id, SilenceAcademy.AgentsNum);
        AddVectorObs(rulesOrder);
        
        SetMask();
    }

    /// <summary>
    /// Applies the mask for the agents action to disallow unnecessary actions.
    /// </summary>
    void SetMask()
    {
        SetActionMask((int) ACTIONS.NONE);
        SetActionMask((int) ACTIONS.EXPOSE);
        SetActionMask((int) ACTIONS.FIREPIT, academy.GetFirePitMask());
    }
    
    public void AddVectorObs(int[][] cards)
    {
        foreach (var card in cards)
        foreach (var val in card)
            AddVectorObs(val);
    }

    public void AddVectorObs(float[][] actions)
    {
        foreach (var action in actions)
            AddVectorObs(action);
    }
    
    public void Win(int chooser)
    {
        Done();
        SetReward(id == chooser ? 2 : 1);
    }

    public void Lose(int chooser)
    {
        Done();
        SetReward(id == chooser ? -2 : -1);
    }
    
    public override void AgentAction(float[] vectorAction)
    {
        AddReward(-0.0001f);

        if (!IsFirePitValid())
            AddReward(-0.0001f);

        AddAction(vectorAction);
        
        academy.DoAction(id, vectorAction);
    }
    
    public void AddToGraveyard(int[] card)
    {
        for (var i = 1; i < graveyard.Length; i++)
            graveyard[i] = graveyard[i - 1];
        graveyard[0] = card;
    }
    
    public void AddAction(float[] action)
    {
        for (var i = 1; i < history.Length; i++)
            history[i] = history[i - 1];
        history[0] = action;
    }

    public override void AgentReset()
    {
        for (var i = 0; i < graveyard.Length; i++)
            graveyard[i] = SilenceAcademy.EmptyCard;
        for (var i = 0; i < history.Length; i++)
            history[i] = EmptyAction;
        sacrifices.Clear();
        
        for (var i = 0; i < order.Length; i++) {
            var temp = order[i];
            int randomIndex = Random.Range(i, order.Length);
            order[i] = order[randomIndex];
            order[randomIndex] = temp;
        }

        for (var i = 0; i < SilenceAcademy.ResourcesPerAgent; i++)
        {
            sacrifices.Add(id*SilenceAcademy.ResourcesPerAgent + order[i], new Sacrifice(rulesSet[i]));
            rulesOrder[order[i]] = (float) rulesSet[i];
        }
    }

    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            return new [] { (float) ACTIONS.RIVER, 0 };
        }        
        if (Input.GetKey(KeyCode.W))
        {
            return new [] { (float) ACTIONS.RIVER, 1 };
        }        
        if (Input.GetKey(KeyCode.E))
        {
            return new [] { (float) ACTIONS.RIVER, 2 };
        }
        if (Input.GetKey(KeyCode.R))
        {
            return new [] {(float) ACTIONS.RIVER, 3};
        }
        if (Input.GetKey(KeyCode.Alpha1))
        {
            return new [] { (float) ACTIONS.FIREPIT, 0 };
        }        
        if (Input.GetKey(KeyCode.Alpha2))
        {
            return new [] { (float) ACTIONS.FIREPIT, 1 };
        }        
        if (Input.GetKey(KeyCode.Alpha3))
        {
            return new [] { (float) ACTIONS.FIREPIT, 2 };
        }        
        if (Input.GetKey(KeyCode.Alpha4))
        {
            return new [] { (float) ACTIONS.FIREPIT, 3 };
        }        
        if (Input.GetKey(KeyCode.Alpha5))
        {
            return new [] { (float) ACTIONS.FIREPIT, 4 };
        }        
        if (Input.GetKey(KeyCode.Alpha6))
        {
            return new [] { (float) ACTIONS.FIREPIT, 5 };
        }          
        if (Input.GetKey(KeyCode.Z))
        {
            return new [] { (float) ACTIONS.FINISH, 0 };
        }           
        if (Input.GetKey(KeyCode.A))
        {
            return new [] { (float) ACTIONS.EXPOSE, 0 };
        }
        if (Input.GetKey(KeyCode.S))
        {
            return new [] { (float) ACTIONS.EXPOSE, 1 };
        }        
        if (Input.GetKey(KeyCode.D))
        {
            return new [] { (float) ACTIONS.EXPOSE, 2 };
        }        
        if (Input.GetKey(KeyCode.F))
        {
            return new [] { (float) ACTIONS.EXPOSE, 3 };
        }        

        return new [] { (float) ACTIONS.FLUSH, 0 };
    }

    public override void AgentOnDone()
    {
    }

    public void AddCard(IEnumerable<int> card)
    {
        foreach (var val in card)
            if (sacrifices.ContainsKey(val))
                sacrifices[val].count++;
    }
    
    public void RemoveCard(IEnumerable<int> card)
    {
        foreach (var val in card)
            if (sacrifices.ContainsKey(val))
                sacrifices[val].count--;
    }
    
    public class Sacrifice
    {
        public RULES type;
        public int count;

        public Sacrifice(RULES type)
        {
            this.type = type;
            count = 0;
        }

        public bool IsValid()
        {
            return type == RULES.NONE ||
                   type == RULES.MUST1 && count >= 1 ||
                   type == RULES.MUST2 && count >= 2 ||
                   type == RULES.MUSTNOT && count == 0;
        }
    }

    public string PrintState()
    {
        var rules = string.Join(",", rulesOrder.Select(i => i.ToString()).ToArray()); 
        var graves = string.Join(" ", graveyard.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        var hist  = string.Join(" ", history.Select(card => string.Join(",", card.Select(i => i.ToString()).ToArray()) ).ToArray());
        return ("id:" + id +", valid:" + IsFirePitValid() +", rules:[" + rules +"], graves:[" + graves + "], history:[" + hist + "]" );
    }
}
