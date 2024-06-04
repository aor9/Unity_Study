using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : BaseScene
{
    private HpBar hpBar;
    public MyPlayerController MyPlayer { get; set; }
    public TMP_Text text;
    private int gold = 0;
    
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;
        
        Managers.Map.LoadMap(1);
          
        Screen.SetResolution(640, 460, false);
        
        GameObject go = GameObject.Find("HpBar");
        hpBar = go.GetComponent<HpBar>();
        
        SetText();
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

    public void SetText()
    {
        text.text = gold.ToString();
    }

    public void UpdateGold()
    {
        gold += 5;
        SetText();
    }
}
