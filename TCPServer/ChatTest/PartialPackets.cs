using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTest
{
    public partial class S_ReqEnterRoom
    {
        partial void OnConstruction()
        {
            PType = PacketType.PSReqEnterRoom;
        }
    }

    public partial class S_LeaveRoom
    {
        partial void OnConstruction()
        {
            PType = PacketType.PSLeaveRoom;
        }
    }

    public partial class S_Chat
    {
        partial void OnConstruction()
        {
            PType = PacketType.PSChat;
        }
    }

    public partial class C_ResEnterRoom
    {
        partial void OnConstruction()
        {
            PType = PacketType.PCResEnterRoom;
        }
    }

    public partial class C_EnterRoom
    {
        partial void OnConstruction()
        {
            PType = PacketType.PCEnterRoom;
        }
    }

    public partial class C_LeaveRoom
    {
        partial void OnConstruction()
        {
            PType = PacketType.PCLeaveRoom;
        }
    }

    public partial class C_Chat
    {
        partial void OnConstruction()
        {
            PType = PacketType.PCChat;
        }
    }
}
