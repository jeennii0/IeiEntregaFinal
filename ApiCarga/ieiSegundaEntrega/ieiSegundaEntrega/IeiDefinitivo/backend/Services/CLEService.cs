using Iei.ModelosFuentesOriginales;
using Iei.Wrappers;

namespace Iei.Services
{
    public class CLEService
    {
        public List<ModeloXMLOriginal> ObtenerDatosXmlCastillaLeon()
        {
            return new CLEWrapper().ConvertXmlToJson();
        }
        }
    }