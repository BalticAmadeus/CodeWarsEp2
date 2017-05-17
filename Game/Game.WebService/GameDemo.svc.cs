using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Game.WebService.GameDemo
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GameDemo : ServiceBase, IGameDemo
    {
        private static Dictionary<string, DemoGame> _games = new Dictionary<string, DemoGame>();
        private static Random _rnd = new Random();

        public GameDemoResp PerformMove(GameDemoReq req)
        {
            try
            {
                string clientAddress = ServiceUtils.GetClientAddress();
                Server.AccessManager.CheckAccess(clientAddress);
            }
            catch (AuthException)
            {
                throw new WebFaultException(System.Net.HttpStatusCode.Forbidden);
            }
            DemoGame game;
            lock (_games)
            {
                string uid = $"{req.GameUid}.{req.YourIndex}";
                if (!_games.TryGetValue(uid, out game))
                {
                    game = new DemoGame(_rnd, req);
                    _games[uid] = game;
                }
            }
            lock (game)
            {
                return game.PerformMove(req);
            }
        }

    }
}
