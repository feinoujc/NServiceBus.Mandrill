# NServiceBus.Mandrill
NServiceBus add-on to allow for sending mandrill emails over the bus


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
