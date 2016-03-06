using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject.Infrastructure.Language;

namespace CVScreeningWeb.SignalR
{
    public static class SignalRUserMapper
    {
        private static readonly Dictionary<string, HashSet<string>> _connection = new Dictionary<string, HashSet<string>>();

        public static int Count
        {
            get { return _connection.Count; }
        }

        public static void Add(string key, string connectionId)
        {
            lock (_connection)
            {
                HashSet<string> connections;
                if (!_connection.TryGetValue(key, out connections))
                {
                    connections = new HashSet<string>();
                    _connection.Add(key,connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public static IEnumerable<string> GetConnections(string key)
        {
            HashSet<string> connections;
            if (_connection.TryGetValue(key, out connections))
            {
                return connections.ToEnumerable();
            }
            return Enumerable.Empty<string>();
        }

        public static void Remove(string key, string connectionId)
        {
            lock (_connection)
            {
                HashSet<string> connections;
                if (!_connection.TryGetValue(key, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _connection.Remove(key);
                    }
                }
            }
        }
    }
}