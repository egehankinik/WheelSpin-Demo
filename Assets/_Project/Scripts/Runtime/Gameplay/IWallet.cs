namespace WheelOfFortune.Gameplay
{
    public interface IWallet
    {
        int Balance { get; }
        void Add(int amount);
        bool TrySpend(int amount);
    }
}
