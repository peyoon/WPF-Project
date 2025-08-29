using System;

namespace WpfOlzServer.Network
{
    public enum E_HeadType : Int16
    {
        Raw = 0,
        JSON = 1,
        StringAscii = 2,
        StringUTF8 = 3
    }

    // NetworkCommands.cs (클라이언트와 서버 모두 동일해야 함)
    public enum E_OPCode : UInt16
    {
        UserCheck = 1,
        CS_PROFILE_UPDATE_REQ = 10, // "프로필 수정해주세요" 요청

        // ✨✨✨ [추가] 프로필 정보 요청/응답 Opcode ✨✨✨
        CS_PROFILE_GET_REQ = 11,    // "내 프로필 정보 주세요" 요청
        SC_PROFILE_GET_RES = 61,    // "여기 네 프로필 정보" 응답

        SC_LOGIN_SUCCESS = 51,
        SC_LOGIN_FAIL = 52,
        SC_PROFILE_UPDATE_RES = 60  // "프로필 수정 완료했습니다" 응답
    }
}