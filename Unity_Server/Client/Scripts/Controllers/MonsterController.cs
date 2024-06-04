using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Google.Protobuf.Protocol;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using Random = System.Random;

public class MonsterController : CreatureController
{
    private Coroutine punchCoroutine;
    
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
    }
    
    public override void OnDamaged()
    {
        // Managers.Object.Remove(Id);
        // Managers.Resource.Destroy(gameObject);
    }
    
    public override void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            State = CreatureState.Skill;
        }
    }
}
