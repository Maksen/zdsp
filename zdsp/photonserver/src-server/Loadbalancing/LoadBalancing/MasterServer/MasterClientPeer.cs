// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MasterClientPeer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the MasterClientPeer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
#define DATABASE_OFFLINE
#define ACCOUNT_REGISTER

namespace Photon.LoadBalancing.MasterServer
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using ExitGames.Logging;
    using Photon.LoadBalancing.Operations;
    using Photon.SocketServer;
    using Zealot.Common;
    using Zealot.RPC;
    using Zealot.DBRepository.GM;
    using Facebook;
    using Photon.Hive.Operations;

    using ErrorCode = Photon.Common.ErrorCode;
    using OperationCode = Photon.LoadBalancing.Operations.OperationCode;
    using Hive.WebRpc;
    using Photon.Common.Authentication;
    using SocketServer.Diagnostics;
    using Common;
    using SocketServer.Rpc;
    using Cluster;
    using PhotonHostRuntimeInterfaces;
    using GameServer;
    #endregion

    public class MasterClientPeer : Peer
    {
        #region Constants and Fields
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public WebRpcHandler WebRpcHandler { get; set; }    
        public int HttpRpcCallsLimit { get; protected set; }
        protected AuthenticationToken unencryptedAuthToken;
        private readonly TimeIntervalCounter httpForwardedRequests = new TimeIntervalCounter(new TimeSpan(0, 0, 1));
        private readonly MasterApplication application;
        private string myCookie = "";
        private string myUserId = "";
        public ZRPC ZRPC;
        #endregion
        #region Constructors and Destructors
        public MasterClientPeer(InitRequest initRequest, MasterApplication application)
            : base(initRequest)
        {
            this.application = application;
            Initialize(initRequest);
            if (MasterApplication.AppStats != null)
            {
                MasterApplication.AppStats.IncrementMasterPeerCount();
                MasterApplication.AppStats.AddSubscriber(this);
            }
            ZRPC = new ZRPC();
        }
        #endregion
        #region Properties
        public bool UseHostnames
        {
            get
            {
                return this.IsIPv6ToIPv4Bridged;
            }
        }
        #endregion
        #region Methods
        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Disconnect: pid={0}: reason={1}, detail={2}", this.ConnectionId, reasonCode, reasonDetail);
            }
            // Update application statistics
            if (MasterApplication.AppStats != null)
            {
                MasterApplication.AppStats.DecrementMasterPeerCount();
                MasterApplication.AppStats.RemoveSubscriber(this);
            }
        }
        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("OnOperationRequest: pid={0}, op={1}", this.ConnectionId, operationRequest.OperationCode);
            }
            MasterApplication.Instance.executionFiber.Enqueue(async () =>
            {
                OperationResponse response = null;
                switch ((OperationCode)operationRequest.OperationCode)
                {
                    default:
                        response = new OperationResponse(operationRequest.OperationCode) { ReturnCode = (short)ErrorCode.OperationInvalid, DebugMessage = "Unknown operation code" };
                        break;
                    //case OperationCode.UIDShift:
                    //    response = await this.HandleUIDShiftAsync(operationRequest);
                    //    break;
                    //case OperationCode.ChangePassword:
                    //    response = await this.HandleChangePasswordAsync(operationRequest);
                    //    break;
                    case OperationCode.ConnectGameSetup:
                        response = this.HandleConnectGameSetup(operationRequest);
                        break;
                    case OperationCode.VerifyLoginId:
                        response = await this.HandleVerifyLoginIdAsync(operationRequest, sendParameters);
                        break;
                    case OperationCode.GetServerList:
                        response = this.HandleGetServerList(operationRequest, sendParameters);
                        break;
                    case OperationCode.Register:
                        response = await this.HandleRegisterAsync(operationRequest, sendParameters);
                        break;
                    case OperationCode.Authenticate:
                        response = await this.HandleAuthenticateAsync(operationRequest, sendParameters);
                        break;
                    case OperationCode.RaiseEvent:
                        break;
                }
                if (response != null)
                    this.SendOperationResponse(response, sendParameters);
            });
        }

        /// <summary>
        ///   Checks if a operation is valid. If the operation is not valid
        ///   an operation response containing a desciptive error message
        ///   will be sent to the peer.
        /// </summary>
        /// <param name = "operation">
        ///   The operation.
        /// </param>
        /// <param name = "sendParameters">
        ///   The send Parameters.
        /// </param>
        /// <returns>
        ///   true if the operation is valid; otherwise false.
        /// </returns>
        public bool ValidateOperation(Operation operation, SendParameters sendParameters)
        {
            if (operation.IsValid)
            {
                return true;
            }

            var errorMessage = operation.GetErrorMessage();
            this.SendOperationResponse(new OperationResponse
            {
                OperationCode = operation.OperationRequest.OperationCode,
                ReturnCode = (short)ErrorCode.OperationInvalid,
                DebugMessage = errorMessage
            }, sendParameters);
            return false;
        }

        private string GetUserFromParams(string authPara)
        {
            int idx = authPara.IndexOf("&token=");
            int length = idx - 9;
            if (length > 0)
                return authPara.Substring(9, idx - 9);
            return "";
        }

        private string GetTokenFromParams(string authPara)
        {
            int idx = authPara.IndexOf("&token="), idx2 = authPara.IndexOf("&extraparam=");
            if (idx2 == -1)
                idx2 = authPara.Length;
            if (idx != 0 & idx2 >= 0)
                return authPara.Substring(idx + 7, idx2 - (idx + 7));
            return "";
        }

        private string GetExtraParam(string authPara)
        {
            int idx = authPara.IndexOf("&extraparam=");
            if (idx != 0)
                return authPara.Substring(idx + 12);
            return "";
        }

        private string GetHashedID(string value)
        {
            byte[] hashInBytes = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(hashInBytes).Replace("-", "");
        }

        // Handle user register request
        private async Task<OperationResponse> HandleRegisterAsync(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var request = new RegisterRequest(this.Protocol, operationRequest);
            if (this.ValidateOperation(request, sendParameters) == false)
                return null;
            string username = request.LoginType;
            string password = request.LoginId;
            string deviceId = request.DeviceId;
            short returnCode = (short)ErrorCode.UsernameInUse;
#if ACCOUNT_REGISTER
            //string salt = Guid.NewGuid().ToString().Replace("-", "");
            //string hpass = GetHashedID(salt+password);
            AccountInsertResult? result
                = await MasterApplication.dbGMRepository.PlayerAccount.InsertAccountAsync((int)LoginAuthType.Username, username, password, deviceId);
            if (result.HasValue)
            {
                if (result.Value.ErrorCode == AccountInsertErrorCode.UsernameInUse)
                {
                    returnCode = (short)ErrorCode.UsernameInUse;
                    log.DebugFormat("ErrorCode: {0}", ErrorCode.UsernameInUse);
                }
                else
                {
                    log.DebugFormat("New register UserId {0}", result.Value.UserId);
                    returnCode = (short)ErrorCode.Ok;
                }
            }
#else
            returnCode = (short)ErrorCode.UsernameInUse;
            log.DebugFormat("ErrorCode: {0}", ErrorCode.UsernameInUse);
#endif

            var opParameter = new Dictionary<byte, object>();
            opParameter.Add((byte)ParameterCode.LoginId, username);
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = returnCode,
                Parameters = opParameter
            };
        }

        private async Task<OperationResponse> HandleVerifyLoginIdAsync(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var request = new RegisterRequest(this.Protocol, operationRequest);
            if (this.ValidateOperation(request, sendParameters) == false)
                return null;

            string loginType = request.LoginType;
            string loginId = request.LoginId;
            string requestType = request.DeviceId;
            short returnCode = (short)ErrorCode.InvalidLoginType;
            LoginAuthType currloginType;
            bool isLoginValid = Enum.TryParse(loginType, out currloginType); // Check if valid
            if (isLoginValid)
            {
                List<Dictionary<string, object>> loginIdList
                    = await MasterApplication.dbGMRepository.PlayerAccount.GetAccountByLoginAsync((int)currloginType, loginId);
                returnCode = (loginIdList.Count > 0) ? (short)ErrorCode.UsernameInUse : (short)ErrorCode.Ok;
            }

            var opParameter = new Dictionary<byte, object>();
            opParameter.Add((byte)ParameterCode.LoginId, loginId);
            if (!string.IsNullOrEmpty(requestType))
                opParameter.Add((byte)ParameterCode.DeviceId, requestType);
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = returnCode,
                Parameters = opParameter
            };
        }

        /*private async Task<OperationResponse> HandleChangePasswordAsync(OperationRequest operationRequest)
        {
            string loginId = operationRequest.Parameters[(byte)ParameterCode.LoginType] as string;
            string newPass = operationRequest.Parameters[(byte)ParameterCode.ExtraParam] as string;
            short returnCode = (short)ErrorCode.Ok;
            List<Dictionary<string, object>> loginIdList 
                = await MasterApplication.dbGM.PlayerAccount.GetAccountByLoginAsync((int)LoginType.Username, loginId);
            if (loginIdList.Count <= 0) // Account does not exist
                returnCode = (short)ErrorCode.UsernameDoesNotExist;
            else // Account exist
            {
                string oldPass = operationRequest.Parameters[(byte)ParameterCode.LoginId].ToString();
                string oldSalt = loginIdList[0]["salt"].ToString();
                string oldHpass = loginIdList[0]["password"].ToString();
                if(oldHpass.Equals(GetHashedID(oldSalt+oldPass))) // Correct old password
                {
                    // Change password
                    string deviceId = operationRequest.Parameters[(byte)ParameterCode.DeviceId].ToString();
                    string salt = Guid.NewGuid().ToString().Replace("-", "");
                    string hpass = GetHashedID(salt+newPass);
                    bool success = await MasterApplication.dbRepository.UserAccount.ChangePassword((int)LoginType.Username, loginId, salt, hpass, deviceId);
                    returnCode = (success) ? (short)ErrorCode.Ok : (short)ErrorCode.PasswordChangeFailed;
                }
                else
                    returnCode = (short)ErrorCode.UserOrPassFailed;
            }

            var opParameter = new Dictionary<byte, object>();
            opParameter.Add((byte)ParameterCode.LoginId, loginId);
            opParameter.Add((byte)ParameterCode.Password, newPass);
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = returnCode,
                Parameters = opParameter
            };
        }*/

        /*private async Task<OperationResponse> HandleUIDShiftAsync(OperationRequest operationRequest)
        {
            string oldLoginTypeStr = operationRequest.Parameters[(byte)ParameterCode.LoginType].ToString();
            string oldLoginId = operationRequest.Parameters[(byte)ParameterCode.LoginId].ToString();
            string newLoginTypeStr = operationRequest.Parameters[(byte)ParameterCode.ExtraParam].ToString();
            string newLoginId = operationRequest.Parameters[(byte)ParameterCode.Username].ToString();
            string pass = "", email = "";
            if(operationRequest.Parameters.ContainsKey((byte)ParameterCode.Password))
                pass = operationRequest.Parameters[(byte)ParameterCode.Password].ToString();
            if(operationRequest.Parameters.ContainsKey((byte)ParameterCode.Email))
                email = operationRequest.Parameters[(byte)ParameterCode.Email].ToString();
            short returnCode = (short)ErrorCode.Ok;

            // Check login validity
            LoginType oldLoginType, newLoginType;
            bool isLoginValid = Enum.TryParse(oldLoginTypeStr, out oldLoginType); 
            isLoginValid = Enum.TryParse(newLoginTypeStr, out newLoginType);
            string loginId = newLoginId; // Login ID of player
            Guid userId = Guid.Empty;
            if(isLoginValid && newLoginType != LoginType.EstablishConnection)
            {
                List<Dictionary<string, object>> loginIdList = await MasterApplication.dbRepository.UserAccount.GetByLogin((int)oldLoginType, oldLoginId);
                if(loginIdList.Count > 0) // Account exist able to bind
                {
                    string deviceId = operationRequest.Parameters[(byte)ParameterCode.DeviceId].ToString();
                    userId = new Guid(loginIdList[0]["userid"].ToString());
                    bool success = false, bindAccExist = true;
                    if(newLoginType == LoginType.Username) // Is a username login bind
                    {
                        string salt = Guid.NewGuid().ToString().Replace("-", "");
                        string hpass = GetHashedID(salt+pass);
                        List<Dictionary<string, object>> loginChk = await MasterApplication.dbRepository.UserAccount.GetByLogin((int)newLoginType, loginId);
                        if(loginChk.Count <= 0) // Only bind if account does not exist
                        {
                            success = await MasterApplication.dbRepository.UserAccount.BindUserName(userId, (int)newLoginType, loginId, salt, hpass, deviceId);
                            bindAccExist = false;
                            // Send mail if success
                            if(success && !string.IsNullOrEmpty(email))
                            {
                                try
                                {
                                    MailMessage mail = new MailMessage();
                                    mail.From = new MailAddress(MasterServerSettings.Default.EmailAddr);
                                    mail.To.Add(email);
                                    mail.Subject = "如來神掌帳號繼承成功通知";
                                    mail.Body = string.Format("親愛的玩家，你好。\n您已經成功繼承帳號至官方帳號，你所設定的帳號密碼如下：\n帳號：{0}\n密碼：{1}\n請妥善保管帳號密碼，若有疑問可聯繫客服人員。", 
                                                              newLoginId, pass);
 
                                    mail.BodyEncoding = Encoding.UTF8;
                                    SmtpClient smtpServer = new SmtpClient(MasterServerSettings.Default.SmtpServer);
                                    smtpServer.Port = 25;
                                    smtpServer.Credentials = new NetworkCredential(MasterServerSettings.Default.EmailAddr, 
                                                                                   MasterServerSettings.Default.EmailPassword);
                                    smtpServer.EnableSsl = true;
                                    //ServicePointManager.ServerCertificateValidationCallback = 
                                    //    delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
                                    //        { return true; };
                                    smtpServer.Send(mail);
                                }
                                catch(SmtpException e)
                                {
                                    string eStr = e.ToString();
                                }
                            }
                        }
                    }
                    else// Social binding
                    {
                        if(oldLoginType == LoginType.Device)  
                        {
                            switch(newLoginType) // Check for social type here
                            {
                                case LoginType.Facebook:
                                    IDictionary<string, object> tokenData = await GetFBTokenData(newLoginId);
                                    if(tokenData != null)
                                    {
                                        bool valid = (bool)tokenData["is_valid"];
                                        if(valid)
                                        {
                                            loginId = tokenData["user_id"].ToString();
                                            List<Dictionary<string, object>> loginChk = await MasterApplication.dbRepository.UserAccount.GetByLogin((int)newLoginType, loginId);
                                            if(loginChk.Count <= 0) // Only bind if account does not exist
                                            {
                                                success = await MasterApplication.dbRepository.UserAccount.BindAccount(userId, (int)newLoginType, loginId, deviceId);
                                                bindAccExist = false;
                                            }
                                        }
                                    }
                                    break;
                                case LoginType.Google:
                                    AuthResponse authResponse = GoogleOAuth2.ExchangeForToken(newLoginId, MasterServerSettings.Default.GoogleClientId, 
                                                                                              MasterServerSettings.Default.GoogleClientSecret, 
                                                                                              MasterServerSettings.Default.GoogleRedirectURI);
                                    if(authResponse.AccessToken != null && authResponse.idToken != null)
                                    {
                                        TokenInfo tokenInfo = GoogleOAuth2.GetTokenInfo(authResponse.idToken);
                                        if(tokenInfo.userId != null)
                                        {
                                            loginId = tokenInfo.userId;
                                            List<Dictionary<string, object>> loginChk = await MasterApplication.dbRepository.UserAccount.GetByLogin((int)newLoginType, loginId);
                                            if(loginChk.Count <= 0) // Only bind if account does not exist
                                            {
                                                success = await MasterApplication.dbRepository.UserAccount.BindAccount(userId, (int)newLoginType, loginId, deviceId);
                                                bindAccExist = false;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    if(success) // Bind account success
                    {
                        //Note: Have to log from game application
                        ZRPC.MasterToGameRPC.LogUIDShift(userId.ToString("N"), (int)oldLoginType, oldLoginId, (int)newLoginType, newLoginId, 
                                                         masterapp.GameServers);
                    }
                    else
                    {
                        returnCode = (short)ErrorCode.UIDShiftFailed;
                    }
                    if(bindAccExist)
                        returnCode = (short)ErrorCode.UIDShiftAccExist;
                }
                else
                    returnCode = (short)ErrorCode.UsernameDoesNotExist;
            }
            else
                returnCode = (short)ErrorCode.InvalidLoginType;

            var opParameter = new Dictionary<byte, object>();
            opParameter.Add((byte)ParameterCode.LoginType, newLoginType);
            opParameter.Add((byte)ParameterCode.LoginId, loginId);
            if(!string.IsNullOrEmpty(pass))
                opParameter.Add((byte)ParameterCode.Password, pass);
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = returnCode,
                Parameters = opParameter
            };
        }*/

        private OperationResponse HandleGetServerList(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var request = new GetServerListRequest(this.Protocol, operationRequest);
            if (this.ValidateOperation(request, sendParameters) == false)
                return null;
            if (!this.application.IsVersionMatch(((ClientPlatform)request.ClientPlatform).ToString(), request.AppVersion))
            {
                return new OperationResponse
                {
                    OperationCode = operationRequest.OperationCode,
                    ReturnCode = (short)ErrorCode.InvalidClientVer,
                    Parameters = new Dictionary<byte, object>()
                };
            }

            short returnCode = (short)ErrorCode.Ok;
            var opParameter = new Dictionary<byte, object>();
            opParameter.Add((byte)ParameterCode.ServerList, application.mMasterGame.GetSerializedServer());
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = returnCode,
                Parameters = opParameter
            };
        }

        private async Task<IDictionary<string, object>> GetFBTokenData(string accessToken)
        {
            var fbClient = new FacebookClient();
            // FB Example - Generating an App Access Token
            // FB Example - Verify the client token using app token                
            IDictionary<string, object> tokenVerify = null;
            try
            {
                string url = string.Format("/debug_token?input_token={0}&access_token={1}", accessToken, application.FbAppToken);
                tokenVerify = await fbClient.GetTaskAsync(url) as IDictionary<string, object>;
            }
            catch (FacebookOAuthException e)
            {
                log.DebugFormat("FacebookOAuthException: {0}", e.ToString());
                return null;
            }
            if (tokenVerify != null)
                return tokenVerify["data"] as IDictionary<string, object>;

            return null;
        }

        // Handle user login request
        public async Task<OperationResponse> HandleAuthenticateAsync(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var request = new AuthenticateLoginRequest(this.Protocol, operationRequest);
            if (this.ValidateOperation(request, sendParameters) == false)
                return null;
            if (!this.application.IsVersionMatch(((ClientPlatform)request.ClientPlatform).ToString(), request.AppVersion))
            {
                return new OperationResponse
                {
                    OperationCode = operationRequest.OperationCode,
                    ReturnCode = (short)ErrorCode.InvalidClientVer,
                    Parameters = new Dictionary<byte, object>()
                };
            }

            string login_type_str = GetUserFromParams(request.ClientAuthenticationParams);
            string token_str = GetTokenFromParams(request.ClientAuthenticationParams);
            string extra_param_str = GetExtraParam(request.ClientAuthenticationParams);

            ErrorCode returnCode = ErrorCode.InvalidAuthentication;
            LoginAuthType currloginType; // Login system authentication 
            bool isLoginValid = Enum.TryParse(login_type_str, out currloginType); // Check if valid login type
            string loginId = ""; // Login ID of player
            string userId = Guid.Empty.ToString();
            if (isLoginValid) // Login type is valid
            {
                List<Dictionary<string, object>> loginIdList = null;
                switch (currloginType)
                {
                    case LoginAuthType.EstablishConnection:
                        // For authentication that only to establish connection
                        loginId = token_str;
                        if (!string.IsNullOrEmpty(loginId))
                        {
                            LoginAuthType tmpType;
                            bool isValidEnum = Enum.TryParse(loginId, out tmpType); // Check if valid login type
                            // If is not a login connection
                            if (!isValidEnum && !loginId.Equals("Register") && !loginId.Equals("VerifyLoginIdRegister") &&
                                !loginId.Equals("ServerList"))
                            {
                                loginId = GetHashedID(string.Format("{0}{1}", DateTime.Now.ToString(), loginId)); // Generate ID
                            }
                        }
                        returnCode = ErrorCode.Ok;
                        break;
//#if ACCOUNT_REGISTER
//                    case LoginType.Device:
//                        loginId = token_str;
//                        loginIdList = await MasterApplication.dbGMRepository.PlayerAccount.GetAccountByLoginAsync((int)LoginType.Device, loginId);
//                        if (loginIdList.Count <= 0) // Account does not exist
//                        {
//                            string deviceID = extra_param_str;
//                            AccountInsertResult? result =
//                                await MasterApplication.dbGMRepository.PlayerAccount.InsertAccountAsync((int)LoginType.Device, loginId, "", deviceID);
//                            if (result.HasValue)
//                            {
//                                userId = result.Value.UserId;
//                                log.DebugFormat("new user {0}", userId);
//                            }
//                        }
//                        else // Account exist
//                        {
//                            userId = new Guid(loginIdList[0]["userid"].ToString());
//                        }
//                        returnCode = ErrorCode.Ok;
//                        break;
//#endif
                    case LoginAuthType.Username:
                        loginId = token_str;
                        loginIdList = await MasterApplication.dbGMRepository.PlayerAccount.GetAccountByLoginAsync((int)LoginAuthType.Username, loginId);
                        if (loginIdList.Count <= 0) // Account does not exist
                            returnCode = ErrorCode.UsernameDoesNotExist;
                        else // Account exist
                        {
                            string password = extra_param_str;
                            string dbPassword = loginIdList[0]["password"].ToString();
                            if (dbPassword.Equals(password))
                            {
                                object dtfreeze = loginIdList[0]["dtfreeze"];
                                DateTime freezedt = dtfreeze == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dtfreeze);
                                userId = loginIdList[0]["userid"].ToString();
                                returnCode = (DateTime.Now >= freezedt) ? ErrorCode.Ok : ErrorCode.UserBlocked;
                            }
                            else
                                returnCode = ErrorCode.UserOrPassFailed;
                        }
                        break;

                    case LoginAuthType.Facebook:
                        string accessToken = token_str;
                        IDictionary<string, object> tokenData = await GetFBTokenData(accessToken);
                        if (tokenData != null)
                        {
                            bool valid = (bool)tokenData["is_valid"];
                            if (valid)
                            {
                                loginId = tokenData["user_id"].ToString();
                                loginIdList = await MasterApplication.dbGMRepository.PlayerAccount.GetAccountByLoginAsync((int)LoginAuthType.Facebook, loginId);
                                if (loginIdList.Count <= 0) // Account does not exist
                                {
                                    string deviceID = extra_param_str;
                                    AccountInsertResult? result = await MasterApplication.dbGMRepository.PlayerAccount.InsertAccountAsync((int)LoginAuthType.Facebook, loginId, "", deviceID);
                                    if (result.HasValue)
                                    {
                                        userId = result.Value.UserId.ToString();
                                        log.DebugFormat("new user {0}", userId);
                                    }
                                }
                                else // Account exist
                                {
                                    userId = loginIdList[0]["userid"].ToString();
                                }
                                returnCode = (short)ErrorCode.Ok;
                            }
                        }
                        // FB Example - Generate long lived token
                        //var llToken = await fbClient.GetTaskAsync("/oauth/access_token?grant_type=fb_exchange_token&client_id=" + fbAppID +
                        //                                           "&client_secret=" + fbAppSecret + "&fb_exchange_token=" +
                        //                                           accessToken) as IDictionary<string, object>;
                        //var longLivedToken = llToken["access_token"];
                        // FB Example - Get access token app info
                        //var tokenChk = await fbClient.GetTaskAsync("/app/?access_token="+accessToken) as IDictionary<string, object>;
                        break;

                    //case LoginType.Google:
                    //    string authCode = getPassToken(operationRequest);
                    //    authCode = Uri.UnescapeDataString(authCode); // Unescape to get correct token
                    //    AuthResponse authResponse = GoogleOAuth2.ExchangeForToken(authCode, MasterServerSettings.Default.GoogleClientId, 
                    //                                                              MasterServerSettings.Default.GoogleClientSecret, 
                    //                                                              MasterServerSettings.Default.GoogleRedirectURI);
                    //    if(authResponse.AccessToken != null && authResponse.idToken != null)
                    //    {
                    //        TokenInfo tokenInfo = GoogleOAuth2.GetTokenInfo(authResponse.idToken);
                    //        if(tokenInfo.userId != null)
                    //        {
                    //            loginId = tokenInfo.userId;
                    //            loginIdList = await MasterApplication.dbRepository.UserAccount.GetByLogin((int)LoginType.Google, loginId);
                    //            if(loginIdList.Count <= 0) // Account does not exist
                    //            {
                    //                string deviceID = getExtraParam(operationRequest);
                    //                UserInsertResult? result = await MasterApplication.dbRepository.UserAccount.InsertNewAsync((int)LoginType.Google, loginId, deviceID);
                    //                if(result.HasValue)
                    //                {
                    //                    userId = result.Value.UserID.ToString();
                    //                    UID = result.Value.UID;
                    //                    log.DebugFormat("new user {0}", userId);
                    //                }
                    //            }
                    //            else // Account exist
                    //            {
                    //                userId = loginIdList[0]["userid"].ToString();
                    //                long.TryParse(loginIdList[0]["uid"].ToString(), out UID);
                    //            }
                    //            returnCode = (short)ErrorCode.Ok;
                    //        }
                    //        else returnCode = (short)ErrorCode.InvalidAuthentication;
                    //    }
                    //    else returnCode = (short)ErrorCode.InvalidAuthentication;
                    //    break;

                    default:
                        isLoginValid = false;
                        returnCode = ErrorCode.UsernameDoesNotExist;
                        break;
                }
            }

            // Send response back to client      
            var opParameter = new Dictionary<byte, object>();
            opParameter.Add((byte)ParameterCode.LoginType, currloginType);
            opParameter.Add((byte)ParameterCode.LoginId, loginId);
            opParameter.Add((byte)ParameterCode.ServerList, application.mMasterGame.GetSerializedServer());

            if (isLoginValid && returnCode == ErrorCode.Ok && currloginType != LoginAuthType.EstablishConnection) // Normal Authentication
            {
                if (application.mMasterGame.DCUsers.ContainsKey(userId))
                    returnCode = ErrorCode.DuplicateLogin;
                else
                {
                    Guid cookie = Guid.NewGuid();
                    string cookieStr = cookie.ToString();
                    myCookie = cookieStr;
                    myUserId = userId;
                    opParameter.Add((byte)ParameterCode.CookieId, cookieStr);
                    opParameter.Add((byte)ParameterCode.UniqueId, userId);
                    if (currloginType == LoginAuthType.Username)
                        opParameter.Add((byte)ParameterCode.Password, extra_param_str);
                    application.mMasterGame.KickPlayer(myUserId, "relogin");
                    var success = MasterApplication.dbGMRepository.PlayerAccount.UpdateCookie(myUserId, myCookie);
                }
            }
     
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = (short)returnCode,
                Parameters = opParameter
            };
        }

        public OperationResponse HandleConnectGameSetup(OperationRequest operationRequest)
        {
            ErrorCode returnCode = ErrorCode.InvalidRequestParameters;
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            if (!string.IsNullOrEmpty(myCookie) && operationRequest.Parameters.ContainsKey((byte)ParameterCode.ServerID))
            {
                int serverid = (int)operationRequest.Parameters[(byte)ParameterCode.ServerID];
                IncomingGamePeer peer = application.mMasterGame.GetPeerByServerId(serverid);
                if (peer == null)
                    returnCode = ErrorCode.ServerOffline;
                else
                {
                    if (application.mMasterGame.DCUsers.ContainsKey(myUserId))
                        returnCode = ErrorCode.DuplicateLogin;
                    else if (peer.Serverconfig.GetServerLoad(peer.Serverconfig.onlinePlayers) == ServerLoad.Full)
                        returnCode = ErrorCode.ServerFull;
                    else
                    {
                        application.mMasterGame.KickPlayer(myUserId, "relogin");
                        peer.ZRPC.MasterToGameRPC.RegCookie(myUserId, myCookie, peer);
                        returnCode = ErrorCode.Ok;
                    }
                }
            }
            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = (short)returnCode,
                Parameters = parameters
            };
        }

        public OperationResponse HandleRpcRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (this.WebRpcHandler != null)
            {
                if (this.HttpRpcCallsLimit > 0 && this.httpForwardedRequests.Increment(1) > this.HttpRpcCallsLimit)
                {
                    var resp = new OperationResponse
                    {
                        OperationCode = operationRequest.OperationCode,
                        ReturnCode = (short)ErrorCode.HttpLimitReached,
                        DebugMessage = HiveErrorMessages.HttpForwardedOperationsLimitReached
                    };

                    this.SendOperationResponse(resp, sendParameters);
                    return null;
                }

                this.WebRpcHandler.HandleCall(this, myUserId, operationRequest, this.unencryptedAuthToken.AuthCookie, sendParameters);
                return null;
            }

            return new OperationResponse
            {
                OperationCode = operationRequest.OperationCode,
                ReturnCode = (short)ErrorCode.OperationDenied,
                DebugMessage = LBErrorMessages.RpcIsNotSetup
            };
        }
        #endregion
    }
}