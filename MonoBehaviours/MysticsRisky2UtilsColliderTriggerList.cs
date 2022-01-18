using UnityEngine;
using System.Collections.Generic;
using RoR2;
using System.Linq;

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

        public List<CharacterBody> RetrieveCharacterBodyList()
        {
            return RetrieveList().Select(x => x.GetComponent<CharacterBody>()).Where(x => x != null).Distinct().ToList();
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
