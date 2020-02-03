using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;

    private bool newPlayer = true;


    private GameObject myCube;
    void Start()
    {
        myCube = Resources.Load("MyCube", typeof(GameObject)) as GameObject;
        udp = new UdpClient();
        udp.Connect("54.147.164.117", 12345);
        Debug.Log(((IPEndPoint)udp.Client.LocalEndPoint).Port.ToString());
        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
        udp.Send(sendBytes, sendBytes.Length);
        udp.BeginReceive(new AsyncCallback(OnReceived), udp);
        InvokeRepeating("HeartBeat", 1, 1);
       
    }

    void OnDestroy(){
        udp.Dispose();
    }

    public enum commands{
        NEW_CLIENT,
        UPDATE,
        OTHERPLAYER,
        DELETE
    };
    
    [Serializable]
    public class Message{
        public commands cmd;
        public Player[] players;
    }

   

    [Serializable]
    public class receivedColor{
        public float R;
        public float G;
        public float B;
    }
    
    [Serializable]
    public class serverPosition{
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class Player{
        public string id;

        public receivedColor color;    
        public serverPosition position;    
    }

    public Queue<Message> spawnMessages = new Queue<Message>();
    public object lockSpawn = new object();
    public Queue<Message> updateMessages = new Queue<Message>();
    public object lockUpdate = new object();
    public Queue<Message> deleteMessages = new Queue<Message>();
    public object lockDelete = new object();

    public Dictionary<string, GameObject> networkPlayers = new Dictionary<string, GameObject>();

    [Serializable]
    public class GameState{
        public Player[] players;
    }

    public Message latestMessage;
    public GameState lastestGameState;
    void OnReceived(IAsyncResult result){
        UdpClient socket = result.AsyncState as UdpClient;
        
        IPEndPoint source = new IPEndPoint(0, 0);

        byte[] message = socket.EndReceive(result, ref source);
        
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);
        latestMessage = JsonUtility.FromJson<Message>(returnData);

        try{
            switch(latestMessage.cmd){
                case commands.NEW_CLIENT:
                    lock(lockSpawn){
                        spawnMessages.Enqueue(latestMessage);
                    }
                    break;
                case commands.UPDATE:
                    lock(lockUpdate)
                    {
                        updateMessages.Enqueue(latestMessage);
                    }
                    break;
                case commands.OTHERPLAYER:
                    lock(lockSpawn){
                        spawnMessages.Enqueue(latestMessage);
                    }
                    break;
                case commands.DELETE:
                    lock(lockDelete){
                        deleteMessages.Enqueue(latestMessage);
                    }
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(){
        lock(lockSpawn)
        {
            while(spawnMessages.Count > 0){
                var spawnMessage = spawnMessages.Dequeue();
                for(int playerCount = 0; playerCount < spawnMessage.players.Length; playerCount++){
                    GameObject newCube = Instantiate(myCube,
                        new Vector3(
                            spawnMessage.players[playerCount].position.x,
                            spawnMessage.players[playerCount].position.y,
                            spawnMessage.players[playerCount].position.z
                        ), 
                        Quaternion.Euler(0, 0, 0)) as GameObject;
                    NetworkCube spawnCube = newCube.GetComponent<NetworkCube>();
                    spawnCube.id = spawnMessage.players[playerCount].id;
                    if((playerCount == spawnMessage.players.Length - 1) && newPlayer){
                        newPlayer = false;
                    }
                    spawnCube.ChangeColor(spawnMessage.players[playerCount].color.R, spawnMessage.players[playerCount].color.G, spawnMessage.players[playerCount].color.B);
                    networkPlayers.Add(spawnMessage.players[playerCount].id, newCube);
                }
            }
        }
    }

    void UpdatePlayers(){
        lock(lockUpdate){
            while(updateMessages.Count > 0){
                var updateMessage = updateMessages.Dequeue();
                for(int playerCounter = 0; playerCounter < updateMessage.players.Length; playerCounter++){
                    var cubeId = updateMessage.players[playerCounter].id;
                    if(networkPlayers.ContainsKey(cubeId)){
                        var currentCube = networkPlayers[cubeId];
                        currentCube.GetComponent<NetworkCube>()
                        .ChangeColor(updateMessage.players[playerCounter].color.R, 
                                     updateMessage.players[playerCounter].color.G, 
                                     updateMessage.players[playerCounter].color.B);
                    }
                }
            }
        }
    }

  

    void DestroyPlayers(){
        lock(lockDelete)
        {
            while(deleteMessages.Count > 0){
                var deleteMessage = deleteMessages.Dequeue();
                for(int playerCounter = 0; playerCounter < deleteMessage.players.Length; playerCounter++){
                    var cubeId = deleteMessage.players[playerCounter].id;
                    if(networkPlayers.ContainsKey(cubeId)){
                        Destroy(networkPlayers[cubeId]);
                        networkPlayers.Remove(cubeId);
                    }
                }
            }
        }
    }
    
    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }
  

    void Update(){
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}