using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using Google.Protobuf.Protocol;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using UnityEngine;

public class CreatureController : BaseController
{
    protected GameScene gameScene;
    public Character Character;
    public ParticleSystem MoveDust;

    public override StatInfo Stat
    {
        get { return base.Stat; }
        set
        {
            base.Stat = value;
        }
    }
    public int Hp
    {
        get { return Stat.Hp;}
        set
        {
            Stat.Hp = value;
            gameScene.UpdateHpBar();
        }
    }
    
    protected override void Init()
    {
        gameScene = GameObject.Find("@GameScene").GetComponent<GameScene>();
        base.Init();
    }
    
    protected override void UpdateAnimation()
    {
        if (Character is null || sprite is null)
        {
            return;
        }
        
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
            Character.Animator.SetTrigger("Jab");
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

    public virtual void OnDamaged()
    {
        
    }
    
    public virtual void OnDead()
    {
        State = CreatureState.Dead;
        
        GameObject effect = Managers.Resource.Instantiate("Effect/DeadEffect");
        effect.transform.position = transform.position + new Vector3(0, 0.5f, 0);
        effect.GetComponent<Animator>().Play("Start");
        GameObject.Destroy(effect, 0.5f);
    }

    public virtual void UseSkill(int skillId)
    {
        
    }
}
