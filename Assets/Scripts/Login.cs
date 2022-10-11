using Google;
using Firebase.Auth;

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
public class Login : MonoBehaviour
{   
    string json;

     private Queue<UnityAction> actionBuffer = new Queue<UnityAction>();

     public TMP_InputField NicknameInput;

     public static string nickname;

     // Firestore 에 저장할 user data
    public class userLoginData
    {
        // 로그인 타입 설정
        public enum LoginType
        {
            guest = 0,
            coupon = 1
        }
        public string nickname;
        public LoginType loginType;
        public string uid;
        public string deviceModel;
        public string deviceName;
        public UnityEngine.DeviceType deviceType;
        public string deviceOS;
        // public ulong createDate;
        public string createDate;
        public string lastVisit;
    }

	// Auth 용 instance
    FirebaseAuth auth = null;

    // 사용자 계정
    public static FirebaseUser user = null;

    public GameObject OnCouponPanelButton;

    // 기기 연동이 되어 있는 상태인지 체크한다.
    private bool signedIn = false;
    
    private void Awake()
    {
        // 초기화
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        // 유저의 로그인 정보에 어떠한 변경점이 생기면 실행되게 이벤트를 걸어준다.
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void Update()
    {
        if(actionBuffer.Count > 0) {
            actionBuffer.Dequeue().Invoke();
        }
    }

    
    // 계정 로그인에 어떠한 변경점이 발생시 진입.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            // 연동된 계정과 기기의 계정이 같다면 true를 리턴한다. 
            signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                UnityEngine.Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                UnityEngine.Debug.Log("Signed in " + user.UserId);
            }
        }
    }
    
    public void AnonyLogin()
    {   
        nickname = NicknameInput.text;

        Debug.Log("로그인 진행");
        // 익명 로그인 진행
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            // 익명 로그인 연동 결과
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            loginDataSave(userLoginData.LoginType.guest, nickname);
        });
        
    }
    
    // 연동 해제
    public void SignOut()
    {
        if (auth.CurrentUser != null)
            auth.SignOut();
    }

    // 연동 계정 삭제
    public void UserDelete()
    {
        if (auth.CurrentUser != null)
            auth.CurrentUser.DeleteAsync();
    }

    // 신규 유저 데이터 입력
    private void loginDataSave(userLoginData.LoginType loginType, string nickname = null)
    {
        Debug.Log("loginType : " + loginType);

        // 유저 데이터
        var newUser = new userLoginData();

        // DB에 저장될 유저데이터 초기화
        newUser.loginType = loginType;
        newUser.nickname = nickname;
        newUser.uid = user.UserId;
        newUser.lastVisit = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        newUser.createDate = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

        actionBuffer.Enqueue(
            delegate
            {
                newUser.deviceModel = SystemInfo.deviceModel;
                newUser.deviceName = SystemInfo.deviceName;
                newUser.deviceType = SystemInfo.deviceType;
                newUser.deviceOS = SystemInfo.operatingSystem;
                // newUser.createDate = auth.CurrentUser.Metadata.CreationTimestamp;
                json = JsonUtility.ToJson(newUser);

                // 새로운 유저에 대한 데이터를 DB에 보낸다.
                UserJsonDBSave(json);
            }
        );
        
    }

    // 유저 데이터(json)을 서버에 저장한다.
    public void UserJsonDBSave(string json)
    {
        Debug.Log("신규 유저 데이터 저장");
        try
        {
            actionBuffer.Enqueue(
                delegate
                {   
                    // StartCoroutine(JsonDBSavePost("http://localhost:5000/user/save", json)); 
                    StartCoroutine(JsonDBSavePost("https://clawmachinegame-beaa6.web.app/user/save", json)); 
                }
            );
        }
        catch (System.Exception err)
        {
            Debug.Log(err);
        }
    }
    private IEnumerator JsonDBSavePost(string url, string json)
    {
        using (var uwr = new UnityWebRequest(url, "POST"))
        {
            Debug.Log(json);

            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error While Sending: " + uwr.error);
            }
            else
            {
                var userData = bool.Parse(uwr.downloadHandler.text);
                Debug.LogFormat("Received: ({0}) {1}", url, userData);
                
                // Home SCENE 전환
                actionBuffer.Enqueue(delegate{
                                    SceneManager.LoadScene("HOME SCENE");
                                } );
                
            }

        }
    }
}