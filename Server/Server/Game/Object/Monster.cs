using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Monster : GameObject
    {
        public Monster()
        {
            ObjectType = GameObjectType.Monster;

            //Test
            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 5.0f;

            State = CreatureState.Idle;
        }

        //FSM
        public override void Update()
        {
            switch(State)
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

        Player _target;
        int searchCellDist = 7;
        int chaseCellDist = 15;

        long nextSearchTick = 0;
        protected virtual void UpdateIdle()
        {
            if(nextSearchTick > Environment.TickCount64) 
            {
                return;
            }
            nextSearchTick = Environment.TickCount64 + 1000;

            Player target = Room.FindPlayer(p =>
            {
                Vector2Int dir = p.CellPosition - CellPosition;
                return dir.cellDistFromZero <= searchCellDist;
            });

            if (target is null)
            { 
                return; 
            }

            _target = target;
            State = CreatureState.Moving;
        }

        // TODO: SKILL 종류에 따른 range 수정
        int skillRange = 1;
        long nextMoveTick = 0;
        protected virtual void UpdateMoving()
        {
            if (nextMoveTick > Environment.TickCount64)
                return;

            int moveTick = (int)(1000 / Speed);
            nextMoveTick = Environment.TickCount64 + moveTick;

            if(_target == null || _target.Room != Room)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            Vector2Int dir = _target.CellPosition - CellPosition;
            int dist = dir.cellDistFromZero;
            if(dist == 0 || dist > chaseCellDist)
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            List<Vector2Int> path = Room.Map.FindPath(CellPosition, _target.CellPosition, checkObjects: false);
            if(path.Count < 2 || path.Count > chaseCellDist) 
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
                return;
            }

            // 스킬
            if(dist <= skillRange && (dir.x == 0 || dir.y == 0))
            {
                _coolTick = 0;
                State = CreatureState.Skill;
                return;
            }

            // 이동
            Dir = GetDirFromVector(path[1] - CellPosition);
            Room.Map.ApplyMove(this, path[1]);
            BroadcastMove();
        }

        void BroadcastMove()
        {
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PositionInfo = PositionInfo;
            Room.Broadcast(movePacket);
        }

        long _coolTick = 0;

        protected virtual void UpdateSkill()
        {
            if (_coolTick == 0)
            {
                //유효한 타겟인가
                if (_target is null || _target.Room != Room || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                //스킬이 아직 사용 가능한가
                Vector2Int dir = (_target.CellPosition - CellPosition);
                int dist = dir.cellDistFromZero;
                bool canUseSkill = (dist <= skillRange && (dir.x == 0 || dir.y == 0));
                if(canUseSkill == false)
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }

                //타게팅 방향 주시
                MoveDir lookDir = GetDirFromVector(dir);
                if(Dir != lookDir)
                {
                    Dir = lookDir;
                    BroadcastMove();
                }

                Skill skillData = null;
                DataManager.SkillDict.TryGetValue(1, out skillData);

                //데미지 판정
                _target.OnDamaged(this, skillData.damage + Stat.Attack);

                //스킬 사용 Broadcast
                S_Skill skill = new S_Skill() { Info = new SkillInfo() };
                skill.ObjectId = Id;
                skill.Info.SkillId = skillData.id;
                Room.Broadcast(skill);

                //스킬 쿨타임 적용
                int coolTick = (int)(1000 * skillData.cooldown);
                _coolTick = Environment.TickCount64 + coolTick;
            }

            if (_coolTick > Environment.TickCount64)
                return;

            _coolTick = 0;
        }

        protected virtual void UpdateDead()
        {

        }


    }
}
