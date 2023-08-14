using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace FarmerBrothers.Utilities
{
    public class EmailUtility
    {

        //public async Task SendAsync(string fromAddress, string toAddress, string subject, string content)
        //{
        //    //string? tenantId = _config["tenantId"];
        //    //string? clientId = _config["clientId"];
        //    // string? clientSecret = _config["clientSecret"];
        //    //MAI
        //    // string? tenantId = "ffc1dd32-94f1-4ef8-b27b-a0dd7fd31d41";
        //    //   string? clientId = "bc6a0a9e-ce06-43f1-8dd1-b91eb8d79459";
        //    //   string? clientSecret = "zEy8Q~XfVaSFpE.it.hgQ1VSLMpbOm98M4KBbdgk";

        //    //Farmer Brothers
        //    string tenantId = "c6af841f-2185-4e29-b8fc-f5f4a312f72d";
        //    string clientId = "2fdb7b4b-ee89-42f1-8fd2-17642b04ba96";
        //    string clientSecret = "eIc8Q~CO~cmh4YIUVze71MPUFW4zhC5lkUgn1cfp";

        //    //ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        //    //GraphServiceClient graphClient = new GraphServiceClient((credential);
        //    ClientSecretCredential credentials = new ClientSecretCredential(tenantId, clientId, clientSecret,
        //                            new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });
        //    GraphServiceClient graphClient = new GraphServiceClient(credentials);

        //    Message message = new Message()
        //    {
        //        Subject = subject,
        //        Body = new ItemBody
        //        {
        //            ContentType = BodyType.Text,
        //            Content = content
        //        },
        //        ToRecipients = new List<Recipient>()
        //        {
        //            new Recipient
        //            {
        //                EmailAddress = new EmailAddress
        //                {
        //                    Address = toAddress
        //                }
        //            }
        //        }
        //    };

        //    bool saveToSentItems = true;

        //    await graphClient.Users[fromAddress]
        //      .SendMail(message, saveToSentItems)
        //      .Request()
        //      .PostAsync();
        //}

        public string SendEmail_old(string fromAddress, string toAddress, string CcAddress, string subject, string message)
        {
            try
            {

                string tenanatID = ConfigurationManager.AppSettings["Email_TenantId"].ToString();
                string clientID = ConfigurationManager.AppSettings["Email_ClientId"].ToString();
                string clientSecret= ConfigurationManager.AppSettings["Email_ClientSecret"].ToString();
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                var credentials = new ClientSecretCredential(
                                    tenanatID, clientID, clientSecret,
                                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });
                GraphServiceClient graphServiceClient = new GraphServiceClient(credentials);


                string[] toMail = toAddress.Split(';');
                List<Recipient> toRecipients = new List<Recipient>();
                int i = 0;
                for (i = 0; i < toMail.Count(); i++)
                {
                    Recipient toRecipient = new Recipient();
                    EmailAddress toEmailAddress = new EmailAddress();

                    toEmailAddress.Address = toMail[i];
                    toRecipient.EmailAddress = toEmailAddress;
                    toRecipients.Add(toRecipient);
                }

                List<Recipient> ccRecipients = new List<Recipient>();
                if (!string.IsNullOrEmpty(CcAddress))
                {
                    string[] ccMail = CcAddress.Split(';');
                    int j = 0;
                    for (j = 0; j < ccMail.Count(); j++)
                    {
                        Recipient ccRecipient = new Recipient();
                        EmailAddress ccEmailAddress = new EmailAddress();

                        ccEmailAddress.Address = ccMail[j];
                        ccRecipient.EmailAddress = ccEmailAddress;
                        ccRecipients.Add(ccRecipient);
                    }
                }
                var mailMessage = new Message
                {
                    Subject = subject,

                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = message
                    },
                    ToRecipients = toRecipients,
                    CcRecipients = ccRecipients

                };
                // Send mail as the given user. 
                graphServiceClient
                   .Users[fromAddress]
                    .SendMail(mailMessage, true)
                    .Request()
                    .PostAsync().Wait();

                return "Email successfully sent.";

            }
            catch (Exception ex)
            {

                return "Send Email Failed.\r\n" + ex.Message;
            }
        }

        public string SendEmail(string fromAddress, string toAddress, string CcAddress, string subject, string message)
        {
            try
            {

                string tenanatID = ConfigurationManager.AppSettings["Email_TenantId"].ToString();
                string clientID = ConfigurationManager.AppSettings["Email_ClientId"].ToString();
                string clientSecret = ConfigurationManager.AppSettings["Email_ClientSecret"].ToString();
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                var credentials = new ClientSecretCredential(
                                    tenanatID, clientID, clientSecret,
                                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });
                GraphServiceClient graphServiceClient = new GraphServiceClient(credentials);


                string[] toMail = toAddress.Split(';');
                List<Recipient> toRecipients = new List<Recipient>();
                int i = 0;
                for (i = 0; i < toMail.Count(); i++)
                {
                    if (string.IsNullOrEmpty(toMail[i])) continue;

                    Recipient toRecipient = new Recipient();
                    EmailAddress toEmailAddress = new EmailAddress();

                    toEmailAddress.Address = toMail[i];
                    toRecipient.EmailAddress = toEmailAddress;
                    toRecipients.Add(toRecipient);
                }

                List<Recipient> ccRecipients = new List<Recipient>();
                if (!string.IsNullOrEmpty(CcAddress))
                {
                    string[] ccMail = CcAddress.Split(';');
                    int j = 0;
                    for (j = 0; j < ccMail.Count(); j++)
                    {
                        if (string.IsNullOrEmpty(ccMail[j])) continue;

                        Recipient ccRecipient = new Recipient();
                        EmailAddress ccEmailAddress = new EmailAddress();

                        ccEmailAddress.Address = ccMail[j];
                        ccRecipient.EmailAddress = ccEmailAddress;
                        ccRecipients.Add(ccRecipient);
                    }
                }

                string contentId = Guid.NewGuid().ToString();
                string logoPath = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "img\\mainlogo.jpg";

                message = message.Replace("cid:logo", "cid:" + contentId);

                byte[] imageArray = System.IO.File.ReadAllBytes(logoPath);
                var attachments = new MessageAttachmentsCollectionPage()
                {
                    new FileAttachment{
                        ContentType= "image/jpeg",
                        ContentBytes = imageArray,
                        ContentId = contentId,
                        Name= "test-image"
                    }
                };

                var mailMessage = new Message
                {
                    Subject = subject,

                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = message
                    },
                    ToRecipients = toRecipients,
                    CcRecipients = ccRecipients,
                    Attachments = attachments

                };
                // Send mail as the given user. 
                graphServiceClient
                   .Users[fromAddress]
                    .SendMail(mailMessage, true)
                    .Request()
                    .PostAsync().Wait();

                return "Email successfully sent.";

            }
            catch (Exception ex)
            {

                return "Send Email Failed.\r\n" + ex.Message;
            }
        }

    }
}