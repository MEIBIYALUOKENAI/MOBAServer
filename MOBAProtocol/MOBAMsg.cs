using SUNet;
using System;
using System.Collections.Generic;

namespace MOBAProtocol
{
    [Serializable]
    public enum CMD
    {
        None = 0,
        BroadCast = 1,
        NetPing = 2,
        ReqLogin = 3, RspLogin = 4,
        ReqMatch = 5, RspMatch = 6,
        ReqCancel=20, RspCancel = 21,

        NtfConfirm = 7,SndConfirm = 8,

        NtfSelect = 9,SndSelect = 10,

        NtfLoadRes = 11,SndLoadPrg = 12,NtfLoadPrg = 13,

        ReqBattleStart = 14,RspBattleStart = 15,

        SndOpKey = 100,
        NtfOpKey = 101,

        //结算
        ReqBattleEnd = 201,
        RspBattleEnd = 202,

        //PING
        ReqPing = 214,
        RspPing = 215,
        //聊天
        SndChat = 203,
        NtfChat = 204,

        


        PengXiang=9999,

    }
    [Serializable]
    public enum ErrorCode
    {
        None = 0,
        AcctIsOnline = 1,
    }
    [Serializable]
    public class MOBAMsg : KCPMsg
    {
        public CMD cmd;
        public ErrorCode errorCode;
        public bool isEmpty;//不是空操作帧包
        public ReqLogin reqLogin;
        public RspLogin rsqLogin;
        public ReqMatch reqMatch;
        public RspMatch rspMatch;
        public NtfConfirm ntfConfirm;
        public SndConfirm sndConfirm;

        public SndSelect sndSelect;

        public NtfLoadRes ntfLoadRes;
        public SndLoadPrg sndLoadPrg;
        public NtfLoadPrg ntfLoadPrg;

        public ReqBattleStart reqBattleStart;
        public RspBatlleStart rspBatlleStart;

        public SndOpKey sndOpKey;
        public NtfOpKey ntfOpKey;

        public SndChat sndChat;
        public NtfChat ntfChat;

        public ReqBattleEnd reqBattleEnd;
        public RspBattleEnd rspBattleEnd;


        public ReqPing reqPing;
        public RspPing rspPing;

        public NetPing netPing;
        public string Info { get; set; }
    }
    #region 登录相关协议

    [Serializable]
    public class ReqLogin
    {
        public string acct;
        public string pass;
    }
    [Serializable]
    public class RspLogin
    {
        public UserData userData;
    }
    [Serializable]
    public class UserData
    {
        public uint Id;
        public string Name;
        public int Lv;
        public int Exp;
        public int Coin;
        public int Diamond;
        public int Ticket;

        public List<HeroSelectData> heroSelectDatas;

    }
    [Serializable]
    public class HeroSelectData
    {
        public int HeroID;
    }

    #endregion

    #region 匹配相关协议
    [Serializable]
    public enum PVPEnum
    {
        None = 0,
        _1V1 = 1,
        _2V2 = 2,
        _5V5 = 3
    }
    [Serializable]
    public class ReqMatch
    {
        public PVPEnum pvpEnum;
    }
    [Serializable]
    public class RspMatch
    {
        public int predictTime;
    }
    [Serializable]
    public class NtfConfirm
    {
        public uint roomID;
        public bool dissmiss;//解散
        public ConfirmData[] confirmArr;
    }
    [Serializable]
    public class ConfirmData
    {
        public int iconIndex;
        public bool confirmDone;
    }
    [Serializable]
    public class SndConfirm
    {
        public uint roomID;
    }
    #endregion

    #region 选择英雄相关协议
    [Serializable]
    public class SelectData
    {
        public int selectHeroID;
        public bool selectDone;
    }
    [Serializable]
    public class SndSelect
    {
        public uint roomID;
        public int heroID;
    }
    #endregion

    #region 加载界面相关协议
    [Serializable]
    public class NtfLoadRes
    {
        public int mapID;
        public List<BattleHeroData> heroList;
        public int posIndex;
    }
    [Serializable]
    public class BattleHeroData
    {
        public string userName;//玩家名字
        public int heroID;
        //级别，皮肤ID,边框，称号TODO
    }
    [Serializable]
    public class SndLoadPrg
    {
        public uint roomID;
        public int percent;
    }
    [Serializable]
    public class NtfLoadPrg
    {
        public List<int> percentLst;
    }
    #endregion

    #region 战斗相关协议
    [Serializable]
    public class ReqBattleStart
    {
        public uint roomID;
    }
    [Serializable]
    public class RspBatlleStart
    {

    }


    [Serializable]
    public class SndOpKey
    {
        public uint roomID;
        public OpKey opKey;
    }
    [Serializable]
    public class NtfOpKey
    {
        public uint frameID;
        public List<OpKey> keyList;
    }

    [Serializable]
    public class SndChat
    {
        public uint roomID;
        public string chatMsg;
    }
    [Serializable]
    public class NtfChat
    {
        public string chatMsg;
    }

    [Serializable]
    public class ReqBattleEnd
    {
        public uint roomID;
    }
    [Serializable]
    public class RspBattleEnd
    {
        //结算数据
    }

    [Serializable]
    public class ReqPing
    {
        public uint pingID;
        public ulong sendTime;
        public ulong backTime;
    }
    [Serializable]
    public class RspPing
    {
        public uint pingID;
    }
    #endregion



    [Serializable]
    public class NetPing
    {
        public bool IsOver;
    }

}
