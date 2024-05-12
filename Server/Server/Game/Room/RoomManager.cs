using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get;} = new RoomManager(); 

        object _lock = new object();
        Dictionary<int, GameRoom> rooms = new Dictionary<int, GameRoom>();
        int roomId = 1;

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapId);

            lock(_lock)
            {
                gameRoom.RoomId = roomId;
                rooms[roomId] = gameRoom;
                roomId++;
            }

            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            lock(_lock)
            {
                return rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            lock (_lock)
            {
                GameRoom room = null;

                if(rooms.TryGetValue(roomId, out room))
                {
                    return room;
                }

                return null;
            }
        }
    }
}
