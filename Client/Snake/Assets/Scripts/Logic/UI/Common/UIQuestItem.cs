using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.UI;

public class UIQuestItem : UIListItem 
{
    public Text lab_name;
    public Text lab_des;

    public override void UpdateItem(int index, object data)
    {
        lab_des.text = "";
        lab_name.text = "Quest " + index.ToString();
    }
}
