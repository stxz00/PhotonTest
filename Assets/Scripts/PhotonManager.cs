using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
using Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using TMPro;
using Google;
using Firebase.Auth;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PhotonManager : MonoBehaviourPunCallbacks
{	
	// 포톤 네트워크 상태 변경 시 디버그 로깅용
	string networkState;

    public static bool isGm = false;

	// 방 접속 플레이어 리스트
	public Text PlayerList;

	// 방 접속 플레이어 수
	public Text PlayerCount;

	// 방 리스트 패널
	public GameObject RoomListPanel;

	// 쿠폰 패널
	public GameObject CouponPanel;


	// 현재 접속중인 방에서 플레이어가 마지막 플레이어인 경우 true
	public bool lastClientBoo = false;


	// 룸 목록 저장 딕셔너리
	private Dictionary<string, GameObject> roomDict = new Dictionary<string, GameObject>();

	// 룸 표시 프리팹
	public GameObject roomPrefab;

	// 룸 프리팹 부모 객체
	public Transform scrollContent;

	// 재접속 방 이름 저장
    public string BackupRoomName
    {
        get => PlayerPrefs.GetString("BackupRoomName", "");
        set => PlayerPrefs.SetString("BackupRoomName", value);
    }

	void Awake()
	{
		// 포톤 연결
		if (!PhotonNetwork.IsConnected)
		{
			Debug.Log("Login.user : " + Login.user.ToString());
			Debug.Log("Metadata : " + Login.user.Metadata);
			Debug.Log("Metadata : " + Login.user.IsAnonymous);
			Debug.Log("Metadata : " + Login.user.ProviderData);
			Debug.Log("ProviderId : " + Login.user.ProviderId);

			PhotonNetwork.ConnectUsingSettings();

		}

	}

    void Update()
    {
		// 네트워크 상태 변경 확인
        string curNetworkState = PhotonNetwork.NetworkClientState.ToString();
        if (networkState != curNetworkState)
        {
            networkState = curNetworkState;
            print(networkState);
        }


		// 방 접속시 인원 및 명단
		if (PhotonNetwork.InRoom) {
			if (PlayerCount != null)
			{
				PlayerCount.text = "접속 인원 " + PhotonNetwork.CurrentRoom.PlayerCount + "명";
			}
			
			if (PlayerList != null) 
			{	
				PlayerList.text = "";
				foreach (Player player in PhotonNetwork.PlayerList)
				{
					PlayerList.text += ", " + player.NickName; 
				}
			}
		}
    }

	// 마스터 서버 연결됨
    public override void OnConnectedToMaster() 
	{
		Debug.Log("서버 연결");
		PhotonNetwork.JoinLobby();
		Debug.Log("Photon Server에서 만든 유저 인증 아이디 : " + PhotonNetwork.AuthValues.UserId);
	}
	
	// 로비 연결
	public override void OnJoinedLobby()
	{
		Debug.Log("로비 연결");
		// 재접속하는 경우 기존 방 접속 시도(타임아웃 시간 내)
		if (BackupRoomName != "")
		{
			PhotonNetwork.JoinRoom(BackupRoomName);
		}

		// 모든 클라이언트가 연결 해제를 가능하도록 해야 강퇴 등이 가능
		PhotonNetwork.EnableCloseConnection = true;

		// webrpc를 통하여 DB에서 현재 접속가능한 방 업데이트
		RoomListUpdate();
		
	}

	// 현재 접속가능한 방 리스트 DB에서 출력
	void RoomListUpdate()
    {
		PhotonNetwork.WebRpc("room/webRpc/curRooms", "");
	}

	// 방 리스트 콜백은 로비에 접속했을때 자동으로 호출.
	public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
		// 실제 포톤 서버의 방 리스트를 가져오는 코드
        Debug.Log($"방 리스트 업데이트 ::::::: 현재 방 갯수 : {roomList.Count}");
    }

	private void UpdatePlayerCounts()
    {
		Debug.Log("방 접속 인원 : "  + PhotonNetwork.CurrentRoom.PlayerCount + "명");
    }

	public override void OnCreatedRoom()
	{
		Debug.Log("방 생성 완료");
	}

    public override void OnJoinedRoom()
    {	
		// 게스트 닉네임 설정
		PhotonNetwork.LocalPlayer.NickName = Login.nickname; 

        //방 접속 시 재접속 가능할 수 있도록 방 이름 저장
		Debug.Log("방 입장 완료");
        BackupRoomName = PhotonNetwork.CurrentRoom.Name;

		//GAME SCENE 전환
		SceneManager.LoadScene("GAME SCENE");

		//네트워크 상에서 prefab 의 인스턴스를 생성
        // PhotonNetwork.Instantiate("RoomEntity", Vector2.zero, Quaternion.identity);
		UpdatePlayerCounts();

		if (PhotonNetwork.IsMasterClient)
        {
			Debug.Log("PhotonNetwork.CurrentRoom : " + PhotonNetwork.CurrentRoom);

			Debug.Log("PhotonNetwork.CurrentRoom.Name : " + PhotonNetwork.CurrentRoom.Name);
			Debug.Log("PhotonNetwork.CurrentRoom.PlayerCount : " + PhotonNetwork.CurrentRoom.PlayerCount);
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("roomName", PhotonNetwork.CurrentRoom.Name);
			parameters.Add("playersCount", PhotonNetwork.CurrentRoom.PlayerCount);
			PhotonNetwork.WebRpc("room/webRpc/updatePlayerCount", parameters);

			parameters.Add("maxPlayers", PhotonNetwork.CurrentRoom.MaxPlayers);
			parameters.Add("room", PhotonNetwork.CurrentRoom.MaxPlayers);
			parameters.Add("isOpen", PhotonNetwork.CurrentRoom.IsOpen);
			parameters.Add("isVisible", PhotonNetwork.CurrentRoom.IsVisible);
			parameters.Add("playerTtl", PhotonNetwork.CurrentRoom.PlayerTtl);
			parameters.Add("emptyRoomTtl", PhotonNetwork.CurrentRoom.EmptyRoomTtl);
			parameters.Add("autoCleanUp", PhotonNetwork.CurrentRoom.AutoCleanUp);
			parameters.Add("createPlayerName", PhotonNetwork.LocalPlayer.NickName);
			parameters.Add("lastUpdatePlayerName", PhotonNetwork.LocalPlayer.NickName);
			parameters.Add("createPlayerUid", PhotonNetwork.LocalPlayer.UserId);
			parameters.Add("lastUpdatePlayerUid", PhotonNetwork.LocalPlayer.UserId);


			PhotonNetwork.WebRpc("room/webRpc/test", parameters);
		}
		
	}

	public void leaveRoom() 
	{
		PhotonNetwork.LeaveRoom();
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
		Debug.Log("방 접속 실패");
    }


    // 모바일 끊김 문제로 나갔을 때 재접속
    public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("방 재접속");
		PhotonNetwork.Reconnect();
	}

	 public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"플레이어 {newPlayer.NickName} 방 참가.");
        UpdatePlayerCounts();
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		Debug.Log("해당 클라이언트가 마스터 클라이언트 위임 : " + newMasterClient.NickName);
		if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 1)
		{
			lastClientBoo = true;
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"플레이어 {otherPlayer.NickName} 방 나감.");
		UpdatePlayerCounts();

		if (PhotonNetwork.IsMasterClient)
        {
			Dictionary<string, object> parameters = new Dictionary<string, object>();
			parameters.Add("roomName", PhotonNetwork.CurrentRoom.Name);
			parameters.Add("playersCount", PhotonNetwork.CurrentRoom.PlayerCount);
			PhotonNetwork.WebRpc("room/webRpc/updatePlayerCount", parameters);
		}
	}
       

	// 방을 나갔을때 백업하지 않음
	public override void OnLeftRoom()
	{
		Debug.Log("방 접속 종료");

		//HOME SCENE 전환
		SceneManager.LoadScene("HOME SCENE");
		BackupRoomName = "";
	}

    // 어플을 종료시 백업하지 않음
    public void OnApplicationQuit()
	{
		Debug.Log("어플리케이션 종료");
		BackupRoomName = "";
	}

	// 방 리스트 업데이트
	private void GetRoomListUpdate(JObject applyJObj)
	{
		foreach (KeyValuePair<string, GameObject> keyValue in roomDict)
        {
			Destroy(keyValue.Value);
		}

		roomDict = new Dictionary<string, GameObject>();

		foreach (var dbRoomInfo in applyJObj["DbroomInfos"])
		{
			Debug.Log("dbRoomInfo : " + dbRoomInfo);

			DbRoomInfo dbRoom = new DbRoomInfo();
			dbRoom.RoomName = dbRoomInfo["RoomName"].ToString();
			dbRoom.PlayersCount = dbRoomInfo["PlayersCount"].ToString();

			GameObject _room = Instantiate(roomPrefab, scrollContent);
			_room.GetComponent<DBRoomData>().DbRoomInfo = dbRoom;
			roomDict.Add(dbRoom.RoomName, _room);
		}

		Invoke("RoomListUpdate", 5.0f);
	}

	//webRpc 응답 내용
	public override void OnWebRpcResponse(OperationResponse operationResponse)
	{
		Debug.Log("operationResponse.ReturnCode : " + operationResponse.ReturnCode);

		if (operationResponse.ReturnCode != 0)
		{
			Debug.Log("WebRPC에 실패했습니다. Response: " + operationResponse.ToStringFull());
			return;
		}

		WebRpcResponse webRpcResponse = new WebRpcResponse(operationResponse);
		Debug.Log("webRpcResponse.ResultCode : " + webRpcResponse.ResultCode);

		Debug.Log("RPC Name : " + webRpcResponse.Name);

		// 접속 가능한 방 리스트 호출
		if (webRpcResponse.Name == "room/webRpc/curRooms")
        {
			foreach (KeyValuePair<string, GameObject> keyValue in roomDict)
			{
				Destroy(keyValue.Value);
			}

			roomDict = new Dictionary<string, GameObject>();

			Dictionary<string, object> parameters = webRpcResponse.Parameters;

			foreach (KeyValuePair<string, object> pair in parameters)
			{
				Debug.Log(string.Format("Key : {0} ", pair.Key));

				string json = "{\"DbroomInfos\":" + pair.Value.ToString() + "}";
				JObject applyJObj = JObject.Parse(json);
				Debug.Log(applyJObj["DbroomInfos"]);
				GetRoomListUpdate(applyJObj);
			}

			return;
        }

		// 입장 전 방이 존재하는지(접속 가능한 방 리스트 갱신 전 DB상의 방이 삭제되었는지) 확인 후 접속
		if (webRpcResponse.Name == "room/webRpc/checkCurRoom")
		{
			Dictionary<string, object> parameters = webRpcResponse.Parameters;

			bool isExist = false;
			string roomName = "";

			foreach (KeyValuePair<string, object> pair in parameters)
            {
				Debug.Log(string.Format("Key : {0} / Value : {1}", pair.Key, pair.Value));
				if (pair.Key == "isExist") isExist = bool.Parse("" + pair.Value);
				if (pair.Key == "roomName") roomName = "" + pair.Value;
            }


			if (isExist && roomName != "")
			{
				// 룸 생성 시 옵션 설정
				RoomOptions ro = new RoomOptions();
				ro.IsOpen = true; // 방 공개 여부
				ro.IsVisible = true; // 룸 리스트에서 방 공개 여부
				ro.MaxPlayers = 20; // 방 접속 최대 인원 설정
				ro.PublishUserId = true; // uid 확인 가능
				ro.EmptyRoomTtl = 1000 * 60; // 빈 방 유지시간 설정 

				PhotonNetwork.JoinOrCreateRoom(roomName, ro, null);
			}
			else
			{
				Debug.Log("삭제된 방으로 입장 불가능");
			}

			return;
		}

		// 쿠폰 유효 검사
		if (webRpcResponse.Name == "coupon/webRpc/validate")
		{
			Dictionary<string, object> parameters = webRpcResponse.Parameters;

			bool result = false;
			string message = "";

			foreach (KeyValuePair<string, object> pair in parameters)
			{
				Debug.Log(string.Format("Key : {0} / Value : {1}", pair.Key, pair.Value));
				if (pair.Key == "result") result = bool.Parse("" + pair.Value);
				if (pair.Key == "message") message = "" + pair.Value;
			}

			if (result)
			{
				RoomListPanel.SetActive(true);
				CouponPanel.SetActive(false);
			}

			Debug.Log(message);

			return;
		}

		if (webRpcResponse.ResultCode != 0)
		{
			Debug.Log("WebRPC '" + webRpcResponse.Name + "' 에 실패했습니다. Error: " + webRpcResponse.ResultCode + " Message: " + webRpcResponse.Message);
			return;
		}
	}


}
