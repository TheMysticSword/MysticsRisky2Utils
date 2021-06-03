using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MysticsRisky2Utils
{
    public static class NetworkHelper
    {
        public delegate void OnSpawnedOnClient(GameObject gameObject);

        public static Dictionary<NetworkInstanceId, Queue<OnSpawnedOnClient>> OnSpawnedOnClientDict = new Dictionary<NetworkInstanceId, Queue<OnSpawnedOnClient>>();

        public static void EnqueueOnSpawnedOnClientEvent(NetworkInstanceId netId, OnSpawnedOnClient onSpawnedOnClient)
        {
            GameObject gameObject = Util.FindNetworkObject(netId);
            if (gameObject)
            {
                onSpawnedOnClient(gameObject);
            }
            else
            {
                if (!OnSpawnedOnClientDict.ContainsKey(netId)) OnSpawnedOnClientDict.Add(netId, new Queue<OnSpawnedOnClient>()); 
                OnSpawnedOnClientDict[netId].Enqueue(onSpawnedOnClient);
            }
        }

        public class MysticsRisky2UtilsNetworkHelper : MonoBehaviour
        {
            public NetworkIdentity networkIdentity;

            public void Awake()
            {
                networkIdentity = GetComponent<NetworkIdentity>();
                if (!networkIdentity)
                {
                    Object.Destroy(this);
                    return;
                }
                DequeueOnSpawnedOnClientEvents();
            }

            public void DequeueOnSpawnedOnClientEvents()
            {
                NetworkInstanceId netId = networkIdentity.netId;
                if (OnSpawnedOnClientDict.ContainsKey(netId))
                {
                    while (OnSpawnedOnClientDict[netId].Count > 0)
                    {
                        OnSpawnedOnClientDict[netId].Dequeue()(gameObject);
                    }
                }
            }
        }
    }
}
