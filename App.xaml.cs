using DominoGame.Controllers;
using DominoGame.Interfaces;
using DominoGame.Interfaces.Services;
using DominoGame.Models;
using DominoGame.Services;
using DominoGameWPF;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
namespace DominoGame
{

    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IBoardService, BoardService>();
            services.AddSingleton<IPlayerService, PlayerService>();
            services.AddSingleton<ITurnService, TurnService>();

            // Register models
            services.AddSingleton<IBoard, Board>();
            services.AddSingleton<IDeck, Deck>();

            // Register controller
            services.AddSingleton<DominoGameController>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            Services = services.BuildServiceProvider();
        }
    }

}
