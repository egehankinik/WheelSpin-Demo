using System;
using System.Collections.Generic;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Configuration
{
    public sealed class GameConfiguration
    {
        #region Fields

        private readonly Dictionary<RewardDefinition, RewardModel> _rewards = new Dictionary<RewardDefinition, RewardModel>();
        private readonly Dictionary<RewardModel, RewardDefinition> _rewardDefinitions = new Dictionary<RewardModel, RewardDefinition>();
        private readonly Dictionary<WheelDefinition, WheelModel> _wheels = new Dictionary<WheelDefinition, WheelModel>();
        private readonly Dictionary<WheelModel, WheelDefinition> _wheelDefinitions = new Dictionary<WheelModel, WheelDefinition>();
        private readonly Dictionary<ZoneTypeDefinition, ZoneTypeModel> _zoneTypes = new Dictionary<ZoneTypeDefinition, ZoneTypeModel>();
        private readonly Dictionary<ZoneTypeModel, ZoneTypeDefinition> _zoneTypeDefinitions = new Dictionary<ZoneTypeModel, ZoneTypeDefinition>();
        private readonly HashSet<string> _rewardIds = new HashSet<string>();

        #endregion

        public GameConfiguration(ZoneSettings zoneSettings, EconomySettings economySettings)
        {
            if (!zoneSettings)
            {
                throw new ArgumentNullException(nameof(zoneSettings));
            }

            if (!economySettings)
            {
                throw new ArgumentNullException(nameof(economySettings));
            }

            foreach (var zoneType in zoneSettings.ZoneTypes)
            {
                if (zoneType)
                {
                    CreateZoneType(zoneType);
                }
            }

            ZoneRules = new ZoneRules(_zoneTypes.Values, zoneSettings.RewardGrowthPerZone);
            EconomyRules = new EconomyRules(GetReward(economySettings.ReviveCurrency), economySettings.ReviveCost);
            WalletKeyPrefix = economySettings.WalletKeyPrefix;
        }

        #region Properties

        public ZoneRules ZoneRules { get; }
        public EconomyRules EconomyRules { get; }
        public string WalletKeyPrefix { get; }

        public IEnumerable<WheelDefinition> WheelDefinitions
        {
            get { return _wheels.Keys; }
        }

        #endregion

        #region Public Methods

        public RewardDefinition GetDefinition(RewardModel reward)
        {
            return _rewardDefinitions[reward];
        }

        public WheelDefinition GetDefinition(WheelModel wheel)
        {
            return _wheelDefinitions[wheel];
        }

        public ZoneTypeDefinition GetDefinition(ZoneTypeModel zoneType)
        {
            return _zoneTypeDefinitions[zoneType];
        }

        public ZoneTypeModel GetZoneType(ZoneTypeDefinition definition)
        {
            return _zoneTypes[definition];
        }

        #endregion

        #region Private Methods

        private void CreateZoneType(ZoneTypeDefinition definition)
        {
            if (_zoneTypes.ContainsKey(definition))
            {
                return;
            }

            var tiers = new List<WheelTierModel>();
            foreach (var tier in definition.WheelTiers)
            {
                if (!tier.Wheel)
                {
                    throw new InvalidOperationException($"{definition.name}: wheel tier from zone {tier.FromZone} has no wheel.");
                }

                tiers.Add(new WheelTierModel(tier.FromZone, GetWheel(tier.Wheel)));
            }

            var model = new ZoneTypeModel(definition.Interval, definition.IsSafe, tiers);
            _zoneTypes.Add(definition, model);
            _zoneTypeDefinitions.Add(model, definition);
        }

        private WheelModel GetWheel(WheelDefinition definition)
        {
            if (_wheels.TryGetValue(definition, out var cached))
            {
                return cached;
            }

            var slices = new List<SliceModel>();
            for (var i = 0; i < definition.SliceCount; i++)
            {
                var slice = definition.Slices[i];
                if (slice.IsBomb)
                {
                    slices.Add(SliceModel.Bomb());
                    continue;
                }

                if (!slice.Reward)
                {
                    throw new InvalidOperationException($"{definition.name}: slice {i} has no reward assigned.");
                }

                slices.Add(new SliceModel(GetReward(slice.Reward), slice.BaseAmount));
            }

            var model = new WheelModel(slices);
            _wheels.Add(definition, model);
            _wheelDefinitions.Add(model, definition);
            return model;
        }

        private RewardModel GetReward(RewardDefinition definition)
        {
            if (!definition)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (_rewards.TryGetValue(definition, out var cached))
            {
                return cached;
            }

            if (!_rewardIds.Add(definition.Id))
            {
                throw new InvalidOperationException($"{definition.name}: reward id '{definition.Id}' is used by another reward.");
            }

            var model = new RewardModel(definition.Id, definition.IsCurrency, definition.StartingBalance);
            _rewards.Add(definition, model);
            _rewardDefinitions.Add(model, definition);
            return model;
        }

        #endregion
    }
}
