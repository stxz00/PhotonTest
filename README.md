# PhotonTest

# 필수 설치 사항 정리

# 1. Firebase 인증 SDK 설치

1) https://firebase.google.com/download/unity?hl=ko 다운로드 

2) Assets - Import Package - Custom Package 선택 후 dotnet4 폴더의 FirebaseAuth 임포트



# 2. 포톤 클라우드 사용을 위한  PUN2 설치 및 서버 적용

1) Asset Manager에서 Pun2 설치

2) Window - Photon Unity Networking - PUN Wizard 클릭

3) Setup Project 선택 후 b849895e-afdf-4753-8018-e732f8be57df 기입(테스트용 무료 포톤 Realtime 클라우드 앱 아이디)


# 3. 깃헙 공유 파일 내용

1) Google - google-services : Firebase 인증 사용을 위한 서비스 키

2) Keystore - user.keystore : 차후 Firebase 구글 인증 사용 시 적용할 keystore 파일

3) Prefabs - RoomEntity.prefab : 방 리스트의 방 정보 프리펩

4) Scripts 
    - PhotonManager : 포톤 상태 관리 및 WebRpc 요청 및 응답 데이터 검증.
    - DBRoomData : 위 3.3) 프리펩의 방 정보 및 포톤 방 접속 시도 스크립트.
    - DBRoomInfo : DBRoomData에 들어가는 방 정보.
    - CouponManage : 쿠폰 이용권 사용을 위한 서버 데이터 검증.
    - Login : 파이어베이스 게스트 로그인.
    

 
