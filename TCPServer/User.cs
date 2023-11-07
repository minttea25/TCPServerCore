using Chat;
using System;
using System.Collections.Generic;

namespace ChatServer.Data
{
    [System.Serializable]
    public class User
    {


        public readonly uint Id;
        public readonly string UserName;
        public UserInfo UserInfo => _userInfo;
        readonly UserInfo _userInfo;

        public User(uint id, string userName)
        {
            Id = id;
            UserName = userName;
            _userInfo = new() { UserId = Id, UserName = UserName };
        }

        public override string ToString()
        {
            return $"User[name={UserName}, id={Id}]";
        }
    }
}