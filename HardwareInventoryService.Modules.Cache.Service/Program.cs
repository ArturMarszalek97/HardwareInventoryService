﻿using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Wcf;
using HardwareInventoryService.DAO;
using HardwareInventoryService.Modules.Cache.Host.Interfaces;
using HardwareInventoryService.Modules.Cache.Host.Services;
using HardwareInventoryService.Modules.Cache.Logic.Interfaces;
using HardwareInventoryService.Modules.Cache.Logic.IRepositories;
using HardwareInventoryService.Modules.Cache.Logic.Logic;
using HardwareInventoryService.Modules.Cache.Logic.Repositories;
using HardwareInventoryService.Modules.Cache.Logic.Services;
using HardwareInventoryService.ServicesReferences.Contracts;
using HardwareInventoryService.ServicesReferences.Interceptors;
using HardwareInventoryService.ServicesReferences.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HardwareInventoryService.Modules.Cache.Service
{
    public static class Program
    {
        private static ILoggerService _logger;

        private static CacheService _cacheService;

        private static IDataProviderService _dataProviderService;

        static void Main(string[] args)
        {
            try
            {
                var container = BuildIOCContainer();
                AutofacHostFactory.Container = container;

                var loggerService = container.Resolve<ILoggerService>();
                _logger = loggerService;

                // self service installer/uninstaller
                if (args != null && args.Length == 1
                                && (args[0][0] == '-' || args[0][0] == '/'))
                    switch (args[0].Substring(1).ToLower())
                    {
                        case "install":
                        case "i":
                            if (!ServiceInstallerUtility.InstallMe())
                                _logger.LogMessage("Failed to install service", LogLevel.Fatal);
                            break;
                        case "uninstall":
                        case "u":
                            if (!ServiceInstallerUtility.UninstallMe())
                                _logger.LogMessage("Failed to uninstall service", LogLevel.Fatal);
                            break;
                        default:
                            _logger.LogMessage(
                                "Unrecognized parameters (allowed: /install and /uninstall, shorten /i and /u)",
                                LogLevel.Error);
                            break;
                    }

                ServiceBase[] ServicesToRun;
                _cacheService = container.Resolve<CacheService>();
                var dataProviderService = container.Resolve<IDataProviderService>();
                _dataProviderService = dataProviderService;
                //AppDomain.CurrentDomain.UnhandledException += MainHandler;
                ServicesToRun = new ServiceBase[]
                {
                    _cacheService
                };

                if (!Environment.UserInteractive)
                {
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {
                    // register console close event
                    //_consoleHandler = ConsoleEventHandler;
                    //SetConsoleCtrlHandler(_consoleHandler, true);

                    _cacheService.Start();
                    GetData(_dataProviderService);


                    //var file = File.ReadAllBytes("C:/Users/Artur/Desktop/pexels-photo-247885.jpeg");
                    //Users users = new Users();
                    //users.Username = "login";
                    //users.Password = "haslo";
                    //users.Photo = file;

                    //_dataProviderService.AddUser(users);

                    Console.Title = AppDomain.CurrentDomain.FriendlyName;

                    Console.WriteLine("Press any key to stop...");
                    Console.ReadKey(true);

                    _cacheService.Stop();
                    //AppDomain.CurrentDomain.UnhandledException -= MainHandler;
                }
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append(@"Failed starting up the application in console mode\n");
                sb.Append(ex.Message);
                if (Environment.UserInteractive) Console.WriteLine(sb);
                _logger.LogMessage(sb.ToString(), LogLevel.Fatal);
                _logger.LogMessage(ex.StackTrace, LogLevel.Trace);
            }

            Environment.Exit(0);
        }

        private static async Task GetData(IDataProviderService dataProviderService)
        {
            await _dataProviderService.GetUsers();
            _dataProviderService.GetItems();
        }

        private static IContainer BuildIOCContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<LoggerService>().As<ILoggerService>()
                .WithParameter("applicationName", "CacheService").SingleInstance();

            builder.RegisterType<MethodLoggingInterceptor>().As<MethodLoggingInterceptor>();
            builder.RegisterType<ExceptionLoggingInterceptor>().As<ExceptionLoggingInterceptor>();
            builder.RegisterType<TimeMeasuringInterceptor>().As<TimeMeasuringInterceptor>();

            builder.RegisterType<SessionRepository>().As<ISessionRepository>().SingleInstance();
            builder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();
            builder.RegisterType<ItemRepository>().As<IItemRepository>().SingleInstance();

            builder.RegisterType<DataProviderService>().As<IDataProviderService>().SingleInstance().EnableInterfaceInterceptors()
                    .InterceptedBy(typeof(MethodLoggingInterceptor))
                    .InterceptedBy(typeof(ExceptionLoggingInterceptor))
                    .InterceptedBy(typeof(TimeMeasuringInterceptor));

            builder.RegisterType<CacheLogicService>().As<ICacheLogicService>()
                .SingleInstance()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(MethodLoggingInterceptor));

            builder.RegisterType<CacheWCFContract>().As<ICacheWCFContract>().SingleInstance().EnableClassInterceptors();

            builder.RegisterType<CacheService>().SingleInstance().EnableClassInterceptors().InterceptedBy(typeof(MethodLoggingInterceptor));

            builder.RegisterType<DataProviderService>().As<IDataProviderService>().SingleInstance().EnableInterfaceInterceptors()
                .InterceptedBy(typeof(MethodLoggingInterceptor))
                .InterceptedBy(typeof(ExceptionLoggingInterceptor))
                .InterceptedBy(typeof(TimeMeasuringInterceptor));

            return builder.Build();
        }
    }
}
