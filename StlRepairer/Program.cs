using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace StlRepairer {
    class Program {
        static async Task Main( string[] args ) {
            Console.WriteLine( "STLRepairer" );

            if( args.Length < 1 ) {
                Console.WriteLine( "Incorrect number of arguments" );
                return;
            } else {
                string path = Path.GetFullPath( args[0] );
                Console.WriteLine( $"Repair: {path}" );

                FileAttributes attributes = File.GetAttributes( path );
                switch( attributes ) {
                    case FileAttributes.Directory:
                        if( Directory.Exists( path ) ) {
                            int index = 1;
                            List<string> files = Directory.EnumerateFiles( path, "*.stl", SearchOption.AllDirectories ).Cast<string>().ToList();
                            files.Sort();
                            foreach( string file in files ) {
                                Console.WriteLine( $"Process File {index} of {files.Count}: {file}" );
                                if( !attributes.HasFlag( FileAttributes.ReadOnly ) ) {
                                    await Repair( file );
                                } else {
                                    Console.WriteLine( $"Skipping READONLY File: {file}" );
                                }
                                index++;
                            }
                        } else {
                            Console.WriteLine( "This directory does not exist." );
                        }
                        break;
                    default:
                        if( File.Exists( path ) && Path.GetExtension( path ).ToLower().Equals( ".stl" ) && !attributes.HasFlag( FileAttributes.ReadOnly ) ) {
                            await Repair( path );
                        } else {
                            Console.WriteLine( "This file does not exist." );
                        }
                        break;
                }
            }
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
    }
}