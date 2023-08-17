using System;
using System.IO;
using IxMilia.ThreeMf;
using IxMilia.Stl;
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
            StlTo3mf();
            await LoadPackage();
            await DoRepair();
            await SavePackage();
            StlFrom3mf();
        }

        public void StlTo3mf() {
            Console.WriteLine( "Convert STL to 3MF: START" );

            var stl = StlFile.Load( this.stlFile );
            var mesh = new ThreeMfMesh();

            stl.Triangles.ForEach( t => {
                mesh.Triangles.Add( new ThreeMfTriangle(
                    new ThreeMfVertex( t.Vertex1.X, t.Vertex1.Y, t.Vertex1.Z ),
                    new ThreeMfVertex( t.Vertex2.X, t.Vertex2.Y, t.Vertex2.Z ),
                    new ThreeMfVertex( t.Vertex3.X, t.Vertex3.Y, t.Vertex3.Z )
                ) );
            } );

            var baseMaterials = new ThreeMfBaseMaterials();
            baseMaterials.Bases.Add( new ThreeMfBase( "white", new ThreeMfsRGBColor( 255, 255, 255 ) ) );

            var obj = new ThreeMfObject {
                Name = "Test",
                Mesh = mesh,
                PropertyResource = baseMaterials,
                PropertyIndex = 0
            };

            var item = new ThreeMfModelItem( obj );

            var model = new ThreeMfModel();
            model.Metadata.Add( "Title", "STL" );
            model.Metadata.Add( "Description", "STL Conversion" );
            model.Items.Add( item );

            var threeMf = new ThreeMfFile();
            threeMf.Models.Add( model );

            using( var stream = File.Create( this.convertedFile ) ) {
                threeMf.Save( stream );
            }

            Console.WriteLine( "Convert STL to 3MF: DONE" );
        }

        public void StlFrom3mf() {
            Console.WriteLine( "Convert 3MF to STL: START" );

            StlFile stlFile = new StlFile();

            ThreeMfFile threeMfFile = ThreeMfFile.Load( this.repairedFile );
            foreach( ThreeMfModel threeMfModel in threeMfFile.Models ) {
                foreach( ThreeMfResource threeMfResource in threeMfModel.Resources ) {
                    if( threeMfResource.GetType() == typeof( ThreeMfObject ) ) {
                        ThreeMfMesh threeMfMesh = ( (ThreeMfObject)threeMfResource ).Mesh;
                        foreach( ThreeMfTriangle threeMfTriangle in threeMfMesh.Triangles ) {
                            stlFile.Triangles.Add( new StlTriangle(
                                new StlNormal( 0, 0, 0 ),
                                new StlVertex( (float)threeMfTriangle.V1.X, (float)threeMfTriangle.V1.Y, (float)threeMfTriangle.V1.Z ),
                                new StlVertex( (float)threeMfTriangle.V2.X, (float)threeMfTriangle.V2.Y, (float)threeMfTriangle.V2.Z ),
                                new StlVertex( (float)threeMfTriangle.V3.X, (float)threeMfTriangle.V3.Y, (float)threeMfTriangle.V3.Z )
                            ) );
                        }
                    }
                }
            }

            stlFile.Save( repairedStlFile, false );

            Console.WriteLine( "Convert STL to 3MF: DONE" );
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