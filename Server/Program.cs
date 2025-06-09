using Wlniao;
using Wlniao.Swagger;
using IGeekFan.AspNetCore.Knife4jUI;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseWlniao(o => { o.AllowSynchronousIO = true; });
builder.Services.AddRouting(o => { o.LowercaseUrls = true; });
builder.Services.AddControllers();
builder.Services.AddSwaggerExtend(new System.Collections.Generic.List<ApiGroupInfo> {
    new ApiGroupInfo { Name = "Service", Title = "对外开放服务接口" },
    new ApiGroupInfo { Name = "Control", Title = "内部管理服务接口" }
});
DbConnectInfo.WLN_CONNSTR_MYSQL.IsNullOrEmpty();
var app = builder.Build();
if (true || app.Environment.IsDevelopment())
{
    SqlContext.Init();
    app.UseSwagger();
    app.UseMiddleware<Wlniao.Middleware.ErrorHandling>();
    app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()).UseStaticFiles();
    app.UseKnife4UI(o => { o.RoutePrefix = "swagger"; ApiGroupInfo.GroupInfos.ForEach(group => { o.SwaggerEndpoint(group.ApiUrl, group.Title); }); });
}
app.MapControllers();
app.Run();