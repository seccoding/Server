using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(PlayerList packet)
    {
        Debug.Log("Add1");
        // Prefab
        Object obj = Resources.Load("Player");

        foreach ( PlayerList.Player p in packet.players )
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            // 내 캐릭터
            if (p.isSelf)
            {
                Debug.Log("Add2 Self");
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.position.posX, p.position.posY, p.position.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                Debug.Log("Add2 Other");
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.position.posX, p.position.posY, p.position.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    public void Move(BroadcastMove packet)
    {
        Debug.Log("Move1");
        if (_myPlayer.PlayerId == packet.playerId)
        {
            Debug.Log("Move2 Self");
            _myPlayer.transform.position = new Vector3(packet.position.posX, packet.position.posY, packet.position.posZ);
        }
        else 
        {
            Debug.Log("Move2 Other");
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                player.transform.position = new Vector3(packet.position.posX, packet.position.posY, packet.position.posZ);
            }
        }
    }

    public void EnterGame(BroadcastEnterGame packet)
    {
        Debug.Log("Enter1");
        if (packet.playerId == _myPlayer.PlayerId)
            return;

        Debug.Log("Enter2");
        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.transform.position = new Vector3(packet.position.posX, packet.position.posY, packet.position.posZ);
        _players.Add(packet.playerId, player);
    }

    public void LeaveGame(BroadcastLeaveGame packet)
    {
        if (_myPlayer.PlayerId == packet.playerId)
        {
            GameObject.Destroy(_myPlayer.gameObject);
            _myPlayer = null;
        }
        else
        {
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                _players.Remove(player.PlayerId);
            }
        }
    }

}
