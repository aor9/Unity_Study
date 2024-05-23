using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        Dictionary<int, Player> players = new Dictionary<int, Player>();
        Dictionary<int, Monster> monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
        Dictionary<int, Item> items = new Dictionary<int, Item>();

        // 이 게임룸의 맵
        public Map Map { get; private set;} = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);

            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.CellPosition = new Vector2Int(4, 5);
            Push(EnterGame, monster);
        }

        public void Update()
        {
            foreach(Monster monster in monsters.Values)
            {
                monster.Update();
            }

            foreach(Projectile projectile in projectiles.Values)
            {
                projectile.Update();
            }

            Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null)
            {
                return;
            }

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if(type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                players.Add(gameObject.Id, player);
                player.Room = this;

                Map.ApplyMove(player, new Vector2Int(player.CellPosition.x, player.CellPosition.y));

                //본인한테 정보 전송
                {
                    S_EnterGame enterPacekt = new S_EnterGame();
                    enterPacekt.Player = player.Info;
                    player.Session.Send(enterPacekt);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in players.Values)
                    {
                        if (player != p)
                        {
                            spawnPacket.Objects.Add(p.Info);
                        }
                    }

                    foreach(Monster m in monsters.Values)
                    {
                        spawnPacket.Objects.Add(m.Info);
                    }

                    foreach(Projectile p in projectiles.Values)
                    {
                        spawnPacket.Objects.Add(p.Info);
                    }

                    foreach (Item i in items.Values)
                    {
                        spawnPacket.Objects.Add(i.Info);
                    }

                    player.Session.Send(spawnPacket);
                }
            }
            else if(type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                monsters.Add(gameObject.Id, monster);
                monster.Room = this;
                Map.ApplyMove(monster, new Vector2Int(monster.CellPosition.x, monster.CellPosition.y));
            }
            else if(type == GameObjectType.Projectile)
            { 
                Projectile projectile = gameObject as Projectile;
                projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;
            }
            else if (type == GameObjectType.Item)
            {
                Item item = gameObject as Item;
                items.Add(gameObject.Id, item);
                item.Room = this;
            }

            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.Info);
                foreach(Player p in players.Values)
                {
                    if(p.Id != gameObject.Id)
                    {
                        p.Session.Send(spawnPacket);
                    }
                }
            }         
        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if(type == GameObjectType.Player)
            {
                Player player = null;
                if (players.Remove(objectId, out player) == false)
                    return;

                Map.ApplyLeave(player);
                player.Room = null;

                //본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if(type == GameObjectType.Monster)
            {
                Monster monster = null;
                if(monsters.Remove(objectId, out monster) == false)
                    return;

                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if(type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if (projectiles.Remove(objectId, out projectile) == false)
                    return;

                projectile.Room = null;
            }
            else if (type == GameObjectType.Item)
            {
                Item item = null;
                if (items.Remove(objectId, out item) == false)
                    return;

                item.Room = null;
            }

            //타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach(Player p in players.Values)
                {
                    if(p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if(player == null)
            {
                return;
            }

            //TODO: 검증 (Client 해킹 변조 위험이 있기 때문에 검증을 해야함)

            // 서버에서 좌표 이동
            PositionInfo movePosInfo = movePacket.PositionInfo;
            ObjectInfo info = player.Info;
                
            if(movePosInfo.PosX != info.PositionInfo.PosX || movePosInfo.PosY != info.PositionInfo.PosY) 
            {
                if (Map.IsMoveable(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
            }

            //아이템 확인
            HandleItem(player, movePosInfo);
            
            info.PositionInfo.State = movePosInfo.State;
            info.PositionInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            // 다른 플레이어에게 알림
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PositionInfo = movePacket.PositionInfo;

            Broadcast(resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if(player == null)
            {
                return;
            }

            ObjectInfo info = player.Info;
            if (info.PositionInfo.State != CreatureState.Idle)
            {
                return;
            }

            //TODO: 스킬 사용 가능 여부 체크

            info.PositionInfo.State = CreatureState.Skill;
            S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            skill.ObjectId = info.ObjectId;
            skill.Info.SkillId = 1;
            Broadcast(skill);

            Data.Skill skillData = null;
            if(DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
            {
                return;
            }

            switch (skillData.skillType)
            {
                case SkillType.SkillAuto:
                    {
                        //TODO: 데미지 판정
                        Vector2Int skillPosition = player.GetFrontCellPos(info.PositionInfo.MoveDir);
                        GameObject target = Map.Find(skillPosition);

                        if (target is not null)
                        {
                            Console.WriteLine("Hit GameObject !");
                        }
                    }
                    break;
                case SkillType.SkillProjectile:
                    {
                        // TODO : Bullet
                        Bullet bullet = ObjectManager.Instance.Add<Bullet>();
                        if (bullet is null)
                        {
                            return;
                        }

                        bullet.Owner = player;
                        bullet.Data = skillData;
                        bullet.PositionInfo.State = CreatureState.Moving;
                        bullet.PositionInfo.MoveDir = player.PositionInfo.MoveDir;
                        bullet.PositionInfo.PosX = player.PositionInfo.PosX;
                        bullet.PositionInfo.PosY = player.PositionInfo.PosY;
                        bullet.Speed = skillData.projectile.speed;
                        Push(EnterGame, bullet);
                    }
                    break;
            }
        }

        public void HandleItem(Player player, PositionInfo moveInfo)
        {
            foreach (var item in items.Values)
            {
                if (item.PositionInfo.PosX == moveInfo.PosX && item.PositionInfo.PosY == moveInfo.PosY)
                {
                    Console.WriteLine("아이템 발견");
                    Push(LeaveGame, item.Id);
                }
            }
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in players.Values)
            {
                if (condition.Invoke(player))
                {
                    return player;
                }
            }

            return null;
        }

        public void Broadcast(IMessage packet)
        {
            foreach(Player p in players.Values)
            {
                p.Session.Send(packet);
            }
        }
    }
}
