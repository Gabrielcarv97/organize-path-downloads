using System;
using System.IO; // Adicionar using para Directory e Path
using System.Linq; // Adicionar using para LINQ (se necessário para processamento adicional)
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // Adicionar using para ILogger

namespace OrganizeDownload
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "OrganizeDownloadService"; // Set a custom service name  
                }) // Configures as a Windows service  
                .ConfigureServices(services => { services.AddHostedService<WorkerService>(); })
                .Build();

            await host.RunAsync();
        }
    }

    public class WorkerService : BackgroundService
    {
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        private const string ApplicationFolderInitial = "ARQUIVOS_";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads");

                if (Directory.Exists(downloadsPath))
                {
                    _logger.LogInformation("Procurando por arquivos na pasta: {DownloadsPath}", downloadsPath);

                    var exeFiles = Directory.GetFiles(downloadsPath, "*", SearchOption.AllDirectories);
                    var extensionsExistents = new Dictionary<string, string[]>();

                    foreach (var filesName in exeFiles)
                    {
                        var fileName = Path.GetFileName(filesName);
                        var extension = Path.GetExtension(fileName);

                        if (extensionsExistents.TryGetValue(extension, out var value))
                        {
                            extensionsExistents[extension] = value.Append(fileName).ToArray();
                        }
                        else
                        {
                            extensionsExistents.Add(extension, [fileName]);
                        }
                    }

                    foreach (var extension in extensionsExistents)
                    {
                        var folderName = extension.Key.Replace(".", string.Empty);
                        var folderPath = Path.Combine(downloadsPath, ApplicationFolderInitial + folderName.ToUpper());

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                            _logger.LogInformation("Criando a pasta: {FolderPath}", folderPath);
                        }

                        foreach (var file in extension.Value)
                        {
                            var sourceFile = Path.Combine(downloadsPath, file);
                            var destinationFile = Path.Combine(folderPath, file);

                            if (!File.Exists(destinationFile))
                            {
                                File.Move(sourceFile, destinationFile);
                                _logger.LogInformation("Movendo o arquivo {FileName} para {Destination}", file,
                                    folderPath);
                            }
                            else
                            {
                                _logger.LogWarning("O arquivo {FileName} já existe em {Destination}", file, folderPath);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("A pasta Downloads não foi encontrada em: {DownloadsPath}", downloadsPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao processar a pasta Downloads.");
            }
        }
    }
}