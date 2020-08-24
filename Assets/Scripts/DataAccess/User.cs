using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    private string userName;
    private int userId;

    public User(string un, int uid)
    {
        userName = un;
        userId = uid;
    }

    public string GetUserName()
    {
        return userName;
    }

    public int GetUserId()
    {
        return userId;
    }
}
