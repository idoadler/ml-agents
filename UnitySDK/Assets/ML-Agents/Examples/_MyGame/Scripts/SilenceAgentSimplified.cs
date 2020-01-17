using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MLAgents;
using Random = UnityEngine.Random;

public class SilenceAgentSimplified : Agent
{
    public const int CollectionSize =
        SilenceManagerSimplified.RiverSize * SilenceManagerSimplified.ResourcesPerCard
        + SilenceManagerSimplified.FirePitSize * SilenceManagerSimplified.ResourcesPerCard
        + /*id*/ SilenceManagerSimplified.AgentsNum
        + /*rules*/ SilenceManagerSimplified.ResourcesPerAgent * (int)RULES.COUNT; 

    public static readonly float[] EmptyAction = new float[ActionSize];
    public const int ActionSize = 2;

    public enum RULES
    {
        NONE,
        MUST1,
        MUSTNOT,
        COUNT
    };

    public enum ACTIONS
    {
        RIVER,
        FIREPIT,
        FINISH,
        COUNT
    };

    [Header("Specific to Silence")] 
    public SilenceAcademySimplified academy;
    public SilenceManagerSimplified manager;
    [HideInInspector]
    public Dictionary<int, Sacrifice> sacrifices = new Dictionary<int, Sacrifice>();

    private int[] order = {0,1,2,3}; 
    private RULES[] rulesSet = {RULES.NONE,RULES.MUST1,RULES.MUST1,RULES.MUSTNOT}; 
    private RULES[] rulesOrder = new RULES[SilenceManagerSimplified.ResourcesPerAgent];  
    
    public bool IsFirePitValid()
    {
        return sacrifices.Values.All(s => s.IsValid());
    }

    public override void InitializeAgent()
    {
        academy = FindObjectOfType<SilenceAcademySimplified>();
    }

    public override void CollectObservations()
    {
        AddVectorObs(manager.river);
        AddVectorObs(manager.firePit);
        foreach (var rule in rulesOrder)
        {
            AddVectorObs((int)rule, (int)RULES.COUNT);
        }
        
        SetMask();
    }

    /// <summary>
    /// Applies the mask for the agents action to disallow unnecessary actions.
    /// </summary>
    void SetMask()
    {
        var firePitMask = manager.GetFirePitMask();
        if (firePitMask.Count() == SilenceManager.FirePitSize)
            SetActionMask((int) ACTIONS.FIREPIT);
        else
            SetActionMask((int) ACTIONS.FIREPIT, firePitMask);
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
        {
            AddVectorObs(Mathf.RoundToInt(action[0]), (int) ACTIONS.COUNT);
            AddVectorObs(action[1]);
        }
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
        AddReward(-0.0001f);

        if (!IsFirePitValid())
            AddReward(-0.0001f);
        
        manager.DoAction(vectorAction);
    }

    public override void AgentReset()
    {
        sacrifices.Clear();
        
        for (var i = 0; i < order.Length; i++) {
            var temp = order[i];
            int randomIndex = Random.Range(i, order.Length);
            order[i] = order[randomIndex];
            order[randomIndex] = temp;
        }

        for (var i = 0; i < SilenceManagerSimplified.ResourcesPerAgent; i++)
        {
            sacrifices.Add(order[i], new Sacrifice(rulesSet[i]));
            rulesOrder[order[i]] = rulesSet[i];
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

        return new [] { (float) ACTIONS.RIVER, 0 };
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
                   type == RULES.MUSTNOT && count == 0;
        }
    }

    public string PrintState()
    {
        var rules = string.Join(",", rulesOrder.Select(i => i.ToString()).ToArray()); 
        return ("valid:" + IsFirePitValid() +", rules:[" + rules + "]" );
    }
}
