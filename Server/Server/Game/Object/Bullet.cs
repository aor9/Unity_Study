using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class Bullet : Projectile
    {
        public GameObject Owner { get; set; }

        long nextMoveTick = 0;

        public override void Update()
        {
            if (Data is null || Data.projectile is null || Owner is null || Room is null)
                return;

            if (nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long)(1000 / Data.projectile.speed);
            nextMoveTick = Environment.TickCount64 + tick;

            Vector2Int destPosition = GetFrontCellPos();
            if (Room.Map.IsMoveable(destPosition))
            {
                CellPosition = destPosition;

                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PositionInfo = PositionInfo;
                Room.Broadcast(movePacket);

                Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPosition);
                if(target is not null) 
                {
                    target.OnDamaged(this, Data.damage + Owner.Stat.Attack);
                }

                // 소멸
                Room.Push(Room.LeaveGame, Id);
            }
        }
    }
}
