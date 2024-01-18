using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtPlatformer.VillageHouse
{
    public class ConstantRotation : MonoBehaviour
    {
        public Vector3 speed;

        private void Update()
        {
            transform.Rotate(speed * Time.deltaTime);
        }
    }
}
