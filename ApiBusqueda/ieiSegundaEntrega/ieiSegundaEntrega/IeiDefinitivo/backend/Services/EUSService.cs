using Iei.Modelos_Fuentes;
using Iei.ModelosFuentesOriginales;
using Iei.Wrappers;

namespace Iei.Services
{
    public class EUSService
    {
    

        public List<ModeloJSONOriginal> ObtenerDatosJsonEuskadi()
        {
            return new EUSWrapper().GenerateProcessedJson();
        }

     
    }
}
