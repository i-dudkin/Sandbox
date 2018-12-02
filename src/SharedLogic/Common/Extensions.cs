using System;
using System.IO;
using System.Reactive.Disposables;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Sandbox.Common
{
    public static class Extensions
    {
        public static IDisposable Lock( this ReaderWriterLockSlim locker )
        {
            locker.EnterWriteLock();
            return Disposable.Create( locker.ExitWriteLock );
        }

        public static void WriteBool( this MemoryStream stream, bool b )
        {
            stream.WriteByte( Convert.ToByte( b ) );
        }

        public static void WriteString( this MemoryStream stream, string str )
        {
            var bytes = Encoding.UTF8.GetBytes( str );
            stream.WriteInt( bytes.Length );
            stream.Write( bytes, 0, bytes.Length );
        }

        public static void WriteInt( this MemoryStream stream, int i )
        {
            stream.Write( BitConverter.GetBytes( i ), 0, sizeof( int ) );
        }

        public static void WriteObject( this MemoryStream stream, BinaryFormatter formatter, object obj )
        {
            if ( obj == null )
            {
                stream.WriteInt( 0 );
                return;
            }

            using ( var ms = new MemoryStream() )
            {
                formatter.Serialize( ms, obj );
                var bytes = ms.ToArray();
                stream.WriteInt( bytes.Length );
                stream.Write( bytes, 0, bytes.Length );
            }
        }

        public static int ReadInt( this MemoryStream stream )
        {
            var bytes = new byte[ sizeof( int ) ];
            stream.Read( bytes, 0, bytes.Length );
            return BitConverter.ToInt32( bytes, 0 );
        }

        public static bool ReadBool( this MemoryStream stream )
        {
            return Convert.ToBoolean( stream.ReadByte() );
        }

        public static string ReadString( this MemoryStream stream )
        {
            var length = stream.ReadInt();
            var bytes = new byte[ length ];
            stream.Read( bytes, 0, length );
            return Encoding.UTF8.GetString( bytes );
        }

        public static object ReadObject( this MemoryStream stream, BinaryFormatter formatter )
        {
            var length = stream.ReadInt();
            if ( length == 0 )
                return null;
            var bytes = new byte[ length ];
            stream.Read( bytes, 0, length );
            using ( var ms = new MemoryStream( bytes ) )
                return formatter.Deserialize( ms );
        }
    }
}