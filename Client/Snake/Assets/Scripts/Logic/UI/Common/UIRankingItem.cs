using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.UI;

public class UIRankingItem : UIListItem 
{
    public Text lab_rank;
    public Text lab_score;
    public Text lab_name;


    public override void UpdateItem(int index, object data)
    {
        
    }

    public override void Clear()
    {
        lab_rank.text = "";
        lab_score.text = "";
        lab_name.text = "";
    }
}
