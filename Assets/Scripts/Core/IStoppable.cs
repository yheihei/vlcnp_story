using Org.BouncyCastle.Crypto.Engines;
using Unity.VisualScripting;

namespace VLCNP.Core
{
    public interface IStoppable
    {
        public bool IsStopped { get; set; }
    }
}
