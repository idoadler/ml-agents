using System;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class SilenceAcademy : Academy
{
    public SilenceManager[] managers;

    public override void InitializeAcademy()
    {
        base.InitializeAcademy();
        managers = FindObjectsOfType<SilenceManager>();
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