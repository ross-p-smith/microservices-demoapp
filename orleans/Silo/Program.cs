﻿using Grains;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Orleans.Persistence.AzureStorage;
using System;
using System.Net;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Silo
{
  public class Program
  {
    private static ISiloHost silo;
    private static readonly ManualResetEvent siloStopped = new ManualResetEvent(false);

    static void Main(string[] args)
    {
      // BC - Get the config from appsettings.json, maybe a better way?
      var appSettingsBuilder = new ConfigurationBuilder()
          .SetBasePath(System.IO.Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
      IConfigurationRoot appSettings = appSettingsBuilder.Build();

      silo = new SiloHostBuilder()
          .Configure(options => options.ClusterId = appSettings["Orleans:ClusterId"])
          .UseAzureStorageClustering(options => options.ConnectionString = appSettings["Orleans:ConnectionString"])
          .UseAzureTableReminderService(options => options.ConnectionString = appSettings["Orleans:ConnectionString"])
          .AddAzureTableGrainStorage("grain-store", options => options.ConnectionString = appSettings["Orleans:ConnectionString"])
          .ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000)
          .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ValueGrain).Assembly).WithReferences())
          .ConfigureLogging(builder => builder.SetMinimumLevel((LogLevel)appSettings.GetValue<int>("Orleans:LogLevel")).AddConsole())
          .Build();

      Task.Run(StartSilo);

      AssemblyLoadContext.Default.Unloading += context =>
      {
        Task.Run(StopSilo);
        siloStopped.WaitOne();
      };

      siloStopped.WaitOne();
    }

    private static async Task StartSilo()
    {
      await silo.StartAsync();
      Console.WriteLine("### Silo started!");
    }

    private static async Task StopSilo()
    {
      await silo.StopAsync();
      Console.WriteLine("### Silo started!");
      siloStopped.Set();
    }
  }
}
