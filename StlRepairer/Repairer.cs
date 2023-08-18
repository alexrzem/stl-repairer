using Windows.Graphics.Printing3D;
using Windows.Storage;
using Windows.Storage.Streams;

namespace StlRepairer {
    public class Repairer {
        private string stlFile;
        private string convertedFile;
        private string repairedFile;
        private string repairedStlFile;

        private Printing3D3MFPackage? package;
        private Printing3DModel? model;

        public Repairer( string stlFile, string convertedFile, string repairedFile, string repairedStlFile ) {
            this.stlFile = stlFile;
            this.convertedFile = convertedFile;
            this.repairedFile = repairedFile;
            this.repairedStlFile = repairedStlFile;

            Console.WriteLine( "Repairing STL: " + this.stlFile );
        }

        public async Task Repair() {
            Converter.StlTo3mf( this.stlFile, this.convertedFile );
            await LoadPackage();
            await DoRepair();
            await SavePackage();
            Converter.StlFrom3mf( this.repairedFile, this.repairedStlFile );
        }

        public async Task LoadPackage() {
            Console.WriteLine( "3D Package for Repair: Loading ..." );

            StorageFile? storageFile = await StorageFile.GetFileFromPathAsync( this.convertedFile );
            using IRandomAccessStream stream = await storageFile.OpenAsync( FileAccessMode.ReadWrite );

            this.package = await Printing3D3MFPackage.LoadAsync( stream );
            this.model = await package.LoadModelFromPackageAsync( stream );
            this.model.Unit = Printing3DModelUnit.Millimeter;

            Console.WriteLine( "3D Package for Repair: Loaded." );
        }

        private async Task DoRepair() {
            Console.WriteLine( "3D Package for Repair: Repairing ..." );

            await this.model.RepairAsync();
            await this.package.SaveModelToPackageAsync( this.model );

            Console.WriteLine( "3D Package for Repair: Repaired." );
        }

        private async Task SavePackage() {
            Console.WriteLine( "3D Package for Repair: Saving ..." );

            using var oStream = await this.package.SaveAsync();
            oStream.Seek( 0 );

            using( System.IO.File.Create( this.repairedFile ) ) {; }
            StorageFile storageFile = await StorageFile.GetFileFromPathAsync( this.repairedFile );

            // read from the file stream and write to a buffer
            using( var dataReader = new DataReader( oStream ) ) {
                await dataReader.LoadAsync( (uint)oStream.Size );
                var buffer = dataReader.ReadBuffer( (uint)oStream.Size );

                // write from the buffer to the storagefile specified
                await FileIO.WriteBufferAsync( storageFile, buffer );
            }

            Console.WriteLine( "3D Package for Repair: Saved." );
        }
    }

}