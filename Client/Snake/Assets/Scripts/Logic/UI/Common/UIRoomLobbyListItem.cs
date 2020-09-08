using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.UI;
using GameProtocol;

public class UIRoomLobbyListItem : UIListItem
{
    public Text lab_PlayerInfo;
    public Text lab_Ready;

    public override void UpdateItem(int index, object data)
    {
        FSPPlayerData player = data as FSPPlayerData;
        lab_PlayerInfo.text = string.Format("{0}-{1}", player.name, player.userId);

        lab_Ready.enabled = player.isReady;
    }
}
