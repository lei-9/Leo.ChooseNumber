using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;

namespace Leo.ChooseNumber.Core.Redis
{
    public class RedisDataBaseManager
    {
        private static object _connectionMultiplexerLock = new object();

        private static ConnectionMultiplexer _connectionMultiplexer;

        private static ConnectionMultiplexer ConnectionMultiplexer
        {
            get
            {
                lock (_connectionMultiplexerLock)
                {
                    if (_connectionMultiplexer == null)
                    {
                        //todo redis settings "ip:port,password=password"
                        _connectionMultiplexer = ConnectionMultiplexer.Connect("ip:port,password=password");
                    }

                    return _connectionMultiplexer;
                }
            }
        }


        private static IDatabase _database;
        private static object _lock = new object();

        private static IServer _server;
        private static object _serverLock = new object();


        public static IDatabase GetDatabase(int dbIndex = 0, object asyncStatu = null)
        {
            lock (_lock)
            {
                if (_database == null || _database.Database != dbIndex)
                    _database = ConnectionMultiplexer.GetDatabase(dbIndex, asyncStatu);

                return _database;
            }
        }

        public static IServer GetServer(string host = "47.95.1.23", int port = 6379, object asyncStatu = null)
        {
            lock (_serverLock)
            {
                if (_server == null)
                    _server = ConnectionMultiplexer.GetServer(host, port, asyncStatu);

                return _server;
            }
        }
    }
}
