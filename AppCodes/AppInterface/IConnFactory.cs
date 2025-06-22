
namespace mvcDapper3.AppCodes.AppInterface;

public interface IConnFactory
{
    IDbConnection CreateConn();
    IDbConnection GetOpenConn();
}
