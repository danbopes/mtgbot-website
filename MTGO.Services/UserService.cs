using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTGO.Database.Models;
using NHibernate;
using NHibernate.Linq;

namespace MTGO.Services
{
    public class UserService
    {
        private readonly ISession _session;

        public UserService(ISession session)
        {
            _session = session;
        }

        public User FindById(int userId)
        {
            return _session.Query<User>().SingleOrDefault(user => user.Id == userId);
        }

        public User FindByTwitterUsername(string username)
        {
            return _session.Query<User>().SingleOrDefault(user => user.Username == username);
        }

        //public bool IsUserBroadcaster(int userId)
        //{
        //    return _session.Query<User>().Any(user => user.Id == userId && user.Broadcaster != null);
        //}
    }
}
