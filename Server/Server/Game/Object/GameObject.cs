using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }
        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public PositionInfo PositionInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public float Speed
        {
            get { return Stat.Speed; } 
            set { Stat.Speed = value; }
        }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
        }

        public MoveDir Dir
        {
            get { return PositionInfo.MoveDir; }
            set { PositionInfo.MoveDir = value; }
        }

        public CreatureState State
        {
            get { return PositionInfo.State; }
            set { PositionInfo.State = value; }
        }


        public GameObject()
        {
            Info.PositionInfo = PositionInfo;
            Info.StatInfo = Stat;
        }

        public virtual void Update()
        {

        }

        public Vector2Int CellPosition
        {
            get
            {
                return new Vector2Int(PositionInfo.PosX, PositionInfo.PosY);
            }

            set
            {
                PositionInfo.PosX = value.x;
                PositionInfo.PosY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PositionInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPos(MoveDir dir)
        {
            Vector2Int cellPosition = CellPosition;

            switch (dir)
            {
                case MoveDir.Up:
                    cellPosition += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPosition += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPosition += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPosition += Vector2Int.right;
                    break;
            }

            return cellPosition;
        }

        public static MoveDir GetDirFromVector(Vector2Int dir)
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

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room is null)
            {
                return;
            }

            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(changePacket);

            if (Stat.Hp <= 0)
            {
                Vector2Int position = new Vector2Int(PositionInfo.PosX, PositionInfo.PosY);
                OnDead(attacker, position);
            }
        }

        public virtual void OnDead(GameObject attacker, Vector2Int position)
        {
            if(Room is null)
            {
                return;
            }
            
            
            GameRoom room = Room;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(diePacket);

            room.LeaveGame(Id);
            
            Item item = ObjectManager.Instance.Add<Item>();
            item.CellPosition = new Vector2Int(position.x, position.y);
            room.Push(room.EnterGame, item);

            Stat.Hp = Stat.MaxHp;
            PositionInfo.State = CreatureState.Idle;
            PositionInfo.MoveDir = MoveDir.Right;
            PositionInfo.PosX = 0; 
            PositionInfo.PosY = 0;

            room.Push(room.EnterGame, this);
        }
        
    }
}
