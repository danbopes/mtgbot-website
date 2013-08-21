using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGO.Services.Tournaments
{
    public class IMatch
    {
        ITournPlayer Player1 { get; }
        ITournPlayer Player2 { get; }
        bool ReportedResult { get; }
        void ReportResult(IPlayer player, int wins, int losses, int ties);
    }
}
