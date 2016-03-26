Microsoft ASP.NET WebHooks Instagram Receiver
---------------------------------------------

This sample illustrates how to wire up an Instagram WebHooks receiver. A sample WebHook URI is:

  https://<host>/api/webhooks/incoming/instagram/{id}

Note that for security reasons the URI <b>MUST</b> be <b>https</b>.

Set the MS_WebHookReceiverSecret_Instagram application setting to the Instagram application secret, optionally using IDs
to differentiate between multiple WebHooks, for example 'secret0, id1=secret1, id2=secret2'. 

Also, set the MS_WebHookReceiverSecret_InstagramId application setting to the application id, again optionally using IDs
to differentiate between multiple WebHooks, for example '<c>secret0, id1=secret1, id2=secret2'.

We use Instasharp to subscribe, unsubscribe, as well as for downloading media from Instagram, see http://instasharp.org/
for details. 

To subscribe to instagram, send an empty POST request to

  http://<host>/api/instagram/subscribe

To unsubscribe, send an empty POST request to

  http://<host>/api/instagram/unsubscribe

You don't have to be logged in using your Instagram credentials to subscribe or unsubscribe. The purpose of logging in 
using Instagram credentials is so that the server can download media from your Instagram when you post data and a 
WebHook notification is received.

See http://www.oauthforaspnet.com/providers/instagram/ for information about how to register an application with Instagram 
and how to authenticate it. 

To test locally and in a deployed Web site, set the redirect URI for the Instagram app to these two entries:

  http://localhost:50006/signin-instagram
  https://<host>/signin-instagram

When deploying into Azure, also change the DefaultConnection connection string to a valid SQL Azure connection string.

Please see https://www.instagram.com/developer/subscriptions for more information about Instagram WebHooks.
