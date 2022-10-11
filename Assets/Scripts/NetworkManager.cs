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

public class NetworkManager : MonoBehaviourPunCallbacks
{	
	string networkState;

    public static bool isGm = false;

	// 방 접속 플레이어 리스트
	public Text PlayerList;

	// 방 접속 플레이어 수
	public Text PlayerCount;

	// 삭제 예정
	//public Button OpenCreateRoomPanelButton;

	// 삭제 예정
	//public GameObject CreateRoomPanel;

	public GameObject RoomListPanel;

	public TMP_InputField CreateRoomNameInput;
	
	// 삭제 예정
	//public Button DeleteRoom;
	
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
		if (!PhotonNetwork.IsConnected)
		{
			Debug.Log("Login.user : " + Login.user.ToString());
			Debug.Log("Metadata : " + Login.user.Metadata);
			Debug.Log("Metadata : " + Login.user.IsAnonymous);
			Debug.Log("Metadata : " + Login.user.ProviderData);
			Debug.Log("ProviderId : " + Login.user.ProviderId);


			// (포톤 서버만 가능하고 Realtime, Pun2 등에서는 불가능)포톤 커스텀 인증 후 포톤 서버 접속
			//AuthenticationValues authValues = new AuthenticationValues();
			//authValues.AuthType = CustomAuthenticationType.Custom;
			//authValues.AddAuthParameter("user", Login.user.UserId);
			// authValues.AddAuthParameter("pass", pass);
			// this is required when you set UserId directly from client and not from web service
			//authValues.UserId = Login.user.UserId; 
			//PhotonNetwork.AuthValues = authValues;

			PhotonNetwork.ConnectUsingSettings();

			// PhotonNetwork.ConnectToMaster("url",5055, "");
			//Debug.Log(PhotonNetwork.AuthValues);

		}
		
		// actionBuffer.Enqueue(
        //     delegate
        //     {	
		// 		IsGmLogin();
		// 	}
		// );


	}

	//class GmData 
	//{
	//	public string uid;
	//	public string photonUid;
	//}
	//private void IsGmLogin()
    //{
	//	GmData gmData = new GmData();
	//	gmData.uid = PhotonNetwork.AuthValues.UserId;
	//	gmData.photonUid = PhotonNetwork.AuthValues.UserId;

	//	string json = JsonUtility.ToJson(gmData);
        // StartCoroutine(ClientIsGm("http://localhost:5000/user/isGm", json));
        // StartCoroutine(ClientIsGm("https://clawmachinegame-beaa6.web.app/user/isGm", json)); 
    //}

    void Update()
    {
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

    public override void OnConnectedToMaster() 
	{
		Debug.Log("서버 연결");
		PhotonNetwork.JoinLobby();
		Debug.Log("Photon Server에서 만든 유저 인증 아이디 : " + PhotonNetwork.AuthValues.UserId);
	}


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

	void RoomListUpdate()
    {
		//StartCoroutine(GetRoomList("localhost:5000/room/curRooms"));
		//StartCoroutine(GetRoomList("https://clawmachinegame-beaa6.web.app/room/curRooms"));
		PhotonNetwork.WebRpc("room/webRpc/curRooms", "");
	}

	// 방 리스트 콜백은 로비에 접속했을때 자동으로 호출.
	public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
		// 실제 포톤 서버의 방 리스트를 가져오는 코드

        Debug.Log($"방 리스트 업데이트 ::::::: 현재 방 갯수 : {roomList.Count}");
		//Debug.Log($"기존 방 갯수 : {roomDict.Count}");
		//GameObject tempRoom = null;
		//foreach (var room in roomList)
		//{
			// 룸이 삭제된 경우
		//	if (room.RemovedFromList == true)
		//	{
		//		Debug.Log("방 삭제");
		
        //        roomDict.TryGetValue(room.Name, out tempRoom);
		//		Destroy(tempRoom);
		//		roomDict.Remove(room.Name);
		//	}
		//	else
		//	{
		//		if (roomDict.ContainsKey(room.Name) == false)
		//		{
		//			Debug.Log("룸 정보 변경");
					// 룸 정보가 갱신(변경)된 경우
		//			GameObject _room = Instantiate(roomPrefab, scrollContent);
		//			_room.GetComponent<RoomData>().RoomInfo = room;
		//			roomDict.Add(room.Name, _room);
		//		}
		//		else
		//		{
        //            Debug.Log("룸 정보 갱신");
                    //룸 정보를 갱신하는 경우
        //            roomDict.TryGetValue(room.Name, out tempRoom);
		//			tempRoom.GetComponent<RoomData>().RoomInfo = room;
		//		}
		//	}
		//}
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

		// 아래삭제 예정
		//운영자가 아닌 마스터 클라이언트가 GM 접속시 마스터 클라이언트 위임
		//if (PhotonNetwork.IsMasterClient && isGm == false) 
		//{
		//	GmData gmData = new GmData();
		//	gmData.uid = Login.user.UserId;
		//	gmData.photonUid = newPlayer.UserId;
		//
		//	string json = JsonUtility.ToJson(gmData);
		//	// StartCoroutine(IsplayerGm("http://localhost:5000/user/isGmClient", json, newPlayer));
		//	StartCoroutine(IsplayerGm("https://clawmachinegame-beaa6.web.app/user/isGmClient", json, newPlayer)); 
		//}


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

	

	// 삭제 예정
	// 방 접속 플레이어들을 모두 나가게 하여 방 삭제
	 public void AllPlayerLeaveRoom() {
		if (PhotonNetwork.IsMasterClient) 
		{
			Debug.Log("방 삭제 진행");
			foreach (Player player in PhotonNetwork.PlayerListOthers)
			{
				bool result =  PhotonNetwork.CloseConnection(player);
			}

			PhotonNetwork.LeaveRoom();
		}
	 }   
	

	// 삭제 예정
	//private IEnumerator ClientIsGm(string url, string json)
    //{
    //    using (var uwr = new UnityWebRequest(url, "POST"))
    //    {
    //        Debug.Log(json);
	//
    //        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
    //        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
    //        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    //        uwr.SetRequestHeader("Content-Type", "application/json");
    //        yield return uwr.SendWebRequest();
    //        if (uwr.result == UnityWebRequest.Result.ConnectionError)
    //        {
    //            Debug.Log("Error While Sending: " + uwr.error);
    //        }
    //        else
    //        {
    //            var result = bool.Parse(uwr.downloadHandler.text);
    //            Debug.LogFormat("Received: ({0}) {1}", url, uwr.downloadHandler.text);

	//			if (result) 
	//			{
	//				isGm = true;
	//				if (OpenCreateRoomPanelButton != null)
	//				OpenCreateRoomPanelButton.gameObject.SetActive(true);

	//				if (DeleteRoom != null)
	//				DeleteRoom.gameObject.SetActive(true);
	//			} else 
	//			{
	//				isGm = false;
	//				if (OpenCreateRoomPanelButton != null)
	//				OpenCreateRoomPanelButton.gameObject.SetActive(false);

	//				if (DeleteRoom != null)
	//				DeleteRoom.gameObject.SetActive(false);
	//			}
    //        }

    //    }
    //}

	//삭제 예정
	//private IEnumerator IsplayerGm(string url, string json, Player newPlayer)
    //{
    //    using (var uwr = new UnityWebRequest(url, "POST"))
    //    {
    //        Debug.Log(json);

    //        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
    //        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
    //        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    //        uwr.SetRequestHeader("Content-Type", "application/json");
    //        yield return uwr.SendWebRequest();
    //        if (uwr.result == UnityWebRequest.Result.ConnectionError)
    //        {
    //            Debug.Log("Error While Sending: " + uwr.error);
    //       }
    //        else
    //        {
    //            var result = bool.Parse(uwr.downloadHandler.text);
    //            Debug.LogFormat("Received: ({0}) {1}", url, result);

	//			if (result) 
	//			{
	//				Debug.Log("마스터 클라이언트 위임");
	//				PhotonNetwork.SetMasterClient(newPlayer);
	//			} 
    //        }

    //    }
    //}


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

	//삭제 예정
	//public void OpenCreateRoomPanel()
	//{
	//	CreateRoomPanel.SetActive(true);
	//}

	//삭제 예정
	//public void CloseCreateRoomPanel()
	//{
	//	CreateRoomPanel.SetActive(false);
	//}

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
				ro.MaxPlayers = 20;
				ro.PublishUserId = true;
				ro.EmptyRoomTtl = 1000 * 60;

				PhotonNetwork.JoinOrCreateRoom(roomName, ro, null);

				// roomName : 룸 이름
				// RoomOptions : 룸 목록 표시 여부, 인원 수 제한 20명(대기자 리스트, 관전 리스트 인원 책정해야함), 재접속 타임아웃 10초
				// typedLobby : null이면 현재 사용되고 있는 로비에서 자동 생성  
				// PhotonNetwork.CreateRoom(roomName, new RoomOptions { IsOpen = true, MaxPlayers = 20, EmptyRoomTtl = 10000 }, null);
				// PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { IsOpen = true, MaxPlayers = 20, EmptyRoomTtl = 10000 }, null);
				// roomNumber += 1;
			}
			else
			{
				Debug.Log("삭제된 방으로 입장 불가능");
			}

			return;
		}



		if (webRpcResponse.ResultCode != 0)
		{
			Debug.Log("WebRPC '" + webRpcResponse.Name + "' 에 실패했습니다. Error: " + webRpcResponse.ResultCode + " Message: " + webRpcResponse.Message);
			return;
		}
	}





}
