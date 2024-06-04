using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();
        Dictionary<int, Player> players = new Dictionary<int, Player>();


        // [UNUSED(1)][TYPE(7)][ID(24)] 비트
        //TODO
        int counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                if(gameObject.ObjectType == GameObjectType.Player)
                {
                    players.Add(gameObject.Id, gameObject as Player);
                }
            }

            return gameObject;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24 | (counter++));
            }
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if(objectType == GameObjectType.Player)
                {
                    return players.Remove(objectId);
                }
            }

            return false;
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    Player player = null;
                    if (players.TryGetValue(objectId, out player))
                    {
                        return player;
                    }
                }
            }

            return null;
        }
    }
}
