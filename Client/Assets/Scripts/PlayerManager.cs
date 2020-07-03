using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerManager
{
    MyPlayer _myPlayer;
    Dictionary<int, Player> _players = new Dictionary<int, Player>();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(PlayerList packet)
    {
        // Prefab
        Object obj = Resources.Load("Player");

        foreach ( PlayerList.Player p in packet.players )
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            // 내 캐릭터
            if (p.isSelf)
            {
                MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                myPlayer.PlayerId = p.playerId;
                myPlayer.transform.position = new Vector3(p.position.posX, p.position.posY, p.position.posZ);
                _myPlayer = myPlayer;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.playerId;
                player.transform.position = new Vector3(p.position.posX, p.position.posY, p.position.posZ);
                _players.Add(p.playerId, player);
            }
        }
    }

    public void Move(BroadcastMove packet)
    {
        if (_myPlayer.PlayerId == packet.playerId)
        {
            _myPlayer.transform.position = new Vector3(packet.position.posX, packet.position.posY, packet.position.posZ);
        }
        else 
        {
            Player player = null;
            if (_players.TryGetValue(packet.playerId, out player))
            {
                player.transform.position = new Vector3(packet.position.posX, packet.position.posY, packet.position.posZ);
            }
        }
    }

    public void EnterGame(BroadcastEnterGame packet)
    {
        if (packet.playerId == _myPlayer.PlayerId)
            return;

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
