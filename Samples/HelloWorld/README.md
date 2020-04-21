# OpenTok Hello World .NET

This is a simple demo app that shows how you can use the OpenTok .NET SDK to create Sessions,
generate Tokens with those Sessions, and then pass these values to a JavaScript client that can
connect and conduct a group chat.

**Note**: These instructions assume you will be using Visual Studio.
The sample projects are contained inside the `OpenTok.sln` solution at the top level.


## Running the App

First, add your own API Key and API Secret to the Application Settings. For your convenience, the
`appsettings.json` file is set up for you to place your values into it.

```
    "API_KEY": "000000" />
    "API_SECRET": "abcdef1234567890abcdef"
```

Next, make sure the HelloWorld project is set as the Solution's Startup project. This can be done
by opening the Properties of the solution, and selecting it under Common Properties > Startup Project.

Lastly, the dependencies of the application are referenced using NuGet. Package Restore is a feature of
NuGET 2.7+ and as long as your installation of Visual Studio has the NuGet extension installed with a
later version, the package should be installed automatically on launch. Otherwise, use the 
[NuGET Packge Restore guide](http://docs.nuget.org/docs/reference/package-restore).

Choose Start (Ctrl+F5) to run the application.

Visit <http://localhost:8080> in your browser. Open it again in a second window. Smile! You've just
set up a group chat.

### OpenTok Service (OpenTokService.cs)

This object provides its very simple functionality as a service to the rest of the application. Its
only responsibility is to initialize a couple OpenTok related objects and make them visible any part
of the application that needs them.

```csharp
public class OpenTokService
{
    public Session Session { get; protected set; }
    public OpenTok OpenTok { get; protected set; }

    public OpenTokService(IConfiguration configuration)
    {
        int apiKey = 0;
        string apiSecret = null;
        try
        {
            string apiKeyString = configuration["API_KEY"];
            apiSecret = configuration["API_SECRET"];
            apiKey = Convert.ToInt32(apiKeyString);
            apiKey = Convert.ToInt32(apiKeyString);
        }

        catch (Exception ex)
        {
            if (!(ex is ConfigurationErrorsException || ex is FormatException || ex is OverflowException))
            {
                throw ex;
            }
        }

        finally
        {
            if (apiKey == 0 || apiSecret == null)
            {
                Console.WriteLine(
                    "The OpenTok API Key and API Secret were not set in the application configuration. " +
                    "Set the values in App.config and try again. (apiKey = {0}, apiSecret = {1})", apiKey, apiSecret);
                Console.ReadLine();
                Environment.Exit(-1);
            }
        }

        this.OpenTok = new OpenTok(apiKey, apiSecret);

        this.Session = this.OpenTok.CreateSession();
    }
}
```

This class simply has the default constructor (called by the IoC Container) and two public properties
with protected setters. In the constructor, the application configuration is read to retrieve an API Key
and an API Secret. If those settings are not available, an exception is thrown and an appropriate error
message is written to the console. Then the API Key and API Secret are used to initialize and instance
of `OpenTok`, which is stored into the one of the public properties. 

Now, lets discuss the Hello World application's functionality. We want to set up a group chat so
that any client (user in a browser) that visits a page will connect to the same OpenTok Session. Once
they are connected they can Publish a Stream and Subscribe to all the other streams in that Session. So we
just need one Session object, and the OpenTokService can be used to make it accessible. The next line of
our application simply calls the `OpenTok` instance's `CreateSession()` method and stores the resulting
Session object in the other public property. Alternatively, for applications that have many Sessions,
the `Id` property of a `Session` can be stored in a database and used for all of the same operations that
can be done with the instance (using slightly different API).

### Main Module (HomeController.cs)

In a Nancy application, any subclasses of `NancyModule` are initialized by the framework and given the
opportunity to respond to requests using route matching. This class's dependencies are expressed as
arguments to the constructor, which the IoC Container will fill in the right instance, such as an
instance of OpenTokService as we need in this Module.

As we've discussed already, since we want any user who comes to the application to be placed into
one group chat, we only need one page. So we create one route handler for any HTTP GET requests to trigger,
which is done by using just the `"/"` as the route.

```csharp
public class MainModule : NancyModule
{

    public MainModule(OpenTokService opentokService)
    {

        Get["/"] = _ =>
            {
                // ...
            };
    }
}
```

Now all we have to do is serve a page with the three values the client will need to connect to the
session: `ApiKey`, `SessionId`, and `Token`. The first two are available from the `OpenTokService`: the
`ApiKey` is a property on the OpenTok instance, and the `SessionId` is a property on the `Session`
instance. The `Token` is generated freshly on this request by calling the Session instance's
`GenerateToken()` method. This is because a Token is a piece of information that carries a specific client's
permissions in a certain Session. Ideally, as we've done here, you generate a unique token for each
client that will connect.

These three values can be stored in a model, and then passed to the View rendering system. Here we've chosen
to use a dynamic `ExpandoObject` instead of strongly typing the model, for our convenience.

```csharp
        // ...
        Get["/"] = _ =>
            {
                dynamic locals = new ExpandoObject();

                locals.ApiKey = opentokService.OpenTok.ApiKey.ToString();
                locals.SessionId = opentokService.Session.Id;
                locals.Token = opentokService.Session.GenerateToken();

                return View["index", locals];
            };
        // ...
```

The View rendering system will find a template named "index" and combine it with the data in the `locals` object
to create a response to send back to the browser.

### Main Template (views/index.sshtml)

This file simply sets up the HTML page for the JavaScript application to run, imports the OpenTok.js
JavaScript library, and passes the values created by the server into the JavaScript application
inside `Content/js/helloworld.js`

### JavaScript Applicaton (Content/js/helloworld.js)

The group chat is mostly implemented in this file. At a high level, we connect to the given
Session, publish a stream from our webcam, and listen for new streams from other clients to
subscribe to.

For more details, read the comments in the file or go to the
[JavaScript Client Library](http://tokbox.com/opentok/libraries/client/js/) for a full reference.
