using System.Collections;
using System.Collections.Generic;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using Google.Protobuf.Protocol;
using AnimationState = Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts.AnimationState;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }

    private StatInfo stat = new StatInfo();
    public virtual StatInfo Stat
    {
        get { return stat;}
        set
        {
            if (stat.Equals(value))
            {
                return;
            }

            stat.Hp = value.Hp;
            stat.MaxHp = value.MaxHp;
            stat.Speed = value.Speed;
        }
    }

    public float Speed
    {
        get { return stat.Speed; }
        set { stat.Speed = value; }
    }

    protected bool updated = false;

    private PositionInfo positionInfo = new PositionInfo();

    public PositionInfo PositionInfo
    {
        get { return positionInfo; }
        set
        {
            if (positionInfo.Equals(value))
            {
                return;
            }

            CellPosition = new Vector3Int(value.PosX, value.PosY, 0);
            State = value.State;
            Dir = value.MoveDir;
        }
    }

    public void SyncPosition()
    {
        Vector3 destPosition = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0f);
        transform.position = destPosition;
    }

    public Vector3Int CellPosition
    {
        get
        {
            return new Vector3Int(PositionInfo.PosX, PositionInfo.PosY);
        }
        set
        {
            if (PositionInfo.PosX == value.x && PositionInfo.PosY == value.y)
            {
                return;
            }
            
            PositionInfo.PosX = value.x;
            PositionInfo.PosY = value.y;
            updated = true;
        }
    }

    protected SpriteRenderer sprite;

    public virtual CreatureState State
    {
        get { return PositionInfo.State; }
        set
        {
            if (PositionInfo.State == value)
            {
                return;
            }

            PositionInfo.State = value;
            UpdateAnimation();
            updated = true;
        }
    }
    
    public MoveDir Dir
    {
        get { return PositionInfo.MoveDir; }
        set
        {
            if (PositionInfo.MoveDir == value)
            {
                return;
            }
            
            PositionInfo.MoveDir = value;
            
            UpdateAnimation();
            updated = true;
        }
    }

    public MoveDir GetDirFromVector(Vector3Int dir)
    {
        if (dir.x > 0)
        {
            return MoveDir.Right;
        }
        else if (dir.x < 0)
        {
            return MoveDir.Left;
        }
        else if (dir.y < 0)
        {
            return MoveDir.Up;   
        }
        else if (dir.y > 0)
        {
            return MoveDir.Down;
        }
        else
        {
            return MoveDir.Right;
        }
    }

    public Vector3Int GetFrontCellPos()
    {
        Vector3Int cellPosition = CellPosition;
        
        switch (Dir)
        {
            case MoveDir.Up:
                cellPosition += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellPosition += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellPosition += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellPosition += Vector3Int.right;
                break;
        }

        return cellPosition;
    }

    protected virtual void UpdateAnimation()
    {
        
    }
    
    
    void Start()
    {
        Init();
    }
    
    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        Vector3 startPosition = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0f);
        transform.position = startPosition;
        
        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Skill:
                UpdateSkill();
                break;
            case CreatureState.Dead:
                UpdateDead();
                break;
        }
    }
    
    protected virtual void UpdateIdle()
    {
        
    }
    
    // 스르륵 이동 처리
    protected virtual void UpdateMoving()
    {
        Vector3 destPosition = Managers.Map.CurrentGrid.CellToWorld(CellPosition) + new Vector3(0.5f, 0f);
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

    protected virtual void MoveToNextPosition()
    {
        
    }

    protected virtual void UpdateSkill()
    {
        
    }
    
    protected virtual void UpdateDead()
    {
        
    }
}
