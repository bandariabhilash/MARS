using System.Web;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.IO;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace FarmerBrothers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EncryptedActionParameterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            //Dictionary<string, object> decryptedParameters = new Dictionary<string, object>();
            if (HttpContext.Current.Request.Url.OriginalString != null)
            {
                string encryptedQueryString = string.Empty;
                string decrptedString = string.Empty;
                int IsUrlNotencryptedIndex = 1;
                if (HttpContext.Current.Request.UrlReferrer!=null)
                {
                    encryptedQueryString = HttpContext.Current.Request.UrlReferrer.OriginalString;
                }
                else
                {
                    encryptedQueryString = HttpContext.Current.Request.Url.OriginalString;
                }
                if (!Convert.ToBoolean(ConfigurationManager.AppSettings["UseEncryption"]))
                {
                    if (encryptedQueryString.Contains("encrypt=yes"))
                    {
                        decrptedString = Utilities.Utility.DecryptUrl(encryptedQueryString.ToString());
                        IsUrlNotencryptedIndex = 0;
                    }
                    else
                    {
                        decrptedString = encryptedQueryString;
                    }
                }
                else
                {
                    decrptedString = Utilities.Utility.DecryptUrl(encryptedQueryString.ToString());
                    IsUrlNotencryptedIndex = 0;
                }
               
                
                string[] paramsArrs = decrptedString.Split('?');
                if (paramsArrs.Length > 0)
                {
                    string[] actionParms = paramsArrs[IsUrlNotencryptedIndex].Split('&');
                    for (int i = 0; i < actionParms.Length; i++)
                    {
                        string[] paramArr = actionParms[i].Split('=');
                        //filterContext.ActionParameters[paramArr[i]] = paramArr[1];
                        //decryptedParameters.Add(paramArr[0], Convert.ToInt32(paramArr[1]));
                        switch (i)
                        {
                            case 0:
                                filterContext.ActionParameters[paramArr[0]] = Convert.ToInt32(paramArr[1]);
                                break;
                            case 1:
                                {
                                    if (filterContext.ActionParameters["techId"]==null)
                                    {
                                        filterContext.ActionParameters[paramArr[0]] = Convert.ToInt32(paramArr[1]);
                                    }                                   
                                }
                                break;
                            case 2:
                                {                                   
                                    switch (Convert.ToInt32(paramArr[1]))
                                    {
                                        case (int)Data.DispatchResponse.ACCEPTED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.ACCEPTED;
                                            break;
                                        case (int)Data.DispatchResponse.REJECTED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.REJECTED;
                                            break;
                                        case (int)Data.DispatchResponse.ARRIEVED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.ARRIEVED;
                                            break;

                                        case (int)Data.DispatchResponse.COMPLETED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.COMPLETED;
                                            break;

                                        case (int)Data.DispatchResponse.ACKNOWLEDGED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.ACKNOWLEDGED;
                                            break;
                                        case (int)Data.DispatchResponse.REDIRECTED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.REDIRECTED;
                                            break;
                                        case (int)Data.DispatchResponse.STARTED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.STARTED;
                                            break;
                                        case (int)Data.DispatchResponse.CALLCLOSURE:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.CALLCLOSURE;
                                            break;
                                        case (int)Data.DispatchResponse.SCHEDULED:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.SCHEDULED;
                                            break;
                                        case (int)Data.DispatchResponse.ESMESCALATION:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.ESMESCALATION;
                                            break;
                                        case (int)Data.DispatchResponse.PROCESSCARD:
                                            filterContext.ActionParameters[paramArr[0]] = Data.DispatchResponse.PROCESSCARD;
                                            break;
                                    }
                                }

                                break;
                            case 3:
                                filterContext.ActionParameters[paramArr[0]] = Convert.ToBoolean(paramArr[1]);
                                break;
                            case 4:
                                filterContext.ActionParameters[paramArr[0]] = paramArr[1];
                                break;
                        }

                    }
                }

            }
            base.OnActionExecuting(filterContext);

        }

    }
}