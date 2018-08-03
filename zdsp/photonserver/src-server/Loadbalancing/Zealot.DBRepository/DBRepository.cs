using System;
using System.Data.SqlClient;
using ExitGames.Logging;
using Zealot.DBRepository.GM;
using Zealot.DBRepository.GM.Compensate;
using Zealot.DBRepository.GM.NPCStore;

namespace Zealot.DBRepository
{
    public abstract class DBAccessor : IDisposable
    {
        public ILogger Log;

        protected string _connectionstring;
        public string ConnectionString { get { return _connectionstring; } }

        public bool IsConnected = false;
        public int mServerId = 0; //only DBRepository need this value.
        public int mServerLine = 1;

        public DBAccessor()
        {
        }

        /// <summary>
        /// Opens connection pool
        /// </summary>
        /// <param name="connectionstr">
        /// should have "Min Pool Size=1" in order to keep connection alive always
        /// </param>
        public bool Initialize(string connectionstr)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionstr))
                {
                    Log.InfoFormat("Try connect DB: {0}", connectionstr);
                    connection.Open();

                    IsConnected = true;
                    _connectionstring = connectionstr;
                    Log.InfoFormat("Connected to Database");
                    return true;
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                Log.ErrorFormat("Error connecting to Database: {0}", ex.Message);
                return false;
            }
        }

        public void Dispose()
        {
            //do cleanup
        }
    }

    /// <summary>
    /// DBRepository for GMTools
    /// </summary>
    public class DBRepositoryGM : DBAccessor
    {
        private ServerConfigRepository _serverConfigRepo;
        public ServerConfigRepository ServerConfig { get { return _serverConfigRepo; } }

        private PlayerAccountRepository _playerAccountRepo;
        public PlayerAccountRepository PlayerAccount { get { return _playerAccountRepo; } }

        public GMActivityRepository GMActivity { get; set; }

        private ItemMallGMRepository _itemMallRepo;
        public ItemMallGMRepository ItemMall { get { return _itemMallRepo; } }

        private RedemptionRepository _redemptionRepo;
        public RedemptionRepository Redemption { get { return _redemptionRepo; } }     

        private CompensateRepository _compensateRepo;
        public CompensateRepository CompensateRepo { get { return _compensateRepo; } }

        private SystemMessageRepository _systemMesaageRepo;
        public SystemMessageRepository SystemMessageRepo { get { return _systemMesaageRepo; } }

        private SevenDaysRepository _sevenDaysRepo;
        public SevenDaysRepository SevenDays { get { return _sevenDaysRepo; } }

        private WelfareRepository _welfareRepo;
        public WelfareRepository Welfare { get { return _welfareRepo; } }

        public TopUpActivityRepository topUpActivityRepository;

        private TongbaoCostBuffRepository _tongbaocostbuffRepo;
        public TongbaoCostBuffRepository TongbaoCostBuff { get { return _tongbaocostbuffRepo; } }

        private SystemSwitchRepository _systemSwitchRepo;
        public SystemSwitchRepository SystemSwitch { get { return _systemSwitchRepo; } }

        private CurrencyExchangeGMRepository _currencyExchangeGMRepo;
        public CurrencyExchangeGMRepository CurrencyExchangeGM { get { return _currencyExchangeGMRepo; } }

        private TalentGMRepository _talentGMRepo;
        public TalentGMRepository TalentGM { get { return _talentGMRepo; } }

        private TickerTapeRepository _tickertapeRepo;
        public TickerTapeRepository TickerTape { get { return _tickertapeRepo; } }

        private NPCStoreRepository _NPCStoreGMRepo;
        public NPCStoreRepository NPCStoreGMRepo { get { return _NPCStoreGMRepo; } }

        public DBRepositoryGM()
            : base()
        {
            Log = LogManager.GetCurrentClassLogger();
            _serverConfigRepo = new ServerConfigRepository(this);
            _playerAccountRepo = new PlayerAccountRepository(this);
            GMActivity = new GMActivityRepository(this);
            _itemMallRepo = new ItemMallGMRepository(this);
            _redemptionRepo = new RedemptionRepository(this);
            _compensateRepo = new CompensateRepository(this);
            _systemMesaageRepo = new SystemMessageRepository(this);
            _sevenDaysRepo = new SevenDaysRepository(this);
            _welfareRepo = new WelfareRepository(this);
            topUpActivityRepository = new TopUpActivityRepository(this);
            _tongbaocostbuffRepo = new TongbaoCostBuffRepository(this);
            _systemSwitchRepo = new SystemSwitchRepository(this);
            _tickertapeRepo = new TickerTapeRepository(this);
            _currencyExchangeGMRepo = new CurrencyExchangeGMRepository(this);
            _talentGMRepo = new TalentGMRepository(this);
            _NPCStoreGMRepo = new NPCStoreRepository(this);
        }
    }

    /// <summary>
    /// DBRepository for Game Server
    /// </summary>
    public class DBRepository : DBAccessor
    {
        private CharacterRepository _characterRepo;
        public CharacterRepository Character { get { return _characterRepo; } }

        private MailOfflineRepository _mailOfflineRepo;
        public MailOfflineRepository MailOffline { get { return _mailOfflineRepo; } }

        public BossKillerRepository BossKiller { get; set; }
        private RealmLadderRepository _realmLadderRepo;
        public RealmLadderRepository RealmLadder { get { return _realmLadderRepo; } }
        public LadderRepository Ladder { get; set; }

        private GuildRepository _guildRepo;
        public GuildRepository Guild { get { return _guildRepo; } } 

        private AuctionRepository _auctionRepo;
        public AuctionRepository AuctionData { get { return _auctionRepo; } }
        public GameConfigRepository GameConfig { get; set; }

        private ProgressRepository _progressRepo;
        public ProgressRepository Progress { get { return _progressRepo; } }

        public LimitedItemRepository LimitedItem { get; private set; }

        public DBRepository()
            : base()
        {
            Log = LogManager.GetCurrentClassLogger();

            _characterRepo = new CharacterRepository(this);
            _realmLadderRepo = new RealmLadderRepository(this);
            BossKiller = new BossKillerRepository(this);
            Ladder = new LadderRepository(this);
            _guildRepo = new GuildRepository(this);
            _auctionRepo = new AuctionRepository(this);
            _mailOfflineRepo = new MailOfflineRepository(this);
            GameConfig = new GameConfigRepository(this);
            _progressRepo = new ProgressRepository(this);
            LimitedItem = new LimitedItemRepository(this);
        }
    }

    public abstract class DBRepoBase
    {
        protected DBAccessor _dbRepo;
        protected string connectionstring { get { return _dbRepo.ConnectionString; } }
        protected bool isConnected { get { return _dbRepo.IsConnected; } }
        protected ILogger Log;

        protected DBRepoBase(DBAccessor dbrepo)
        {
            _dbRepo = dbrepo;
            Log = _dbRepo.Log;
        }

        protected virtual void HandleQueryException(Exception ex)
        {
            Log.ErrorFormat("DBException: {0}", ex.ToString());
        }
    }
}
