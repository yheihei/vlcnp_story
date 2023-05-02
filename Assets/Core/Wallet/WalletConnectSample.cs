using Thirdweb;
using UnityEngine;
using UnityEngine.UI;

public class WalletConnectSample : MonoBehaviour
{
    private ThirdwebSDK sdk;
    private bool isHit = false;
    public Text address;

    void Start()
    {
        sdk = new ThirdwebSDK("goerli");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHit)
        {
            return;
        }
        if (collision.tag.Equals("Player"))
        {
            print("Wallet Connecting!!");
            isHit = true;
            ConnectWallet();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            isHit = false;
        }
    }

    public async void ConnectWallet()
    {
        // Connect to the wallet
        string _address = await sdk.wallet.Connect();
        address.text = _address;
        print(address);
    }
}