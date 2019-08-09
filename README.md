# sdk-csharp
Stratumn services SDK

To create a secret key using user/password :

    Secret s = Secret.NewPrivateKeySecret("private key");
    SdkOptions opts = new SdkOptions("workflow id", s);
    Sdk sdk = new Sdk(opts);
    String token = await sdk.LoginAsync();
    
 To create a secret key using private key:

    Secret s = Secret.NewCredentialSecret("username", "password");
    SdkOptions opts = new SdkOptions(workflowId, s);
    Sdk sdk = new Sdk(opts);
    String token = await sdk.LoginAsync();


The login returns a token. In case of failure, it returns an error message .

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

 
