// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.ComponentModel;
using AlphaCoreExtractor.Core;
using AlphaCoreExtractor.DBC.Structures;

namespace AlphaCoreExtractor.Helpers
{
    public class AsyncMapLoader : BackgroundWorker
    {
        public delegate void MapLoaded(object sender, AsyncMapLoaderEventArgs e);
        public event MapLoaded OnMapLoaded;

        public AsyncMapLoader(DBCMap dbcMap, string fileName) : base()
        {
            this.WorkerReportsProgress = true;

            this.DoWork += new DoWorkEventHandler((o, e) =>
            {
                CMapObj Map = new CMapObj(dbcMap, fileName, this);
                Map.LoadData();
                e.Result = Map;
            });

            this.ProgressChanged += new ProgressChangedEventHandler((o, e) =>
            {
                Program.UpdateLoadingStatus();
            });

            this.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, e) =>
            {
                if (e.Result is CMapObj _map)
                    OnMapLoaded?.Invoke(this, new AsyncMapLoaderEventArgs(_map));

                OnMapLoaded = null;
            });
        }
    }
}
