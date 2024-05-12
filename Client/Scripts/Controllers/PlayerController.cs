using System;
using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using Google.Protobuf.Protocol;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using UnityEngine;

public class PlayerController : CreatureController
{
    protected Coroutine skillCoroutine;
    protected bool isRangedSkill = false;
    
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        if (sprite == null) return;
        
        if (State == CreatureState.Idle)
        {
            Character.SetState(AnimationState.Idle);
        }
        else if (State == CreatureState.Moving)
        {
            Character.SetState(AnimationState.Running);
            switch (Dir)
            {
                case MoveDir.Left:
                    sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            Character.Animator.SetTrigger(isRangedSkill ? "Slash" : "Jab");
            switch (Dir)
            {
                case MoveDir.Left:
                    sprite.flipX = true;
                    break;
                case MoveDir.Right:
                    sprite.flipX = false;
                    break;
            }
        }
        else
        {

        }
    }

    protected override void UpdateController()
    {
        base.UpdateController();
    }
    
    public override void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            skillCoroutine = StartCoroutine("StartBasicAttack");
        }
        else if (skillId == 2)
        {
            skillCoroutine = StartCoroutine("StartShooting");
        }
    }
    
    protected virtual void CheckUpdatedFlag()
    {
        
    }
    
    IEnumerator StartBasicAttack()
    {
        isRangedSkill = false;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        skillCoroutine = null;
        CheckUpdatedFlag();
    }

    IEnumerator StartShooting()
    {
        isRangedSkill = true;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.2f);
        State = CreatureState.Idle;
        skillCoroutine = null;
        CheckUpdatedFlag();
    }
}

