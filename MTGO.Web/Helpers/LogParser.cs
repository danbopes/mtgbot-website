using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MTGO.Common;
using MTGO.Common.Helpers;

namespace MTGO.Web.Helpers
{
    public class LogInfo
    {
        public string Username;
        public int Joins;
        public int MessageCount;
        public string[] Last50Messages;

        public override string ToString()
        {
            return String.Format("Username='{0}', Joins='{1}', MessageCount='{2}', LastMessage='{3}'",
                                 Username, Joins, MessageCount, Last50Messages.LastOrDefault());
        }
    }

    public class LogParser
    {
        private readonly string _fileName;
        private readonly string _broadcasterName;
        public LogParser(string broadcasterName)
        {
            _broadcasterName = broadcasterName.ToLower();
            _fileName = @"C:\Users\Administrator\Desktop\ReleaseBeta\ChatLogs\" + _broadcasterName + ".log";
        }

        public async Task<LogInfo> FetchInfoAsync(string username)
        {
            username = username.ToLower();
            var joins = 0;
            var messages = new List<string>();

            using (var reader = new StreamReader(_fileName))
            {
                string line;
                while ( (line = await reader.ReadLineAsync()) != null )
                {
                    var idx = line.IndexOf(": >> :", StringComparison.Ordinal);

                    if (idx <= -1) continue;

                    line = line.Substring(idx + 6);

                    if (line.StartsWith(String.Format("{0}!{0}@{0}.tmi.twitch.tv JOIN #{1}", username, _broadcasterName)))
                    {
                        joins++;
                        continue;
                    }

                    var idx2 =
                        line.IndexOf(String.Format("{0}!{0}@{0}.tmi.twitch.tv PRIVMSG #{1} :", username,
                                                   _broadcasterName), StringComparison.Ordinal);

                    if (idx2 > -1)
                    {
                        messages.Add(line.Substring(idx2));
                    }
                }
            }

            return new LogInfo
                {
                    Username = username,
                    Joins = joins,
                    MessageCount = messages.Count,
                    Last50Messages = messages.TakeLast(50).ToArray()
                };
        }
    }
}