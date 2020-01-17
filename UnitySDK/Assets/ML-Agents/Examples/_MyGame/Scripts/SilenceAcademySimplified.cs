using System;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class SilenceAcademySimplified : Academy
{
    public SilenceManagerSimplified[] managers;

    public override void InitializeAcademy()
    {
        base.InitializeAcademy();
        managers = FindObjectsOfType<SilenceManagerSimplified>();
    }

    public override void AcademyReset()
    {
        base.AcademyReset();
        foreach (var man in managers)
        {
            man.SceneReset();
        }
    }
    
    public override void AcademyStep()
    {
        base.AcademyStep();
        foreach (var man in managers)
        {
            man.Step();
        }
    }
}
