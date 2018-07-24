namespace Zealot.Billing.Client
{
    using Contracts.Requests;
    using Contracts.Responses;
    using Enums;
    using log4net;
    using log4net.Appender;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class BillingAgent
    {
        // Performant variation of the Singleton pattern 
        // with thread safety and lazy instantiation.
        public static BillingAgent Instance { get { return Nested.instance; } }

        private readonly string _LogFilePath = ConfigurationManager.AppSettings["Zealot.Billing.Client - LogFilePath"];

        private readonly string _BillingServerVerifyUrl = ConfigurationManager.AppSettings["Zealot.Billing.Client - BillingServerVerifyUrl"];
        private readonly string _BillingServerClaimUrl = ConfigurationManager.AppSettings["Zealot.Billing.Client - BillingServerClaimUrl"];
        private readonly string _BillingServerGetLatestUrl = ConfigurationManager.AppSettings["Zealot.Billing.Client - BillingServerGetLatestUrl"];

        private readonly HttpClient _HttpClient = new HttpClient();

        private ILog _iLog;

        private bool _isEnabled = ConfigurationManager.AppSettings["Zealot.Billing.Client - Enabled"] == "true" ? true : false;
        
        private BillingAgent()
        {
            InitLogging();
        }

        public bool IsAndroid(byte merchantType)
        {
            if ((MerchantType)merchantType == MerchantType.Google)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Dictionary<string, DateTime>> GetLatestPurchasesAsync(Guid charId, string serverId)
        {
            Dictionary<string, DateTime> recentPurchases = new Dictionary<string, DateTime>();
            
            _iLog.InfoFormat("serverId: {0} | GetLatestPurchasesAsync [ charId: {1}, status: Request ]", serverId, charId);

            RequestGetLatest requestGetLatest = new RequestGetLatest();
            requestGetLatest.charId = charId;
            requestGetLatest.serverId = serverId;

            string json = JsonConvert.SerializeObject(requestGetLatest);

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage httpResponseMessage = await _HttpClient.PostAsync(_BillingServerGetLatestUrl, content).ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode == true)
                {
                    HttpContent httpContent = httpResponseMessage.Content;
                    string jsonResponse = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

                    ResponseGetLatest responseGetLatest = JsonConvert.DeserializeObject<ResponseGetLatest>(jsonResponse);
                    string responseServerId = responseGetLatest.serverId;
                    Guid responseCharId = responseGetLatest.charId;
                    GetLatestStatus responseStatus = responseGetLatest.status;

                    _iLog.InfoFormat("serverId: {0} | GetLatestPurchasesAsync [ charId: {1}, status: {2}]",
                        responseServerId, responseCharId, responseStatus);

                    switch (responseGetLatest.status)
                    {
                        case GetLatestStatus.Pass:
                            foreach (ResponseGetLatest.BillingRecord billingRecord in responseGetLatest.billingRecords)
                            {
                                recentPurchases.Add(billingRecord.productId, billingRecord.purchasedDate);
                            }

                            break;

                        case GetLatestStatus.NoRecordsFound:
                            break;
                        case GetLatestStatus.DBError:
                        case GetLatestStatus.Offline:
                        default:
                            recentPurchases = null;

                            break;
                    }
                }
                else
                {
                    _iLog.ErrorFormat("serverId: {0} | GetLatestPurchasesAsync [ charId: {1}, httpStatusCode: {2} ]", 
                        charId, httpResponseMessage.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _iLog.Error(ex.ToString());
            }

            return recentPurchases;
        }

        public async Task<Guid> VerifyPurchaseAsync(string serverId, Guid charId, string productId, string transactionId, string receipt, byte merchantType)
        {
            _iLog.InfoFormat("serverId: {0} | VerifyPurchaseAsync [ charId: {1}, transactionId: {2}, status: Request ]", serverId, charId, transactionId);

            RequestVerify requestVerify = new RequestVerify();
            requestVerify.merchantType = (MerchantType)merchantType;
            requestVerify.transactionId = transactionId;
            requestVerify.productId = productId;
            requestVerify.receipt = receipt;
            requestVerify.charId = charId;
            requestVerify.serverId = serverId;

            // Used by WeGames
            //requestVerify.auxiliaryId = "";
            //requestVerify.guestId = "";
            //requestVerify.charName = gameClientPeer.mPlayer.Name;
            //requestVerify.originServerId = gameClientPeer.ServerIDDisplay.ToString();

            string json = JsonConvert.SerializeObject(requestVerify);

            Guid billingId = Guid.Empty;

            try
            {
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponseMessage = await _HttpClient.PostAsync(_BillingServerVerifyUrl, content).ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode == true)
                {
                    HttpContent httpContent = httpResponseMessage.Content;
                    string jsonResponse = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

                    ResponseVerify responseVerify = JsonConvert.DeserializeObject<ResponseVerify>(jsonResponse);
                    string responseServerId = responseVerify.serverId;
                    Guid responseCharId = responseVerify.charId;
                    string responseTransactionId = responseVerify.transactionId;
                    VerifyStatus responseStatus = responseVerify.status;

                    _iLog.InfoFormat("serverId: {0} | VerifyPurchaseAsync [ charId: {1}, transactionId: {2}, status: {3}]",
                        responseServerId, responseCharId, transactionId, responseStatus);

                    if (responseVerify.status == VerifyStatus.Pass || responseVerify.status == VerifyStatus.AlreadyExists)
                    {
                        billingId = responseVerify.billingId;
                    }
                }
                else
                {
                    _iLog.ErrorFormat("serverId: {0} | VerifyPurchaseAsync [ charId: {1}, transactionId: {2}, httpStatusCode: {3} ]", 
                        serverId, charId, transactionId, httpResponseMessage.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _iLog.Error(ex.ToString());
            }

            return billingId;
        }

        public async Task<string> ClaimPurchaseAsync(string serverId, Guid charId, Guid billingId)
        {
            _iLog.InfoFormat("serverId: {0} | ClaimPurchaseAsync [ charId: {1}, billingId: {2}, status: Request ]", serverId, charId, billingId);

            string productId = "";

            RequestClaim requestClaim = new RequestClaim();
            requestClaim.charId = charId;
            requestClaim.serverId = serverId;
            requestClaim.billingId = billingId;

            string json = JsonConvert.SerializeObject(requestClaim);
            
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            
            try
            {
                HttpResponseMessage httpResponseMessage = await _HttpClient.PostAsync(_BillingServerClaimUrl, content).ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode == true)
                {
                    HttpContent httpContent = httpResponseMessage.Content;
                    string jsonResponse = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

                    ResponseClaim responseClaim = JsonConvert.DeserializeObject<ResponseClaim>(jsonResponse);
                    string responseServerId = responseClaim.serverId;
                    Guid responseCharId = responseClaim.charId;
                    Guid responseBillingId = responseClaim.billingId;
                    ClaimStatus responseStatus = responseClaim.status;

                    _iLog.InfoFormat("serverId: {0} | ClaimPurchaseAsync [ charId: {1}, billingId:{2}, status: {3} ]",
                        responseServerId, responseCharId, responseBillingId, responseStatus);

                    if (responseClaim.status == ClaimStatus.Pass)
                    {
                        productId = responseClaim.productId;
                    }
                }
                else
                {
                    _iLog.ErrorFormat("serverId: {0} | ClaimPurchaseAsync [ charId: {1}, billingId: {2}, httpStatusCode: {3} ]", 
                        serverId, charId, billingId, httpResponseMessage.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _iLog.Error(ex.ToString());
            }

            return productId;
        }

        private void InitLogging()
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.Name = "Zealot.Billing.Client.Appender";
            appender.File = _LogFilePath;
            appender.Encoding = Encoding.UTF8;
            appender.AppendToFile = true;
            appender.RollingStyle = RollingFileAppender.RollingMode.Date;
            appender.StaticLogFileName = false;
            appender.DatePattern = "yyyyMMdd'.log'";
            appender.LockingModel = new FileAppender.MinimalLock();
            
            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%date [%thread] %level %logger - %message%newline";
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            _iLog = LogManager.GetLogger("Zealot.Billing.Client");
            Logger logger = (Logger)_iLog.Logger;
            logger.Level = logger.Hierarchy.LevelMap["DEBUG"];
            logger.AddAppender(appender);
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit.
            static Nested()
            {

            }

            internal static readonly BillingAgent instance = new BillingAgent();
        }
    }
}
