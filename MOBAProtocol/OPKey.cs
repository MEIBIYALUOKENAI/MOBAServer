/*-------------------------------------------------------------------------
* 命名空间名称/文件名:    MOBAProtocol/OPKey 
* 功 能：       N/A
* 类 名：       OPKey
* 创建时间：  2024/8/12 23:15:31
* 创建人:        Meibiyaluokenai
*-------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace MOBAProtocol
{
    [Serializable]
    public enum KeyType
    {
        None,
        Move,
        Skill,
        //TOADD
    }
    [Serializable]
    public class OpKey
    {
        public int opIndex;
        public KeyType keyType;
        public SkillKey skillKey;
        public MoveKey moveKey;
        //TOADD
    }

    [Serializable]
    public class SkillKey
    {
        public uint skillID;

        public long x_value;
        public long z_value;
    }

    [Serializable]
    public class MoveKey
    {
        //debug
        public uint keyID;

        public long x;
        public long z;
    }
}


