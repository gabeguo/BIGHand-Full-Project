using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IDataRepository
{
    bool UsernameInUse(string username);
    bool CreateUser(string username, string password);
    bool SignUserIn(string username, string password);

    void SignOut();
    void CloseConnection();
    void AddData(IGameSessionData data);

    User GetCurrentUser();

    IGameSessionData[] GetSessionData(string game, DateTime date);
    IGameSessionData[] GetSessionData(string game, DateTime startDate, DateTime endDate);
    IGameSessionData[] GetSessionData(string game, int numPreviousSessions);
    IGameSessionData[] GetLastSessionData(string game);
}
