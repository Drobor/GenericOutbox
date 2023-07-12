namespace PersonService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //builder.WebHost.UseStartup<Startup>();// .ConfigureWebHostDefaults(x => x.UseStartup<Program>());
        var startup = new Startup();
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        startup.Configure(app);


        app.Run();
    }
}