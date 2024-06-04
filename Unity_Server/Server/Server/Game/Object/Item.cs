using Google.Protobuf.Protocol;

namespace Server.Game;

public class Item : GameObject
{
    public Item()
    {
        ObjectType = GameObjectType.Item;
    }
}