using System.Globalization;
using System.IO;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class LocalDocumentDescriptor
    {
        private string _filename;
        private int _pageNumber;
        private static string LocalDocumentFilenameAttr = "Filename";
        private static string LocalDocumentPageNumberAttr = "PageNumber";

        public LocalDocumentDescriptor(string filename, int pageNumber)
        {
            this._filename = filename;
            this._pageNumber = pageNumber;
        }

        public int GetPageNumber()
        {
            return this._pageNumber;
        }

        public string GetDefaultDisplayName()
        {
            return Path.GetFileNameWithoutExtension(this._filename);
        }

        public string GetFilesystemAbsolutePath()
        {
            return Path.GetFullPath(this._filename);
        }

        public LocalDocumentDescriptor GetLocalDocumentDescriptor()
        {
            return this;
        }

        public void ValidateFilename()
        {
            if (!File.Exists(this._filename))
            {
                throw new InvalidFileContentsException(string.Format("SourceMap file reference {0} invalid",
                    this._filename));
            }
        }

        public static string GetXMLTag()
        {
            return "LocalDocumentDescriptor";
        }

        public void WriteXML(MashupWriteContext wc, string pathBase)
        {
            string value = this._filename;
            string directoryName = Path.GetDirectoryName(this._filename);
            if (pathBase != null && pathBase.ToLower().Equals(directoryName.ToLower()))
            {
                value = Path.GetFileName(this._filename);
            }

            wc.writer.WriteStartElement(GetXMLTag());
            wc.writer.WriteAttributeString(LocalDocumentFilenameAttr, value);
            wc.writer.WriteAttributeString(LocalDocumentPageNumberAttr,
                this._pageNumber.ToString(CultureInfo.InvariantCulture));
            wc.writer.WriteEndElement();
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            hash.Accumulate(this._filename);
            hash.Accumulate(this._pageNumber);
        }

        public LocalDocumentDescriptor(MashupParseContext context, string pathBase)
        {
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            string requiredAttribute = context.GetRequiredAttribute(LocalDocumentFilenameAttr);
            this._filename = Path.Combine(pathBase, requiredAttribute);
            this._pageNumber = context.GetRequiredAttributeInt(LocalDocumentPageNumberAttr);
            xMLTagReader.SkipAllSubTags();
        }
    }
}
