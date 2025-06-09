using SqlSugar;
using Wlniao;

/// <summary>
/// 数据库链接实体
/// </summary>
public class SqlContext : SqlSugarClient
{
    /// <summary>
    /// 表名前缀
    /// </summary>
    public const string Perfix = "logistic_";
    private static string conn_str = null;
    private static DbType conn_type = DbType.MySql;
    private static string ConnStr
    {
        get
        {
            if (!string.IsNullOrEmpty(DbConnectInfo.WLN_CONNSTR_MYSQL))
            {
                conn_type = DbType.MySql;
                conn_str = DbConnectInfo.WLN_CONNSTR_MYSQL;
            }
            else
            {
                conn_type = DbType.Sqlite;
                conn_str = DbConnectInfo.WLN_CONNSTR_SQLITE;
            }
            return conn_str;
        }
    }

    private static DbType ConnType
    {
        get
        {
            return ConnStr == null ? DbType.MySql : conn_type;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public SqlContext() : base(new ConnectionConfig()
    {
        DbType = ConnType,
        ConnectionString = ConnStr,
        IsAutoCloseConnection = false//手动释放是长连接 
    })
    { }
    /// <summary>
    /// 数据库初始化
    /// </summary>
    public static void Init()
    {
        // 非微服务模式运行时才执行数据库迁移
        if (Config.GetConfigs("MicroservicesNode").ToLower() != "true")
        {
            try
            {
                using (var db = new SqlContext())
                {
                    var now = DateTools.GetUnix();
                    db.CodeFirst.InitTables<Models.CertKey>();
                    db.CodeFirst.InitTables<Models.Contract>();
                    db.CodeFirst.InitTables<Models.Owner>();
                    db.CodeFirst.InitTables<Models.User>();
                    db.CodeFirst.InitTables<Models.Operator>();
                    if (!db.Queryable<Models.Owner>().Any())
                    {
                        var rows = new List<Models.Owner>();
                        rows.Add(new Models.Owner { id = 270100001, role = "buyer", name = "发货测试单位", time_create = now });
                        rows.Add(new Models.Owner { id = 270100002, role = "buyer", name = "承运测试单位", time_create = now });
                        db.Storageable<Models.Owner>(rows).ExecuteCommand();
                    }
                    if (!db.Queryable<Models.User>().Any())
                    {
                        var rows = new List<Models.User>();
                        rows.Add(new Models.User { sid = "240623000100001", name = "斜阳草树", mobile = "13038937751", time_create = now });
                        rows.Add(new Models.User { sid = "172510577500001", name = "田荣明", mobile = "13983240200", time_create = now });
                        db.Storageable<Models.User>(rows).ExecuteCommand();
                    }
                }
            }
            catch (Exception ex)
            {
                Wlniao.Log.Loger.Error("Database migrate error:" + ex.Message);
            }
        }
    }
}