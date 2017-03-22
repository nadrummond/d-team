using System;
using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.DTeam.Core.Storage
{
    public class Migrations
    {
        public const string MigrationProductName = "DTeam";

        private readonly ApplicationContext _applicationContext;

        public Migrations(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public void Run()
        {
            var installedVersion = new SemVersion(0);
            var logger = _applicationContext.ProfilingLogger.Logger;

            // get latest executed migration
            var service = _applicationContext.Services.MigrationEntryService;
            var migrations = service.GetAll(MigrationProductName);
            var latest = migrations.OrderByDescending(x => x.Version).FirstOrDefault();

            if (latest != null)
                installedVersion = latest.Version;

            // compare to this version
            logger.Debug<Migrations>("Versions: this=" + Constants.Version + ", installed=" + installedVersion);
            if (Constants.Version == installedVersion)
                return;

            var migrationsRunner = new MigrationRunner(service, logger, installedVersion, Constants.Version, MigrationProductName);

            try
            {
                var database = _applicationContext.DatabaseContext.Database;
                migrationsRunner.Execute(database);
            }
            catch (Exception e)
            {
                logger.Error<Migrations>("Failed to run migrations.", e);
            }
        }
    }
}
