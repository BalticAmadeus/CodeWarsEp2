using Game.AdminClient.AdminService;
using Game.AdminClient.Infrastructure;
using Game.AdminClient.Models;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GameLogic.UserManagement;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Game.AdminClient.ViewModels
{
    public class GamePreviewViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly IAdministrationServiceGateway _administrationService;
        private readonly IMessageBoxDialogService _messageBoxDialogService;
        private readonly IConfirmationDialogService _confirmationDialogService;

        private int _gameId;

        public GamePreviewViewModel(
            IRegionManager regionManager,
            IAdministrationServiceGateway administrationService,
            IMessageBoxDialogService messageBoxDialogService,
            IConfirmationDialogService confirmationDialogService)
        {
            _regionManager = regionManager;
            _administrationService = administrationService;
            _messageBoxDialogService = messageBoxDialogService;
            _confirmationDialogService = confirmationDialogService;
        }

        #region Properties

        private readonly object _lock = new object();
        private bool _canEnter = true;

        private int _mapWidth;
        private const int RefreshTime = 100;
        private AutoRefreshOperation _autoRefreshOperation;

        public AutoRefreshOperation AutoRefreshOperation
        {
            get
            {
                return _autoRefreshOperation ?? (_autoRefreshOperation = new AutoRefreshOperation(async () =>
                {
                    lock (_lock)
                    {
                        if (!_canEnter)
                            return;

                        _canEnter = !_canEnter;
                    }

                    RequestCount++;

                    try
                    {
                        if (Game == null || Game.GameId != _gameId)
                        {
                            var game = await _administrationService.GetGameAsync(_gameId);
                         
                            Game = new GameViewModel
                            {
                                GameId = game.GameId,
                                Label = game.Label,
                                State = game.State,
                                PlayerCollection = new ObservableCollection<PlayerViewModel>(game.Players.Select(p => new PlayerViewModel
                                {
                                    GameId = p.GameId,
                                    Name = p.Name,
                                    PlayerId = p.PlayerId,
                                    Team = p.Team
                                }))
                            };

                            await ResetMap();

                            IsResumeEnabled = true;
                            IsPauseEnabled = false;
                        }
                        

                        var turn = await _administrationService.GetNextTurnAsync(Game.GameId);
                        LastCallTime = _administrationService.LastCallTime;

                        TurnQueueSize = turn.NumberOfQueuedTurns;

                        switch (turn.Game.State.ToUpper())
                        {
                            case "FINISH":
                                IsPauseEnabled = false;
                                IsResumeEnabled = false;
                                break;
                            case "PAUSE":
                                IsPauseEnabled = false;
                                IsResumeEnabled = true;
                                break;
                            case "PLAY":
                                IsPauseEnabled = true;
                                IsResumeEnabled = false;
                                break;
                        }

                        if (turn.TurnNumber == -1 && turn.NumberOfQueuedTurns == -1)
                        {
                            await ResetMap();
                        }
                        else if (turn.TurnNumber != -1)
                        {
                            TurnNumber = turn.TurnNumber;

                            ChangeMap(turn.MapChanges, turn.PlayerStates);

                            var playerCollection = new ObservableCollection<PlayerStateViewModel>();

                            for (int i = 0; i < Game.PlayerCollection.Count; i++)
                            {
                                playerCollection.Add(new PlayerStateViewModel
                                {
                                    ColorId = i,
                                    Condition = turn.PlayerStates[i].Condition,
                                    Comment = turn.PlayerStates[i].Comment,
                                    Player = Game.PlayerCollection[i],
                                    Score = turn.PlayerStates[i].Score,
                                });
                            }

                            PlayerCollection = playerCollection;
                        }
                        else
                        {
                            // Observer call timeout, we can do more things here
                            if (UserSettings.AutoOpen)
                            {
                                var games = await _administrationService.ListGamesAsync();
                                var lastActiveGameId = games
                                            .OrderByDescending(g => g.GameId)
                                            .Where(g => "Play".Equals(g.State, StringComparison.OrdinalIgnoreCase) || "Pause".Equals(g.State, StringComparison.OrdinalIgnoreCase))
                                            .Select(g => g.GameId)
                                            .FirstOrDefault();

                                if (lastActiveGameId > 0 && lastActiveGameId != _gameId)
                                {
                                    // This will force game switch on next timer tick
                                    _gameId = lastActiveGameId;
                                }
                            }
                        }

                        for (int i = 0; i < PlayerCollection.Count; i++)
                        {
                            PlayerCollection[i].SlowTurn = turn.SlowPlayers.Contains(i);
                        }
                    }
                    catch (Exception e)
                    {
                        AutoRefreshOperation.IsAutoRefreshEnabled = false;
                        _regionManager.RequestNavigate("MainRegion", new Uri("LobbyView", UriKind.Relative));
                        //_messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }

                    lock (_lock)
                    {
                        _canEnter = !_canEnter;
                    }
                }, RefreshTime));
            }
        }

        private void ChangeMap(IList<MapChange> mapChanges, IList<PlayerState> playerStates)
        {
            SetRatMonikers(playerStates);
            foreach (var mapChange in mapChanges)
            {
                switch (mapChange.ChangeType)
                {
                    case MapChangeType.Eat:
                        CookieSprite cookie;
                        if (_cookieSprites.TryGetValue(mapChange.Target.Value, out cookie))
                            cookie.Eat();
                        break;
                    case MapChangeType.Demolish:
                        SetupDemolition(mapChange.Target.Value);
                        break;
                    case MapChangeType.Move:
                        _ratSprites[mapChange.PlayerIndex][mapChange.RatIndex].Move(mapChange.Target.Value);
                        break;
                    case MapChangeType.Casualty:
                        _ratSprites[mapChange.PlayerIndex][mapChange.RatIndex].Die();
                        break;
                    case MapChangeType.Explode:
                        SetupExplosion(mapChange.Target.Value);
                        break;
                }
            }
        }

        private void SetupDemolition(Point target)
        {
            var duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.5));
            var ranim = new RectAnimation(new System.Windows.Rect(target.X, target.Y, 1, 1), duration);
            var gmRect = new RectangleGeometry(new System.Windows.Rect(target.X + 0.5, target.Y + 0.5, 0, 0));
            _wallBreakLayer.Children.Add(gmRect);
            gmRect.BeginAnimation(RectangleGeometry.RectProperty, ranim);
        }

        private void SetupExplosion(Point target)
        {
            var gg = new GeometryGroup();
            var center = new System.Windows.Point(target.X + 0.5, target.Y + 0.5);
            EllipseGeometry e1, e2;
            gg.Children.Add(e1 = new EllipseGeometry(center, 0.1, 0.1));
            gg.Children.Add(e2 = new EllipseGeometry(center, 0.0, 0.0));
            var dw = new GeometryDrawing(Brushes.White, null, gg);
            _explosionLayer.Children.Add(dw);
            var duration = new System.Windows.Duration(TimeSpan.FromSeconds(0.5));
            var anim1 = new DoubleAnimation(0.5, duration);
            var anim2 = new DoubleAnimation(0.5, duration);
            e1.BeginAnimation(EllipseGeometry.RadiusXProperty, anim1);
            e1.BeginAnimation(EllipseGeometry.RadiusYProperty, anim1);
            e2.BeginAnimation(EllipseGeometry.RadiusXProperty, anim2);
            e2.BeginAnimation(EllipseGeometry.RadiusYProperty, anim2);
        }

        private void SetRatMonikers(IList<PlayerState> playerStates)
        {
            var points = new Dictionary<Point, Tuple<int, RatSprite>>();
            for (int i = 0; i < playerStates.Count; i++)
            {
                for (int j = 0; j < playerStates[i].RatPositions.Length; j++)
                {
                    RatPosition rat = playerStates[i].RatPositions[j];
                    _ratSprites[i][rat.RatId].SetMoniker(0);
                    Tuple<int, RatSprite> rec;
                    if (!points.TryGetValue(rat.Position, out rec))
                    {
                        points[rat.Position] = Tuple.Create(0, _ratSprites[i][rat.RatId]);
                        continue;
                    }
                    if (rec.Item1 == 0)
                    {
                        rec.Item2.SetMoniker(1);
                        _ratSprites[i][rat.RatId].SetMoniker(2);
                        points[rat.Position] = new Tuple<int, RatSprite>(3, null);
                    }
                    else
                    {
                        _ratSprites[i][rat.RatId].SetMoniker(rec.Item1);
                        points[rat.Position] = new Tuple<int, RatSprite>(rec.Item1 + 1, null);
                    }
                }
            }
        }

        private async Task ResetMap()
        {
            MapResetCount++;

            var match = await _administrationService.GetMatchAsync(Game.GameId);
            _mapWidth = match.Map.Width;


            // Map background & cookies
            var gmWalls = new GeometryGroup();
            var dwCookies = new DrawingGroup();
            _cookieSprites = new Dictionary<Point, CookieSprite>();

            for (int i = 0; i < match.Map.Rows.Count; i++)
            {
                for (int j = 0; j < match.Map.Rows[i].Length; j++)
                {
                    switch (match.Map.Rows[i][j])
                    {
                        case '#':
                            gmWalls.Children.Add(new RectangleGeometry(new System.Windows.Rect(j, i, 1, 1)));
                            break;
                        case '.':
                            var cookie = new CookieSprite(j, i);
                            dwCookies.Children.Add(cookie.SpriteDrawing);
                            _cookieSprites[new Point(j, i)] = cookie;
                            break;
                    }
                }
            }

            var dwWalls = new GeometryDrawing(Brushes.Blue, null, gmWalls);

            _wallBreakLayer = new GeometryGroup();
            var dwWallBreakLayer = new GeometryDrawing(Brushes.Black, null, _wallBreakLayer);

            _ratSprites = new RatSprite[match.PlayerStates.Count][];
            for (int i = 0; i < match.PlayerStates.Count; i++)
            {
                PlayerState ps = match.PlayerStates[i];
                if (ps.RatPositions.Length == 0)
                {
                    _ratSprites[i] = new RatSprite[0];
                    continue;
                }
                int count = ps.RatPositions.Max(r => r.RatId) + 1;
                _ratSprites[i] = new RatSprite[count];
                foreach (RatPosition rp in ps.RatPositions)
                {
                    _ratSprites[i][rp.RatId] = new RatSprite(rp.Position, i);
                }
            }

            //_ratSprites.SelectMany(r => r).Where(r => r != null).Select((r, i) => { r.SetMoniker(i + 1); return 1; }).Count();
            SetRatMonikers(match.PlayerStates);

            _explosionLayer = new DrawingGroup();

            var bgDrawing = new DrawingGroup();
            bgDrawing.Children.Add(dwWalls);
            bgDrawing.Children.Add(dwWallBreakLayer);
            foreach (RatSprite rat in _ratSprites.SelectMany(p => p).Where(p => p != null))
            {
                bgDrawing.Children.Add(rat.SpriteDrawing);
            }
            bgDrawing.Children.Add(dwCookies);
            bgDrawing.Children.Add(_explosionLayer);

            var bgImage = new DrawingImage(bgDrawing);

            BackgroundImage = bgImage;


            // Player stats

            var playerCollection = new ObservableCollection<PlayerStateViewModel>();
            for (int i = 0; i < Game.PlayerCollection.Count; i++)
            {
                playerCollection.Add(new PlayerStateViewModel
                {
                    ColorId = i,
                    Condition = match.PlayerStates[i].Condition,
                    Comment = match.PlayerStates[i].Comment,
                    Player = Game.PlayerCollection[i],
                    Score = match.PlayerStates[i].Score
                });
            }

            PlayerCollection = playerCollection;
        }

        private GameViewModel _game;
        public GameViewModel Game
        {
            get { return _game; }
            set { SetProperty(ref _game, value); }
        }

        private PlayerStateViewModel _selectedPlayer;
        public PlayerStateViewModel SelectedPlayer
        {
            get { return _selectedPlayer; }
            set { SetProperty(ref _selectedPlayer, value); }
        }

        private ObservableCollection<PlayerStateViewModel> _playerCollection;
        public ObservableCollection<PlayerStateViewModel> PlayerCollection
        {
            get { return _playerCollection; }
            set { SetProperty(ref _playerCollection, value); }
        }

        private int _mapResetCount;
        public int MapResetCount
        {
            get { return _mapResetCount; }
            set { SetProperty(ref _mapResetCount, value); }
        }

        private int _turnNumber;
        public int TurnNumber
        {
            get { return _turnNumber; }
            set { SetProperty(ref _turnNumber, value); }
        }

        private double _turnQueueSize;
        public double TurnQueueSize
        {
            get { return _turnQueueSize; }
            set { SetProperty(ref _turnQueueSize, value); }
        }

        private double _requestCount;
        public double RequestCount
        {
            get { return _requestCount; }
            set { SetProperty(ref _requestCount, value); }
        }

        private bool _isPauseEnabled;
        public bool IsPauseEnabled
        {
            get { return _isPauseEnabled; }
            set { SetProperty(ref _isPauseEnabled, value); }
        }

        private bool _isResumeEnabled;
        public bool IsResumeEnabled
        {
            get { return _isResumeEnabled; }
            set { SetProperty(ref _isResumeEnabled, value); }
        }

        private double _canvasWidth;
        public double CanvasWidth
        {
            get { return _canvasWidth; }
            set { SetProperty(ref _canvasWidth, value); }
        }

        private double _canvasHeight;
        public double CanvasHeight
        {
            get { return _canvasHeight; }
            set { SetProperty(ref _canvasHeight, value); }
        }

        private double _lastCallTime;
        public double LastCallTime
        {
            get { return _lastCallTime; }
            set { SetProperty(ref _lastCallTime, value); }
        }

        public bool _isAutoOpenEnabled;
        public bool IsAutoOpenEnabled
        {
            get
            {
                _isAutoOpenEnabled = UserSettings.AutoOpen;
                return _isAutoOpenEnabled;
            }
            set
            {
                UserSettings.AutoOpen = value;
                SetProperty(ref _isAutoOpenEnabled, UserSettings.AutoOpen);
            }
        }

        private ImageSource _backgroundImage;
        public ImageSource BackgroundImage
        {
            get { return _backgroundImage; }
            set { SetProperty(ref _backgroundImage, value); }
        }

        private GeometryGroup _wallBreakLayer;
        private DrawingGroup _explosionLayer;
        private Dictionary<Point, CookieSprite> _cookieSprites;
        private RatSprite[][] _ratSprites; /* [player][rat] */

        #endregion

        #region Visibility

        public bool ShowResumeGame
        {
            get
            {
                return UserSettings.Role == TeamRole.Normal || UserSettings.Role == TeamRole.Power;
            }
        }

        public bool ShowPauseGame
        {
            get
            {
                return UserSettings.Role == TeamRole.Normal || UserSettings.Role == TeamRole.Power;
            }
        }

        public bool ShowDropPlayer
        {
            get
            {
                return UserSettings.Role == TeamRole.Normal || UserSettings.Role == TeamRole.Power;
            }
        }

        public bool ShowAutoOpen
        {
            get
            {
                return UserSettings.Role == TeamRole.Observer;
            }
        }

        #endregion

        #region Commands

        private AsyncDelegateCommandWrapper _closeCommand;
        public AsyncDelegateCommandWrapper CloseCommand
        {
            get
            {

                return _closeCommand ?? (_closeCommand = new AsyncDelegateCommandWrapper(() =>
                {
                    UserSettings.AutoOpen = false;
                    _regionManager.RequestNavigate("MainRegion", new Uri("LobbyView", UriKind.Relative));
                }));
            }
        }

        private AsyncDelegateCommandWrapper _pauseGameCommand;
        public AsyncDelegateCommandWrapper PauseGameCommand
        {
            get
            {
                return _pauseGameCommand ?? (_pauseGameCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    if (!IsPauseEnabled)
                        return;

                    IsPauseEnabled = false;
                    IsResumeEnabled = true;

                    try
                    {
                        await _administrationService.PauseGameAsync(Game.GameId);
                    }
                    catch (Exception e)
                    {
                        IsResumeEnabled = false;
                        IsPauseEnabled = true;

                        _messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _resumeGameCommand;
        public AsyncDelegateCommandWrapper ResumeGameCommand
        {
            get
            {
                return _resumeGameCommand ?? (_resumeGameCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    if (!IsResumeEnabled)
                        return;

                    IsResumeEnabled = false;
                    IsPauseEnabled = true;

                    try
                    {
                        await _administrationService.ResumeGameAsync(Game.GameId);
                    }
                    catch (Exception e)
                    {
                        IsPauseEnabled = false;
                        IsResumeEnabled = true;

                        _messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _dropPlayerCommand;
        public AsyncDelegateCommandWrapper DropPlayerCommand
        {
            get
            {
                return _dropPlayerCommand ?? (_dropPlayerCommand = new AsyncDelegateCommandWrapper(async () =>
                {
                    if (SelectedPlayer == null)
                        return;

                    string message = string.Format("Are you sure to drop player {0} {1}?", SelectedPlayer.Player.Team, SelectedPlayer.Player.Name);
                    bool result = _confirmationDialogService.OpenDialog("Drop Player", message);
                    if (!result)
                        return;

                    var selectedPlayer = SelectedPlayer;
                    SelectedPlayer = null;

                    try
                    {
                        await _administrationService.DropPlayer(Game.GameId, selectedPlayer.Player.PlayerId);
                    }
                    catch (Exception e)
                    {
                        _messageBoxDialogService.OpenDialog(e.Message, "Error");
                    }
                }));
            }
        }

        private AsyncDelegateCommandWrapper _showInfoComamnd;
        public AsyncDelegateCommandWrapper ShowInfoCommand
        {
            get
            {
                return _showInfoComamnd ?? (_showInfoComamnd = new AsyncDelegateCommandWrapper(async () =>
                {
                    var response = await _administrationService.GetLiveInfo(Game.GameId);
                    string message;

                    using (var stringStream = new StringWriter())
                    {
                        var requestSerializer = new XmlSerializer(typeof(GetLiveInfoResp));
                        requestSerializer.Serialize(stringStream, response);
                        message = stringStream.ToString();
                    }

                    _messageBoxDialogService.OpenDialog(message, "Info");
                }));
            }
        }

        #endregion

        #region Navigation

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _gameId = (int)navigationContext.Parameters["SelectedGameId"];

            TurnQueueSize = 0;

            AutoRefreshOperation.Resume();

            TurnNumber = 0;
            MapResetCount = 0;

            RequestCount = 0;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            AutoRefreshOperation.Pause();
        }

        #endregion
    }
}
