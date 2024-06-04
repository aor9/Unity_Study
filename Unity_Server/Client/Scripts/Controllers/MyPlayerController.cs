using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    private bool isMovePressed = false;
    protected override void Init()
    {
        Character.SetState(AnimationState.Idle);
        base.Init();
    }
    
    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }
        
        base.UpdateController();
    }
    
    protected override void UpdateIdle()
    {
        if (isMovePressed)
        {
            State = CreatureState.Moving;
            return;
        }
        
        if (skillCoolTimeCoroutine == null && Input.GetKeyDown(KeyCode.J))
        {
            C_Skill skill = new C_Skill() { Info = new SkillInfo() };
            skill.Info.SkillId = 2;
            Managers.Network.Send(skill);

            skillCoolTimeCoroutine = StartCoroutine("InputCoolTimeCoroutine", 0.2f);
        }
    }

    private Coroutine skillCoolTimeCoroutine;
    
    private IEnumerator InputCoolTimeCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        skillCoolTimeCoroutine = null;
    }
    
    private void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
    
    void GetDirInput()
    {
        isMovePressed = true;
        
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
            MoveDust.Play();
        } 
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
            MoveDust.Play();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
            MoveDust.Play();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
            MoveDust.Play();
        }
        else
        {
            isMovePressed = false;
            MoveDust.Stop();
        }
    }
    
    protected override void MoveToNextPosition()
    {
        if (isMovePressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag();
            return;
        }
        
        Vector3Int destPosition = CellPosition;
        switch (Dir)
        {
            case MoveDir.Up:
                destPosition += Vector3Int.up;
                break;
            case MoveDir.Down:
                destPosition += Vector3Int.down;
                break;
            case MoveDir.Left:
                destPosition += Vector3Int.left;
                break;
            case MoveDir.Right:
                destPosition += Vector3Int.right;
                break;
        }
        
        if (Managers.Map.IsMoveable(destPosition))
        {
            if (Managers.Object.FindCreatre(destPosition) is null)
            {
                CellPosition = destPosition;
            }
        }
        
        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if (updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PositionInfo = PositionInfo;
            Managers.Network.Send(movePacket);
            Debug.Log(movePacket.PositionInfo.State);
            updated = false;
        }
    }
}
