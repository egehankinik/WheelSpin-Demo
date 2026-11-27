namespace WheelOfFortune.Gameplay
{
    public interface IWallet
    {
        int GetBalance(RewardModel currency);
        void Add(RewardModel currency, int amount);
        bool TrySpend(RewardModel currency, int amount);
    }
}
