
using Iei.Wrappers;

namespace Iei.Services
{
    public class CVService
    {
     

        public List<ModeloCSVOriginal> ObtenerDatosCsvCV()
        {
            return new CVWrapper().ParseMonumentosCsv();
        }
    }
}
