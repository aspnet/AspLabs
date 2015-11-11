using System;
using Microsoft.AspNet.WebHooks.Config;
using Microsoft.AspNet.WebHooks.Services;
using Microsoft.Owin.Hosting;

namespace MailChimpReceiver.Selfhost
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string baseAddress = "http://localhost:50008/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                string mailChimpAddress = GetWebHookAddress(baseAddress);
                Console.WriteLine("Starting MailChimp WebHooks receiver running on " + mailChimpAddress);
                Console.WriteLine("For non-localhost requests, use of 'https' is required!");
                Console.WriteLine("For more information about MailChimp WebHooks, please see 'https://apidocs.mailchimp.com/webhooks/'");
                Console.WriteLine("Hit ENTER to exit!");
                Console.ReadLine();
            }
        }

        private static string GetWebHookAddress(string baseAddress)
        {
            SettingsDictionary settings = CommonServices.GetSettings();
            string code = settings["MS_WebHookReceiverSecret_MailChimp"];
            string address = baseAddress + "api/webhooks/incoming/mailchimp?code=" + code;
            return address;
        }
    }
}
