# NServiceBus.Mandrill

<a href="http://www.nuget.org/packages/NServiceBus.Mandrill/"><img src="http://img.shields.io/nuget/v/NServiceBus.Mandrill.svg?" title="NuGet Status"></a>

NServiceBus add-on to allow for sending [mandrill](https://mandrillapp.com/api/docs/) emails over the bus


## Getting Started

```ps
install-package NServiceBus.Mandrill
```

```cs
public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
{
    public void Customize(BusConfiguration configuration)
    {
        var apiKey = ConfigurationManager.AppSettings["MANDRILL_API_KEY"]; //load your api key from somewhere
        configuration.UseMandrill(apiKey);
        //the rest of your setup...
    }
}

```

```cs
 var mail = new MandrillMessage();
 mail.FromEmail = "mandrill.net@example.com";
 mail.AddTo("nservicebus@example.com", "Udi Dahan");
 mail.Subject = "NServiceBus.Mandrill test";
 mail.Text = "Hello NServiceBus! \nRegards";
 Bus.SendEmail(mail);
```


This is internally forwarding a message to a custom NServiceBus Satellite that will process the request using the Mandrill api, meaning that this email will be sent using the same transactional behavior you expect with any NServiceBus operation

## Handling responses

Optionally, you can write a message handler to handle the responses coming back from the API. To do this, use the configuration method to configure the satellite to return the response message:

```cs
configuration.UseMandrill(apiKey: apiKey, replyResult: true);
```

You'll then need to write a standard message handler for message type `MandrillEmailResult`

```cs
class EmailResultHandler : IHandleMessages<MandrillEmailResult>
{
    public void Handle(MandrillEmailResult message)
    {
        //do something with the message result
        Console.WriteLine("{0} {1} {2}", message.Response.Id, message.Response.Status, message.Response.Email);
    }
}
```

The Sample project includes an example using this approach to set a callback that retrieves the email content for all sent messages

## API errors

In the event that the Mandrill API returns an error (invalid api key, 5xx server errors, invalid message errors, etc) that is handled using the standard exception handling in NServiceBus, and may ultimately end up in the error queue, depending on your configuration. 
