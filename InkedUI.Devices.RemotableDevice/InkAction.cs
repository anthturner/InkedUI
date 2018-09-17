using InkedUI.Shared;
using Newtonsoft.Json;

namespace InkedUI.Devices.RemotableDevice
{
    public class InkAction
    {
        public enum InkActionTypes : int
        {
            Unknown = 0,
            Draw = 1,
            Clear = 2,
            Reset = 3,
            Init = 4,
            Ready = 5
        }
        public InkActionTypes InkActionType { get; set; }
        
        public string CanvasJson { get; set; }
        
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static InkAction Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<InkAction>(json);
        }

        public InkAction(InkActionTypes type)
        {
            InkActionType = type;
        }

        public InkAction() { }

        public InkAction(EInkCanvas canvas)
        {
            InkActionType = InkActionTypes.Draw;
            CanvasJson = canvas.ExportJson();
        }
    }
}
