using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.Networking;

public class DBRoomData : MonoBehaviour
{
    private TMP_Text RoomInfoText;

    private DbRoomInfo _roomInfo;

    public DbRoomInfo DbRoomInfo
    {
        get 
        {
            return _roomInfo;
        }
        set 
        {
            _roomInfo = value;
            // ex) room_03 (1/2)
            RoomInfoText.text = $"{_roomInfo.RoomName} ({_roomInfo.PlayersCount} /10)";

            // 버튼의 클릭 이벤트에 함수를 연결
            GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => OnEnterRoom(_roomInfo.RoomName));
        }
    }

    void Awake()
    {
        RoomInfoText = GetComponentInChildren<TMP_Text>();
        
    }

    // 방 참가 또는 방 만들기
    public void OnEnterRoom(string roomName)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("roomName", roomName);

        PhotonNetwork.WebRpc("room/webRpc/checkCurRoom", parameters);
    }
}