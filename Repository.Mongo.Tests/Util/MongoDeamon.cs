using System;
using System.Diagnostics;
using System.IO;

namespace Repository.Mongo.Tests.Util
{
    public class MongoDaemon : IDisposable
    {
        public const string ConnectionString = "mongodb://localhost:27017/test";
        public const string Host = "localhost";
        public const string Port = "27017";
        private readonly Process process;

        public MongoDaemon()
        {
            var assemblyFolder = Path.GetDirectoryName(new Uri(typeof(MongoDaemon).Assembly.CodeBase).LocalPath);
            var mongoFolder = Path.Combine(assemblyFolder, "Database");
            var dbFolder = Path.Combine(mongoFolder, "temp");

            if (Directory.Exists(dbFolder))
                Directory.Delete(dbFolder, true);
            Directory.CreateDirectory(dbFolder);

            process = new Process();
            process.StartInfo.FileName = Path.Combine(mongoFolder, "mongod.exe");
            process.StartInfo.Arguments = "--dbpath " + dbFolder + " --storageEngine ephemeralForTest";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }
    }
}
