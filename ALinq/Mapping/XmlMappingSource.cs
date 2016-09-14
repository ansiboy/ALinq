using System;
using System.IO;
using System.Xml;
using LinqToSqlShared.Mapping;

namespace ALinq.Mapping
{
    /// <summary>
    /// Represents a mapping source that uses an external XML mapping file to create the model.
    /// </summary>
    public sealed class XmlMappingSource : MappingSource
    {
        // Fields
        private readonly DatabaseMapping map;

        // Methods
        private XmlMappingSource(DatabaseMapping map)
        {
            this.map = map;
        }

        protected override MetaModel CreateModel(Type dataContextType)
        {
            if (dataContextType == null)
            {
                throw Error.ArgumentNull("dataContextType");
            }
            return new MappedMetaModel(this, dataContextType, this.map);
        }

        /// <summary>
        /// Creates a mapping source from an XML reader.
        /// </summary>
        /// <param name="reader">An XML reader.</param>
        /// <returns>The new XML mapping source, as type ALinq.Mapping.XmlMappingSource.</returns>
        public static XmlMappingSource FromReader(XmlReader reader)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            reader.MoveToContent();
            DatabaseMapping map = XmlMappingReader.ReadDatabaseMapping(reader);
            if (map == null)
            {
                throw Error.DatabaseNodeNotFound("http://schemas.microsoft.com/linqtosql/mapping/2007");
            }
            return new XmlMappingSource(map);
        }

        /// <summary>
        /// Creates a mapping source from XML in a stream.
        /// </summary>
        /// <param name="stream">A stream of XML.</param>
        /// <returns>The new XML mapping source, as type ALinq.Mapping.XmlMappingSource.</returns>
        public static XmlMappingSource FromStream(Stream stream)
        {
            if (stream == null)
            {
                throw Error.ArgumentNull("stream");
            }
            XmlReader reader = new XmlTextReader(stream);
            return FromReader(reader);
        }

        /// <summary>
        /// Creates a mapping source from XML that is loaded from a URL.
        /// </summary>
        /// <param name="url">The URL pointing to the XML.</param>
        /// <returns>The new XML mapping source, as type ALinq.Mapping.XmlMappingSource.</returns>
        public static XmlMappingSource FromUrl(string url)
        {
            XmlMappingSource source;
            if (url == null)
            {
                throw Error.ArgumentNull("url");
            }
            XmlReader reader = new XmlTextReader(url);
            try
            {
                source = FromReader(reader);
            }
            finally
            {
                reader.Close();
            }
            return source;
        }

        /// <summary>
        /// Creates a mapping source from an XML string.
        /// </summary>
        /// <param name="xml">A string that contains XML.</param>
        /// <returns>The new XML mapping source, as type ALinq.Mapping.XmlMappingSource.</returns>
        public static XmlMappingSource FromXml(string xml)
        {
            if (xml == null)
            {
                throw Error.ArgumentNull("xml");
            }
            XmlReader reader = new XmlTextReader(new StringReader(xml));
            return FromReader(reader);
        }
    }



}
