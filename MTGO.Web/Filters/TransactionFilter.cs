using System.Data;
using System.Web.Mvc;
using NHibernate;

namespace MTGO.Web.Filters
{
    public class TransactionFilterAttribute : ActionFilterAttribute
    {
        private ISession _session;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            _session = DependencyResolver.Current.GetService<ISession>();
            _session.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            try
            {
                if (filterContext.Exception != null)
                {
                    _session.Transaction.Rollback();
                }
                else
                {
                    _session.Flush();
                    _session.Transaction.Commit();
                }
            }
            finally
            {
                _session.Dispose();
            }
        }
    }
}