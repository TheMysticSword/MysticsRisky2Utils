using UnityEngine;
using System.Collections.Generic;

namespace MysticsRisky2Utils.MonoBehaviours
{
    public class MysticsRisky2UtilsColliderTriggerList : MonoBehaviour
    {
        private List<Collider> list;

        public void Awake()
        {
            list = new List<Collider>();
        }

        public List<Collider> RetrieveList()
        {
            list.RemoveAll(x => !x); // remove colliders that don't exist anymore from the list
            return list;
        }
        
        public void OnTriggerEnter(Collider other)
        {
            list.Add(other);
        }

        public void OnTriggerExit(Collider other)
        {
            list.Remove(other);
        }
    }
}
