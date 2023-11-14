using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using VLCNP.Actions;
using VLCNP.UI;

namespace VLCNP.SceneManagement
{
    public class PortalCollisionAction : MonoBehaviour, ICollisionAction
    {
        [SerializeField]
        public string InformationText = null;

        bool isAction = true;

        InformationText informationTextObject = null;
        Portal portal;

        public bool IsAction { get => isAction; set => isAction = value; }

        private void Awake()
        {
            portal = GetComponent<Portal>();
        }

        public void Execute()
        {
            if (!isAction) return;
            StartCoroutine(Transition());
        }

        IEnumerator Transition() {
            isAction = false;
            yield return portal.Transition();
        }

        public void ShowInformation()
        {
            if (InformationText == null) return;
            InformationTextSpawner spawner = GetComponent<InformationTextSpawner>();
            if (spawner == null) return;
            if (informationTextObject != null) return;
            informationTextObject = spawner.Spawn(InformationText);
        }

        public void HideInformation()
        {
            if (informationTextObject == null) return;
            Destroy(informationTextObject.gameObject);
        }

        public bool IsAutoStart()
        {
            return false;
        }
    }
}
