// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System.ComponentModel;
using AlphaCoreExtractor.Core;

namespace AlphaCoreExtractor.Helpers
{
    public class AsyncMapLoader : BackgroundWorker
    {
        public delegate void MapLoaded(object sender, AsyncMapLoaderEventArgs e);
        public event MapLoaded OnMapLoaded;

        public AsyncMapLoader(string fileName) : base()
        {
            this.WorkerReportsProgress = true;

            this.DoWork += new DoWorkEventHandler((o, e) =>
            {
                CMapObj Map = new CMapObj(fileName, this);
                Map.LoadData();
                e.Result = Map;
            });

            this.ProgressChanged += new ProgressChangedEventHandler((o, e) =>
            {
                Program.UpdateLoadingStatus();
            });

            this.RunWorkerCompleted += new RunWorkerCompletedEventHandler((o, e) =>
            {
                if (e.Result is CMapObj map)
                    OnMapLoaded?.Invoke(this, new AsyncMapLoaderEventArgs(map));

                OnMapLoaded = null;
            });
        }
    }
}
