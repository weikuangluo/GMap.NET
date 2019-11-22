using System.Globalization;
using System.Xml;
using MSR.CVE.BackMaker.ImagePipeline;

namespace MSR.CVE.BackMaker
{
    public class SourceMapRegistrationView : IMapView, ICurrentView
    {
        private SourceMap _sourceMap;
        private bool _locked;
        private LatLonZoom sourceMapView;
        private MapPosition referenceMapView;
        private static string lockedAttribute = "locked";
        private static string sourceMapViewTag = "SourceMapPosition";
        private static string referenceMapViewTag = "ReferenceMapPosition";

        public SourceMap sourceMap
        {
            get
            {
                return this._sourceMap;
            }
        }

        public bool locked
        {
            get
            {
                return this._locked;
            }
        }

        public object GetViewedObject()
        {
            return this.sourceMap;
        }

        public SourceMapRegistrationView(SourceMap sourceMap, MapPosition lockedMapView)
        {
            this._sourceMap = sourceMap;
            this._locked = true;
            this.referenceMapView = new MapPosition(lockedMapView, null);
            this.sourceMapView = lockedMapView.llz;
        }

        public SourceMapRegistrationView(SourceMap sourceMap, LatLonZoom sourceMapView, MapPosition referenceMapView)
        {
            this._sourceMap = sourceMap;
            this._locked = false;
            this.sourceMapView = sourceMapView;
            this.referenceMapView = new MapPosition(referenceMapView, null);
        }

        public static string GetXMLTag()
        {
            return "SourceMapView";
        }

        public SourceMapRegistrationView(SourceMap sourceMap, MashupParseContext context)
        {
            this._sourceMap = sourceMap;
            XMLTagReader xMLTagReader = context.NewTagReader(GetXMLTag());
            this._locked = context.GetRequiredAttributeBoolean(lockedAttribute);
            bool flag = false;
            while (xMLTagReader.FindNextStartTag())
            {
                if (xMLTagReader.TagIs(sourceMapViewTag))
                {
                    XMLTagReader xMLTagReader2 = context.NewTagReader(sourceMapViewTag);
                    while (xMLTagReader2.FindNextStartTag())
                    {
                        if (xMLTagReader2.TagIs(LatLonZoom.GetXMLTag()))
                        {
                            this.sourceMapView = new LatLonZoom(context, ContinuousCoordinateSystem.theInstance);
                            flag = true;
                        }
                    }
                }
                else
                {
                    if (xMLTagReader.TagIs(referenceMapViewTag))
                    {
                        XMLTagReader xMLTagReader3 = context.NewTagReader(referenceMapViewTag);
                        while (xMLTagReader3.FindNextStartTag())
                        {
                            if (xMLTagReader3.TagIs(MapPosition.GetXMLTag(context.version)))
                            {
                                this.referenceMapView =
                                    new MapPosition(context, null, MercatorCoordinateSystem.theInstance);
                            }
                        }
                    }
                }
            }

            if (this.referenceMapView == null)
            {
                throw new InvalidMashupFile(context, "No " + referenceMapViewTag + " tag in LayerView.");
            }

            if (flag == this._locked)
            {
                throw new InvalidMashupFile(context, "locked flag disagrees with " + sourceMapViewTag + " element.");
            }
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement(GetXMLTag());
            writer.WriteAttributeString(lockedAttribute, this.locked.ToString(CultureInfo.InvariantCulture));
            if (!this.locked)
            {
                writer.WriteStartElement(sourceMapViewTag);
                this.sourceMapView.WriteXML(writer);
                writer.WriteEndElement();
            }

            writer.WriteStartElement(referenceMapViewTag);
            this.referenceMapView.WriteXML(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public MapPosition GetReferenceMapView()
        {
            return this.referenceMapView;
        }

        public LatLonZoom GetSourceMapView()
        {
            if (this.locked)
            {
                return this.referenceMapView.llz;
            }

            return this.sourceMapView;
        }
    }
}
