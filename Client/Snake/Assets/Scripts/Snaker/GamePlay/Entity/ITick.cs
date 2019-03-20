
namespace GamePlay
{
    public interface ITick 
    {
        bool IsReleased{ get; }

        void Release();
        void EnterFrame(int frame);
    }
}
