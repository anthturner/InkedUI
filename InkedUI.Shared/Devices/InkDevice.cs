using System.Threading.Tasks;

namespace InkedUI.Shared.Devices
{
    public abstract class InkDevice
    {
        public abstract Task Draw(EInkCanvas canvas);
        public abstract Task Clear();
        public abstract Task Reset();
        public abstract Task Init();
    }
}
