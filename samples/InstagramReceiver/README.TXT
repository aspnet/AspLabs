Microsoft ASP.NET WebHooks Instagram Receiver
---------------------------------------------

This sample illustrates how to wire up an Instagram WebHooks receiver. A sample WebHook URI is:

  https://<host>/api/webhooks/incoming/instagram/{id}

Note that for security reasons the URI <b>MUST</b> be <b>https</b>.

Set the MS_WebHookReceiverSecret_Instagram application setting to the Instagram application secret, optionally using IDs
to differentiate between multiple WebHooks, for example 'secret0, id1=secret1, id2=secret2'. 

Also, set the MS_WebHookReceiverSecret_InstagramId application setting to the application id, again optionally using IDs
to differentiate between multiple WebHooks, for example '<c>secret0, id1=secret1, id2=secret2'.

To subscribe to instagram, send an empty POST request to

  http://<host>/api/instagram/subscribe

To unsubscribe, send an empty POST request to

  http://<host>/api/instagram/unsubscribe?id=<id>

Please see https://www.instagram.com/developer/subscriptions for more information about Instagram WebHooks for more information.

See http://www.oauthforaspnet.com/providers/instagram/ for information about how to register an application with Instagram and
how to authenticate it. To test locally and in a deployed Web site, set the redirect URI for the Instagram app to both

  http://localhost:50006/signin-instagram
  https://<host>/signin-instagram

When deploying into Azure, also change the DefaultConnection connection string in Web.Config to a valid SQL Azure connection string

