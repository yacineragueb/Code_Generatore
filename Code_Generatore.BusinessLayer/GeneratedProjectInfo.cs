
namespace Code_Generatore.BusinessLayer
{
    public class GeneratedProjectInfo
    {
        public string PresentationLayerPath { get; set; }
        public string BusinessLogicLayerPath { get; set; }
        public string DataAccessLayerPath { get; set; }

        public GeneratedProjectInfo(string presentationLayerPath, string businessLogicLayerPath, string dataAccessLayerPath) 
        {
            PresentationLayerPath = presentationLayerPath;
            BusinessLogicLayerPath = businessLogicLayerPath;
            DataAccessLayerPath = dataAccessLayerPath;
        }
    }
}
