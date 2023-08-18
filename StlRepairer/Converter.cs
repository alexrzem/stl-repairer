using System;
using System.IO;
using IxMilia.ThreeMf;
using IxMilia.Stl;
using Windows.Graphics.Printing3D;
using Windows.Storage;
using Windows.Storage.Streams;

using System;
using System.Numerics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Schema2;

namespace StlRepairer {

    using VERTEX = SharpGLTF.Geometry.VertexTypes.VertexPosition;

    public class Converter {
        private string stlFile;
        private string convertedFile;
        private string type;

        private Printing3D3MFPackage? package;
        private Printing3DModel? model;

        public Converter( string stlFile, string convertedFile, string type ) {
            this.stlFile = stlFile;
            this.convertedFile = convertedFile;
            this.type = type;

            Console.WriteLine( $"Converting STL: {this.stlFile} to {this.type}" );
        }

        public async Task Convert() {

        }

        public static void StlTo3mf( string inFile, string outFile ) {
            Console.WriteLine( "Convert STL to 3MF: START" );

            var stl = StlFile.Load( inFile );
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

            using( var stream = File.Create( outFile ) ) {
                threeMf.Save( stream );
            }

            Console.WriteLine( "Convert STL to 3MF: DONE" );
        }

        public static void StlToGtlf( string inFile, string outFile ) {
            Console.WriteLine( "Convert STL to GTLF: START" );

            var stl = StlFile.Load( inFile );
            var mesh = new MeshBuilder<VERTEX>( "mesh" );

            var material = new MaterialBuilder()
                .WithDoubleSide( false )
                //.WithBaseColor( new Vector4( 1, 1, 1, 1 ) )
                .WithChannelParam( KnownChannel.BaseColor, KnownProperty.RGBA, new Vector4( 1, 1, 1, 1 ) );

            var prim = mesh.UsePrimitive( material );

            stl.Triangles.ForEach( t => {
                prim.AddTriangle(
                    new VERTEX( t.Vertex1.X, t.Vertex1.Y, t.Vertex1.Z ), //
                    new VERTEX( t.Vertex2.X, t.Vertex2.Y, t.Vertex2.Z ), //
                    new VERTEX( t.Vertex3.X, t.Vertex3.Y, t.Vertex3.Z ) //
                );
            } );

            var scene = new SharpGLTF.Scenes.SceneBuilder();
            scene.AddRigidMesh( mesh, Matrix4x4.Identity );

            var model = scene.ToGltf2();
            //model.SaveGLB( "mesh.glb" );
            model.SaveGLTF( outFile );

            Console.WriteLine( "Convert STL to GTLF: DONE" );
        }

        public static void StlFrom3mf( string inFile, string outFile ) {
            Console.WriteLine( "Convert 3MF to STL: START" );

            StlFile stlFile = new StlFile();

            ThreeMfFile threeMfFile = ThreeMfFile.Load( inFile );
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

            stlFile.Save( outFile, false );

            Console.WriteLine( "Convert STL to 3MF: DONE" );
        }

        public static void StlFromGtlf( string inFile, string outFile ) {
            Console.WriteLine( "Convert 3MF to STL: START" );

            StlFile stlFile = new StlFile();

            ThreeMfFile threeMfFile = ThreeMfFile.Load( inFile );
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

            stlFile.Save( outFile, false );

            Console.WriteLine( "Convert STL to 3MF: DONE" );
        }
    }
}