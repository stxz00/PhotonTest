using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using Photon.Pun;
using Photon.Realtime;

public class CouponManage : MonoBehaviour
{
    public GameObject CouponPanel;
    public GameObject RoomListPanel;

    public TMP_InputField CouponInput;

    // 쿠폰 패널 열기
    public void OpenCouponPanel() 
    {
        RoomListPanel.SetActive(false);
        CouponPanel.SetActive(true);
    }

    // 쿠폰 유효 검사
    public void ValidateCoupon()
    {   
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("couponId", CouponInput.text);
        parameters.Add("uid", Login.user.UserId);
        PhotonNetwork.WebRpc("coupon/webRpc/validate", parameters);
    }


}
