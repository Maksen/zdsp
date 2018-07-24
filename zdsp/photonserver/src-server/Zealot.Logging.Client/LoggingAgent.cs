namespace Zealot.Logging.Client
{
    using Contracts.Requests;
    using log4net;
    using log4net.Appender;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using LogClasses;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public sealed class LoggingAgent
    {
        // Performant variation of the Singleton pattern 
        // with thread safety and lazy instantiation.
        public static LoggingAgent Instance { get { return Nested.instance; } }

        private bool _isEnabled = ConfigurationManager.AppSettings["Zealot.Logging.Client - Enabled"] == "true" ? true : false;

        private readonly string _logFilePath = ConfigurationManager.AppSettings["Zealot.Logging.Client - LogFilePath"];
        private readonly string _failOverLogFilePath = ConfigurationManager.AppSettings["Zealot.Logging.Client - FailOverLogFilePath"];

        private readonly string _loggingServerUpdateTablesUrl = ConfigurationManager.AppSettings["Zealot.Logging.Client - LoggingServerUpdateTablesUrl"];
        private readonly string _loggingServerInsertRecordUrl = ConfigurationManager.AppSettings["Zealot.Logging.Client - LoggingServerInsertRecordUrl"];

        private readonly HttpClient _httpClient = new HttpClient();

        private RequestUpdateTables _requestUpdateTables;

        private ILog _iLog;
        private ILog _iFailOverLog;

        private bool _isFailOverMode = false;

        private LoggingAgent()
        {
            InitLogging();
        }

        public void Init(string serverId)
        {
            if (_isEnabled == false)
            {
                _iLog.Warn("Offline. <Zealot.Logging.Client - Enabled> set to false.");

                return;
            }

            _iLog.Info("Online.");

            LogClass.serverId = serverId;

            _requestUpdateTables = new RequestUpdateTables();
            _requestUpdateTables.tables = new List<RequestUpdateTables.TableData>();
            _requestUpdateTables.serverId = serverId;

            // Init performs all the necessary synchronization and setup from 
            // Zealot.Logging.Client > Zealot.Logging.Server > Database.
            // All calls are intended to be sequential, blocking and synchronous.
            CollateLogTables();

            UpdateLogTables().GetAwaiter().GetResult();

            ProcessFailOverLogFile().GetAwaiter().GetResult();
        }

        public async Task LogAsync(LogClass logClass)
        {
            if (_isEnabled == false)
            {
                return;
            }

            RequestInsertRecord requestInsertRecord = logClass.GetRequestInsertRecord();

            JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };

            string json = JsonConvert.SerializeObject(requestInsertRecord, microsoftDateFormatSettings);

            if (_isFailOverMode == true)
            {
                _iFailOverLog.Info(json);

                return;
            }

            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(_loggingServerInsertRecordUrl, content).ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode == true)
                {
                    HttpContent httpContent = httpResponseMessage.Content;
                    string jsonResponse = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

                    ResponseInsertRecord responseInsertRecord = JsonConvert.DeserializeObject<ResponseInsertRecord>(jsonResponse);

                    if (responseInsertRecord.isSuccessful == false)
                    {
                        _iLog.Error("LogAsync: Insert failed. Check LoggingServer.");

                        ActivateFailOverMode();

                        _iFailOverLog.Info(json);
                    }
                }
                else
                {
                    _iLog.ErrorFormat("LogAsync: HTTP Error. | Status Code: {0}", httpResponseMessage.StatusCode);

                    ActivateFailOverMode();

                    _iFailOverLog.Info(json);
                }
            }
            catch (Exception ex)
            {
                _iLog.ErrorFormat(ex.ToString());

                ActivateFailOverMode();

                _iFailOverLog.Info(json);
            }
        }

        private bool IsTypeAllowed(Type type)
        {
            if (type.IsEnum)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                case TypeCode.Boolean:
                case TypeCode.DateTime:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                //case TypeCode.Object:
                //    if (type == typeof(Guid))
                //    {
                //        return true;
                //    }

                //    return false;
                default:
                    return false;
            }
        }

        private void ActivateFailOverMode()
        {
            _isFailOverMode = true;

            FileAppender fileAppender = new FileAppender();
            fileAppender.Name = "Zealot.Logging.Client.FileAppender";
            fileAppender.File = _failOverLogFilePath;
            fileAppender.Encoding = Encoding.UTF8;
            fileAppender.AppendToFile = true;

            PatternLayout layout = new PatternLayout();
            layout.ConversionPattern = "%message%newline";
            layout.ActivateOptions();

            fileAppender.Layout = layout;
            fileAppender.ActivateOptions();

            _iFailOverLog = LogManager.GetLogger("Zealot.Logging.Client.FailOver");
            Logger logger = (Logger)_iFailOverLog.Logger;
            logger.Level = logger.Hierarchy.LevelMap["INFO"];
            logger.AddAppender(fileAppender);

            _iLog.WarnFormat("ActivateFailOverMode: Logs directed to {0}", _failOverLogFilePath);
        }

        private void CollateLogTables()
        {
            if (_isFailOverMode == true)
            {
                _iLog.Warn("CollateLogTables: Operation denied. FailOverMode Online.");

                return;
            }

            Type logClassType = typeof(LogClass);
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Reflection code. It is slow and should only be called once on init.
            // Retrieves all types derived from Zealot.Logging.Client.LogClasses.LogClass.
            Type[] types = assembly.GetTypes().Where(t => t.IsSubclassOf(logClassType)).ToArray();

            for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
            {
                RequestUpdateTables.TableData tableData = new RequestUpdateTables.TableData();
                tableData.tableName = types[typeIndex].Name;
                tableData.columnName = new List<string>();
                tableData.columnType = new List<string>();

                // Retrieves only properties that's not inherited from LogClass
                List<PropertyInfo> lstPropInfo = new List<PropertyInfo>(types[typeIndex].GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));

                foreach (PropertyInfo propInfo in lstPropInfo)
                {
                    string propName = propInfo.Name;
                    string propType = (propInfo.PropertyType).Name;

                    if (IsTypeAllowed(propInfo.PropertyType) == false)
                    {
                        string exMsg = string.Format("LogClass: {0}, Property Name: {1}, Property Type: {2}", types[typeIndex].Name, propName, propType);
                        throw new NotSupportedException(exMsg);
                    }

                    tableData.columnName.Add(propName);
                    tableData.columnType.Add(propType);
                }

                _requestUpdateTables.tables.Add(tableData);
            }
        }

        private async Task UpdateLogTables()
        {
            if (_isFailOverMode == true)
            {
                _iLog.Warn("UpdateLogTables: Operation denied. FailOverMode Online.");

                return;
            }

            string json = JsonConvert.SerializeObject(_requestUpdateTables);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(_loggingServerUpdateTablesUrl, content).ConfigureAwait(false);

                if (httpResponseMessage.IsSuccessStatusCode == true)
                {
                    HttpContent httpContent = httpResponseMessage.Content;
                    string jsonResponse = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

                    ResponseUpdateTable responseUpdateTable = JsonConvert.DeserializeObject<ResponseUpdateTable>(jsonResponse);

                    if (responseUpdateTable.isSuccessful == false)
                    {
                        _iLog.Error("UpdateLogTables: Update failed. Check LoggingServer.");

                        ActivateFailOverMode();
                    }
                    else
                    {
                        _iLog.Info("UpdateLogTables: Update passed.");
                    }
                }
                else
                {
                    _iLog.ErrorFormat("UpdateLogTables: HTTP Error. | Status Code: {0}", httpResponseMessage.StatusCode);

                    ActivateFailOverMode();
                }
            }
            catch (Exception ex)
            {
                _iLog.ErrorFormat(ex.ToString());

                ActivateFailOverMode();
            }
        }

        private async Task ProcessFailOverLogFile()
        {
            if (_isFailOverMode == true)
            {
                _iLog.Warn("ProcessFailOverLogFile: Operation denied. FailOverMode Online.");

                return;
            }

            if (File.Exists(_failOverLogFilePath) == false)
            {
                _iLog.Info("ProcessFailOverLogFile: No failed logs. Systems Nominal.");

                return;
            }
            else
            {
                _iLog.InfoFormat("ProcessFailOverLogFile: Processing - {0}", _failOverLogFilePath);
            }

            int linesProcessed = 0;

            // Do not use File.ReadLines to read the FailOverLog file.
            // This file can potentially be larger than 650MB which 
            // cannot be loaded fully into memory.
            using (StreamReader sr = new StreamReader(_failOverLogFilePath))
            {
                string line = sr.ReadLine();

                while (line != null)
                {
                    RequestInsertRecord requestInsertRecord = JsonConvert.DeserializeObject<RequestInsertRecord>(line);

                    JsonSerializerSettings microsoftDateFormatSettings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                    };

                    string json = JsonConvert.SerializeObject(requestInsertRecord, microsoftDateFormatSettings);

                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    try
                    {
                        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(_loggingServerInsertRecordUrl, content).ConfigureAwait(false);

                        if (httpResponseMessage.IsSuccessStatusCode == true)
                        {
                            HttpContent httpContent = httpResponseMessage.Content;
                            string jsonResponse = await httpContent.ReadAsStringAsync().ConfigureAwait(false);

                            ResponseInsertRecord responseInsertRecord = JsonConvert.DeserializeObject<ResponseInsertRecord>(jsonResponse);

                            if (responseInsertRecord.isSuccessful == true)
                            {
                                linesProcessed++;

                                line = sr.ReadLine();
                            }
                            else
                            {
                                _iLog.Error("ProcessFailOverLogFile: Insert failed. Check LoggingServer.");

                                ActivateFailOverMode();

                                break;
                            }
                        }
                        else
                        {
                            _iLog.ErrorFormat("ProcessFailOverLogFile: HTTP Error. | Status Code: {0}", httpResponseMessage.StatusCode);

                            ActivateFailOverMode();

                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _iLog.ErrorFormat(ex.ToString());

                        ActivateFailOverMode();

                        break;
                    }
                }
            }

            CleanUpFailOverLogFile(linesProcessed);
        }

        private void CleanUpFailOverLogFile(int linesProcessed)
        {
            if (linesProcessed > 0)
            {
                // Do not use File.ReadLines to read FailOverLog file.
                // This file can potentially be larger than 650MB which 
                // cannot be loaded fully into memory.
                using (StreamReader sr = new StreamReader(_failOverLogFilePath))
                using (StreamWriter sw = new StreamWriter(_failOverLogFilePath + "_temp"))
                {
                    int lineCount = 0;
                    string line = sr.ReadLine();

                    while (line != null)
                    {
                        if (lineCount < linesProcessed)
                        {
                            lineCount++;
                            line = sr.ReadLine();

                            continue;
                        }

                        sw.WriteLine(line);
                        line = sr.ReadLine();
                    }
                }
            }

            if (File.Exists(_failOverLogFilePath + "_temp") == true)
            {
                File.Replace(_failOverLogFilePath + "_temp", _failOverLogFilePath, null);
            }
        }

        private void InitLogging()
        {
            RollingFileAppender appender = new RollingFileAppender();
            appender.Name = "Zealot.Logging.Client.RollingFileAppender";
            appender.File = _logFilePath;
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

            _iLog = LogManager.GetLogger("Zealot.Logging.Client");
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

            internal static readonly LoggingAgent instance = new LoggingAgent();
        }
    }
}
