# sdk-csharp
Stratumn services SDK

To create a secret using the private key or the user/password create SDK options object.


    Secret s = Secret.NewCredentialSecret("username", "password");
    SdkOptions opts = new SdkOptions(workflowId, s);
    Sdk sdk = new Sdk(opts);
    String token = await sdk.LoginAsync();

The login returns a token. In case of failure, it returns an error message ( this is inside the SDK so they can change to just throw the exception and handle it from outside as well)

     public async Task<string> LoginAsync() {
            Client client = new Client(this.opts);
            try
            {
            return   await client.Login();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
      }

 
