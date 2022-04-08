using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour {
    [Tooltip("플레이어의 렌더카메라")]
    public Camera renderCam;
    public SendTexture sender;
    [Tooltip("텍스처 전송 오브젝트 이름")]
    public string senderName = "Sender";
    [Tooltip("플레이어ID")]
    public uint playerID;

    private bool mbAddPlayer;
    public override void OnStartClient()
    {
        sender = GameObject.Find(senderName).GetComponent<SendTexture>();
        playerID = gameObject.GetComponent<NetworkIdentity>().netId;
        Debug.Log(playerID);
        PlayerList.playersList.Add(this);
        CmdAddList(this,playerID);
        mbAddPlayer = true;
    }

    public void Update()
    {
        if (isLocalPlayer)
        {
            sender.isActiveRenderCam = renderCam.gameObject.activeSelf;
            
            // if (!mbAddPlayer)
            // {
            //     
            // }
        }
        
    }
    

    // public override void OnStopClient()
    // {
    //     CmdRemoveList(this);
    //     PlayerList. playersList.Remove(this);
    //     
    // }
    
    #region Command
    /// <summary>
    /// 서버에 있는 PlayerList.playerList에 로컬 플레이어를 추가한다.
    /// </summary>
    /// <param name="player">로컬 플레이어</param>
    /// <param name="id">추적을 위한 로컬 플레이어 netId</param>
    [Command]
    void CmdAddList(Player player,uint id)
    {
        player.playerID = id;
        PlayerList.playersList.Add(player);
    }
    /// <summary>
    /// 클라이언트를 끝낼 때 PlayerList.playersList에서 로컬 플레이어를 제거한다.
    /// </summary>
    /// <param name="player"></param>
    [Command]
    void CmdRemoveList(Player player)
    {
        PlayerList.playersList.Remove(player);
    }
    #endregion
    
    
}
