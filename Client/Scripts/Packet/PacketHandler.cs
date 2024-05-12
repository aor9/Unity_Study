using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
	}
	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
		Managers.Object.Clear();
	}
	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
		
		foreach (ObjectInfo obj in spawnPacket.Objects)
		{
			Managers.Object.Add(obj, myPlayer: false);
		}
	}
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;

		foreach (var id in despawnPacket.ObjectIds)
		{
			Managers.Object.Remove(id);
		}
	}
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;

		GameObject unit = Managers.Object.FindById(movePacket.ObjectId);
		if (unit == null)
		{
			return;
		}

		if (Managers.Object.MyPlayer.Id == movePacket.ObjectId)
		{
			return;
		}

		BaseController baseController = unit.GetComponent<BaseController>();
		if (baseController == null)
		{
			return;
		}

		baseController.PositionInfo = movePacket.PositionInfo;
	}
	
	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
		
		GameObject unit = Managers.Object.FindById(skillPacket.ObjectId);
		if (unit is null)
		{
			return;
		}

		CreatureController creatureController = unit.GetComponent<CreatureController>();
		if (creatureController is not null)
		{
			creatureController.UseSkill(skillPacket.Info.SkillId);
		}
	}
	
	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp hpPacket = packet as S_ChangeHp;
		
		GameObject unit = Managers.Object.FindById(hpPacket.ObjectId);
		if (unit is null)
		{
			return;
		}

		CreatureController creatureController = unit.GetComponent<PlayerController>();
		if (creatureController is not null)
		{
			creatureController.Hp = hpPacket.Hp;
		}
	}
	
	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
		
		GameObject unit = Managers.Object.FindById(diePacket.ObjectId);
		if (unit is null)
		{
			return;
		}

		CreatureController creatureController = unit.GetComponent<CreatureController>();
		if (creatureController is not null)
		{
			creatureController.Hp = 0;
			creatureController.OnDead();
		}
	}
}
