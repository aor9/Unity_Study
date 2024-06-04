using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BulletController : BaseController
{
    
    protected override void Init()
    {
        switch (Dir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        
        State = CreatureState.Moving;
        
        base.Init();
        Vector3 startPosition = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0f, 0.5f);
        transform.position = startPosition;
    }

    protected override void UpdateAnimation()
    {
    }
    
    protected override void UpdateMoving()
    {
        if (State != CreatureState.Moving)
        {
            return;
        }
        
        Vector3 destPosition = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0.5f);
        Vector3 moveDir = destPosition - transform.position;

        float dist = moveDir.magnitude;
        if (dist < Speed * Time.deltaTime)
        {
            transform.position = destPosition;
            MoveToNextPosition();
        }
        else
        {
            transform.position += moveDir.normalized * Speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
    }
}
