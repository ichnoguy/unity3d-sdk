using UnityEngine;
using UnityEngine.UI;
using Loom.Unity3d;
using Newtonsoft.Json;
using Google.Protobuf;
using System;
using System.Threading.Tasks;

public class etherboySample : MonoBehaviour
{
    public Text statusTextRef;
    public InputField ownerInputRef;
    public InputField healthInputRef;
    public InputField coinsInputRef;

    private Address contractAddr;
    private Identity identity;
    private Address callerAddr;
    private DAppChainClient chainClient;

    // Use this for initialization
    void Start()
    {
        // By default the editor won't respond to network IO or anything if it doesn't have input focus,
        // which is super annoying when input focus is given to the web browser for the Auth0 sign-in.
        Application.runInBackground = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IAuthClient CreateAuthClient()
    {
#if !UNITY_WEBGL
        try
        {
            CertValidationBypass.Enable();
            return AuthClientFactory.Configure()
                .WithLogger(Debug.unityLogger)
                .WithClientId("25pDQvX4O5j7wgwT052Sh3UzXVR9X6Ud") // unity3d sdk
                .WithDomain("loomx.auth0.com")
                .WithScheme("io.loomx.unity3d")
                .WithAudience("https://keystore.loomx.io/")
                .WithScope("openid profile email picture")
                .WithRedirectUrl("http://127.0.0.1:9999/auth/auth0/")
                .Create();
        }
        finally
        {
            CertValidationBypass.Disable();
        }
#else
        return AuthClientFactory.Configure()
            .WithLogger(Debug.unityLogger)
            .WithHostPageHandlers(new Loom.Unity3d.WebGL.HostPageHandlers
            {
                SignIn = "authenticateFromGame",
                GetUserInfo = "getUserInfo",
                SignOut = "clearUserInfo"
            })
            .Create();
#endif
    }

#if !UNITY_WEBGL // In WebGL all interactions with the key store should be done in the host page.
    private async Task<IKeyStore> CreateKeyStore(string accessToken)
    {
        return await KeyStoreFactory.CreateVaultStore(new VaultStoreConfig
        {
            Url = "https://stage-vault.delegatecall.com/v1/",
            VaultPrefix = "unity3d-sdk",
            AccessToken = accessToken
        });
    }
#endif

    public async void SignIn()
    {
#if !UNITY_WEBGL
        try
        {
            CertValidationBypass.Enable();
            var authClient = this.CreateAuthClient();
            var accessToken = await authClient.GetAccessTokenAsync();
            var keyStore = await this.CreateKeyStore(accessToken);
            this.identity = await authClient.GetIdentityAsync(accessToken, keyStore);
        }
        finally
        {
            CertValidationBypass.Disable();
        }
#else
        var authClient = this.CreateAuthClient();
        this.identity = await authClient.GetIdentityAsync("", null);
#endif
        this.statusTextRef.text = "Signed in as " + this.identity.Username;
        this.ownerInputRef.text = this.identity.Username;

        // This DAppChain client will connect to the example REST server in the Loom Go SDK. 
        this.chainClient = new DAppChainClient("http://localhost:46658", "http://localhost:47000")
        {
            Logger = Debug.unityLogger
        };
        this.chainClient.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]{
            new NonceTxMiddleware{
                PublicKey = this.identity.PublicKey,
                Client = this.chainClient
            },
            new SignedTxMiddleware(this.identity.PrivateKey)
        });

        // There is only one contract address at the moment...
        this.contractAddr = new Address
        {
            ChainId = "default",
            Local = ByteString.CopyFrom(CryptoUtils.HexStringToBytes("0x005B17864f3adbF53b1384F2E6f2120c6652F779"))
        };
        this.callerAddr = this.identity.ToAddress("default");
    }

    public async void SignOut()
    {
        var authClient = this.CreateAuthClient();
        await authClient.ClearIdentityAsync();
    }

    public async void ResetPrivateKey()
    {
#if !UNITY_WEBGL
        try
        {
            CertValidationBypass.Enable();
            var authClient = this.CreateAuthClient();
            var accessToken = await authClient.GetAccessTokenAsync();
            var keyStore = await this.CreateKeyStore(accessToken);
            this.identity = await authClient.CreateIdentityAsync(accessToken, keyStore);
        }
        finally
        {
            CertValidationBypass.Disable();
        }
#else
        // TODO
        throw new NotImplementedException();
#endif
    }

    // The backend doesn't care what the account info structure looks like,
    // it just needs to be serializable to JSON.
    // NOTE: Don't store any private info like email.
    private class SampleAccountInfo
    {
        public DateTime CreatedOn { get; set; }
    }

    public async void CreateAccount()
    {
        if (this.identity == null)
        {
            throw new System.Exception("Not signed in!");
        }
        // Create new player account
        var accountInfo = JsonConvert.SerializeObject(new SampleAccountInfo
        {
            CreatedOn = DateTime.Now
        });
        var createAcctTx = new EtherboyCreateAccountTx
        {
            Version = 0,
            Owner = this.ownerInputRef.text,
            Data = ByteString.CopyFromUtf8(accountInfo)
        };

        var result = await this.chainClient.CallAsync(this.callerAddr, this.contractAddr, "etherboycore.CreateAccount", createAcctTx);
        this.statusTextRef.text = "Committed Tx to Block " + result.Height;
    }

    // The backend doesn't care what the state structure looks like,
    // it just needs to be serializable to JSON.
    private class SampleState
    {
        public int Health { get; set; }
        public int Coins { get; set; }
    }

    public async void SaveState()
    {
        if (this.identity == null)
        {
            throw new System.Exception("Not signed in!");
        }

        // Save state to the backend
        var state = JsonConvert.SerializeObject(new SampleState
        {
            Health = int.Parse(this.healthInputRef.text),
            Coins = int.Parse(this.coinsInputRef.text)
        });
        var saveStateTx = new EtherboyStateTx
        {
            Version = 0,
            Owner = this.ownerInputRef.text,
            Data = ByteString.CopyFromUtf8(state)
        };
        
        var result = await this.chainClient.CallAsync(this.callerAddr, this.contractAddr, "etherboycore.SaveState", saveStateTx);
        this.statusTextRef.text = "Committed Tx to Block " + result.Height;
    }

    public async void Query()
    {
        // NOTE: Query results can be of any type that can be deserialized via Newtonsoft.Json.
        var result = await this.chainClient.QueryAsync<StateQueryResult>(
            this.contractAddr, "etherboycore.GetState", new StateQueryParams{ Owner = this.ownerInputRef.text }
        );
        if ((result == null) || (result.State == null))
        {
            throw new Exception("Failed to retrieve state");
        }
        var state = JsonConvert.DeserializeObject<SampleState>(result.State.ToStringUtf8());
        this.statusTextRef.text = string.Format("Health: {0}, Coins: {1}", state.Health, state.Coins);
    }
}
