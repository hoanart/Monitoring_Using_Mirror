using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerList : NetworkBehaviour
{
   public static readonly List<Player> playersList = new List<Player>();
   [Tooltip("보고싶은 플레이어 선택을 위해 버튼들을 모아둔 패널이다.")]
   public GameObject buttonPanel;
   private ControlButton controlButton;
   
   private int buttonNum = 0;
   private int listIdx =0;

   private void Awake()
   {
      controlButton = buttonPanel.GetComponent<ControlButton>();
   }

   public void Start()
   {
      for (int i = 0; i < controlButton.buttonObjects.Length; i++)
      {
         int temp = i;
         controlButton.buttonObjects[i].GetComponent<Button>().onClick
            .AddListener(() => { OnClickChangeRenderCamera(temp);});
         
      }
   }

   private void Update()
   {
      // for (int i = 0; i < playersList.Count; i++)
      // {
      //    if (playersList[i] == null)
      //    {
      //       playersList.Remove(playersList[i]);
      //    }
      // }
   }

   /// <summary>
   /// 서버에서의 버튼 클릭시 보려고 하는 플레이어의 렌더카메라를 작동시킨다.
   /// </summary>
   /// <param name="idx"></param>
   void OnClickChangeRenderCamera(int idx)
   {
      if (isServer)
      {
         try
        {
           RpcActiveRenderCam(idx,playersList[idx].playerID);
        }
        catch (Exception e)
        {
           Debug.Log("해당되는 클라이언트가 없습니다. ");
        }
        
      }
   }

   #region ClientRpc
   /// <summary>
   /// ID와 idx에 위치한 playersList의 요소가 가진 playerID가 동일하다면 그 요소 하위의 렌더카메라 오브젝트를 활성화 하여
   /// 클라이언트로 보낸다.
   /// </summary>
   /// <param name="idx">버튼의 인덱스와 동일한 playersList의 인덱스 </param>
   /// <param name="ID">서버화면에 비출 클라이언트의 ID와 비교를 위한 클라이언트의 ID</param>
   [ClientRpc]
   void RpcActiveRenderCam(int idx,uint ID)
   {
      //Debug.Log($"ID : {ID}, playerId : {playersList[idx].playerID}");
      for (int i = 0; i < playersList.Count; i++)
      {
         playersList[i].renderCam.gameObject.SetActive(false);
      }
      
      if (ID == playersList[idx].playerID)
      {
         playersList[idx].renderCam.gameObject.SetActive(true);
      }
   }
   #endregion
}
