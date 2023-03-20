using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Helpers
{
    public class TokenCancellation
    {
        public static void Reset(CancellationTokenSource resetCacheToken)
        {
            if (resetCacheToken != null && !resetCacheToken.IsCancellationRequested && resetCacheToken.Token.CanBeCanceled)
            {
                resetCacheToken.Cancel();
                resetCacheToken.Dispose();
            }

            resetCacheToken =  new CancellationTokenSource();
        }
    }
}
