using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentArgs;


namespace StlRepairer {

    class Program {
        static async Task Main( string[] args ) {
            await FluentArgsBuilder.New()
                .DefaultConfigsWithAppDescription( "STLRepairer" )
                .Parameter( "-c", "--convert" )
                    .WithDescription( "Convert file to specified format" )
                    .WithExamples( "gTLF" )
                    .IsOptionalWithDefault( "" )
                .Flag( "-r", "--repair" )
                    .WithDescription( "Repair file" )
                .Flag( "-z", "--zero" )
                    .WithDescription( "Input png file" )
                .LoadRemainingArguments()
                    .WithDescription( "All files which should be processed" )
                .Call( files => zero => repair => convert => Run( files, zero, repair, convert ) )
                .ParseAsync( args );
        }

        static async Task Run( IReadOnlyList<string> files, bool zero, bool repair, string convert ) {
            Console.WriteLine( $"STLRepair: {files}" );

            List<string> fileList = GetFiles( files );

            int index = 1;
            int count = files.Count;
            foreach( string file in fileList ) {
                if( repair ) {
                    Console.WriteLine( $"Repair file {index} of {count}: {file}" );
                    await Repair( file );
                }

                if( zero ) {
                    Console.WriteLine( $"Zero file {index} of {count}: {file}" );
                    //Task task = Zero( file );
                }

                if( String.Equals( convert.ToLower(), "3mf" ) || String.Equals( convert.ToLower(), "gtlf" ) ) {
                    Console.WriteLine( $"Convert to {convert.ToUpper()} {index} of {count}: {file}" );
                    await Convert( file, convert.ToLower() );
                }

                index++;
            }
        }

        static List<string> GetFiles( IReadOnlyList<string> files ) {
            List<string> list = new List<string>();

            foreach( string path in files ) {

                FileAttributes attributes = File.GetAttributes( path );
                switch( attributes ) {
                    case FileAttributes.Directory:
                        if( Directory.Exists( path ) ) {
                            foreach( string file in Directory.EnumerateFiles( path, "*.stl", SearchOption.AllDirectories ).Cast<string>().ToList() ) {
                                if( !attributes.HasFlag( FileAttributes.ReadOnly ) ) {
                                    list.Add( file );
                                }
                            }
                        }
                        break;
                    default:
                        if( File.Exists( path ) && Path.GetExtension( path ).ToLower().Equals( ".stl" ) && !attributes.HasFlag( FileAttributes.ReadOnly ) ) {
                            list.Add( path );
                        }
                        break;
                }
            }

            list.Sort();
            return list;
        }

        static async Task Repair( string path ) {
            string stlFile = path;
            string convertedFile = Path.ChangeExtension( stlFile, "3mf" );
            string repairedFile = Path.ChangeExtension( stlFile, "repaired.3mf" );
            string repairedStlFile = Path.ChangeExtension( stlFile, "stl" );

            try {
                var repairer = new Repairer( stlFile, convertedFile, repairedFile, repairedStlFile );
                await repairer.Repair();

                File.Delete( convertedFile );
                File.Delete( repairedFile );
            } catch( Exception e ) {
                Console.WriteLine( e.Message );
                Console.WriteLine( e );
            }
        }

        static async Task Convert( string path, string type ) {
            string stlFile = path;
            string convertedFile = Path.ChangeExtension( stlFile, type );

            try {
                var converter = new Converter( stlFile, convertedFile, type );
                await converter.Convert();
            } catch( Exception e ) {
                Console.WriteLine( e.Message );
                Console.WriteLine( e );
            }
        }
    }
}