using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }
    private Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }
    
    // object를 game에 생성하는 메소드
    public void Add(ObjectInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.ObjectId);
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                Debug.Log($"myPlayerSpawn : {info.Name}");
                GameObject player = Managers.Resource.Instantiate("Creature/MyPlayer");
                player.name = info.Name;
                _objects.Add(info.ObjectId, player);

                MyPlayer = player.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.ObjectId;
                MyPlayer.PositionInfo = info.PositionInfo;
                MyPlayer.Stat = info.StatInfo;
                MyPlayer.SyncPosition();
                GameScene gameScene = GameObject.Find("@GameScene").GetComponent<GameScene>();
                gameScene.MyPlayer = MyPlayer;
                gameScene.UpdateHpBar();
            }
            else
            {
                if (_objects.ContainsKey(info.ObjectId))
                {
                    return;
                }
            
                GameObject go = Managers.Resource.Instantiate("Creature/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.ObjectId;
                pc.PositionInfo = info.PositionInfo;
                pc.Stat = info.StatInfo;
                pc.SyncPosition();
            }
        }
        else if (objectType == GameObjectType.Monster)
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Monster");
            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

            MonsterController monsterController = go.GetComponent<MonsterController>();
            monsterController.Id = info.ObjectId;
            monsterController.PositionInfo = info.PositionInfo;
            monsterController.Stat = info.StatInfo;
            monsterController.SyncPosition();
        }
        else if (objectType == GameObjectType.Projectile)
        {
            GameObject bullet = Managers.Resource.Instantiate("Creature/Bullet");
            bullet.name = "Bullet";
            _objects.Add(info.ObjectId, bullet);

            BulletController bulletController = bullet.GetComponent<BulletController>();
            bulletController.PositionInfo = info.PositionInfo;
            bulletController.Stat = info.StatInfo;
            bulletController.SyncPosition();
        }
    }
    public void Remove(int id)
    {
        GameObject unit = FindById(id);
        if(unit == null)
        {
            return;
        }
        
        _objects.Remove(id);
        Managers.Resource.Destroy(unit);
    }
    
    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject FindCreatre(Vector3Int cellPosition)
    {
        foreach (var obj in _objects.Values)
        {
            CreatureController creatureController = obj.GetComponent<CreatureController>();
            if (creatureController is null)
            {
                continue;
            }

            if (creatureController.CellPosition == cellPosition)
            {
                return obj;
            }
        }

        return null;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (var obj in _objects.Values)
        {
            CreatureController creatureController = obj.GetComponent<CreatureController>();
            if (creatureController is null)
            {
                continue;
            }

            if (condition.Invoke(obj))
            {
                return obj;
            }
        }
        
        return null;
    }
    
    public void Clear()
    {
        foreach (var obj in _objects.Values)
        {
            Managers.Resource.Destroy(obj);
        }
        
        _objects.Clear();
        MyPlayer = null;
    }
}
