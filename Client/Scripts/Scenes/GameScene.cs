using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    private HpBar hpBar;
    public MyPlayerController MyPlayer { get; set; }
    
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        
        Managers.Map.LoadMap(1);
          
        Screen.SetResolution(640, 460, false);
        
        GameObject go = GameObject.Find("HpBar");
        hpBar = go.GetComponent<HpBar>();

        // GameObject player = Managers.Resource.Instantiate("Creature/Player");
        // player.name = "Player";
        // Managers.Object.Add(player);

        //Managers.UI.ShowSceneUI<UI_Inven>();
        //Dictionary<int, Data.Stat> dict = Managers.Data.StatDict;
        //gameObject.GetOrAddComponent<CursorController>();

        //GameObject player = Managers.Game.Spawn(Define.WorldObject.Player, "UnityChan");
        //Camera.main.gameObject.GetOrAddComponent<CameraController>().SetPlayer(player);

        ////Managers.Game.Spawn(Define.WorldObject.Monster, "Knight");
        //GameObject go = new GameObject { name = "SpawningPool" };
        //SpawningPool pool = go.GetOrAddComponent<SpawningPool>();
        //pool.SetKeepMonsterCount(2);
    }

    public override void Clear()
    {
        
    }

    public void UpdateHpBar()
    {
        if (hpBar == null)
        {
            return;
        }

        float ratio = 0.0f;
        if (MyPlayer.Stat.MaxHp > 0)
        {
            ratio = ((float)MyPlayer.Hp / MyPlayer.Stat.MaxHp);
        }
        
        hpBar.SetHpBar(ratio);
    }
}
