using System.Threading.Tasks;
using Grpc.Core;
using Test;

namespace csharp;

public class PtoCService : ServicePtoC.ServicePtoCBase 
{
    public override Task<ReplyCtoC> InviaReport(ReportData request, ServerCallContext context) 
    {
        Form1.Istanza?.Invoke(() => {
            Form1.ShowReport(request);
        });
        return Task.FromResult(new ReplyCtoC { Success = true });
    }
}