﻿using System;
using System.IO;
using MimeTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ziks.WebServer;

namespace SourceUtils.WebExport
{
    [JsonConverter( typeof(UrlConverter) )]
    public struct Url : IEquatable<Url>
    {
        public static implicit operator Url( string value )
        {
            return new Url( value );
        }

        public static implicit operator string( Url url )
        {
            return url.Value;
        }

        public readonly string Value;

        public Url( string value )
        {
            Value = value;
        }

        public bool Equals( Url other )
        {
            return string.Equals( Value, other.Value );
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) ) return false;
            return obj is Url && Equals( (Url) obj );
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class UrlConverter : JsonConverter
    {
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            var url = (Url) value;
            writer.WriteValue( url.Value );
            if ( Program.IsExporting ) Program.AddExportUrl( url );
        }

        public override object ReadJson( JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer )
        {
            return new Url( reader.ReadAsString() );
        }

        public override bool CanConvert( Type objectType )
        {
            return objectType == typeof(Url);
        }
    }

    class ResourceController : Controller
    {
        [ResponseWriter]
        public void OnWriteObject( object obj )
        {
            OnServiceJson( JObject.FromObject( obj ) );
        }

        protected override void OnServiceJson( JToken token )
        {
            Response.ContentType = MimeTypeMap.GetMimeType( ".json" );

            using ( var writer = new StreamWriter( Response.OutputStream ) )
            {
                writer.Write( token.ToString( Formatting.None ) );
            }

            Response.OutputStream.Close();
        }
    }
}
