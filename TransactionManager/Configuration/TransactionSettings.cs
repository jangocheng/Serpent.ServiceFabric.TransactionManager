namespace TransactionManager.Configuration
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;

    public struct TransactionSettings
    {
        private readonly ConfigurationSection configSection;

        public TransactionSettings(ConfigurationPackage configurationPackage)
            : this(configurationPackage.Settings.Sections[TransactionConfigurationSectionConstants.Transaction])
        {
        }

        public TransactionSettings(ConfigurationSection configSection)
        {
            this.configSection = configSection;

            this.DefaultTransactionTimeout = this.GetTimeSpanSettingOrDefault(TransactionConfigurationSettingConstants.DefaultTransactionTimeout, TimeSpan.FromSeconds(30));
            this.TransactionRollbackTimeout = this.GetTimeSpanSettingOrDefault(TransactionConfigurationSettingConstants.TransactionRollbackTimeout, TimeSpan.FromSeconds(30));
            this.TransactionRollbackNotificationInterval = this.GetTimeSpanSettingOrDefault(
                TransactionConfigurationSettingConstants.TransactionRollbackNotificationInterval,
                TimeSpan.FromSeconds(2));
            this.MaximumTransactionTimeToLive = this.GetTimeSpanSettingOrDefault(TransactionConfigurationSettingConstants.MaximumTransactionTimeToLive, TimeSpan.FromHours(1));

            this.TransactionCommitTimeout = this.GetTimeSpanSettingOrDefault(TransactionConfigurationSettingConstants.TransactionCommitTimeout, TimeSpan.FromSeconds(30));
            this.TransactionCommitNotificationInterval = this.GetTimeSpanSettingOrDefault(
                TransactionConfigurationSettingConstants.TransactionCommitNotificationInterval,
                TimeSpan.FromSeconds(2));
        }

        public TimeSpan DefaultTransactionTimeout { get; }

        public TimeSpan MaximumTransactionTimeToLive { get; }

        public TimeSpan TransactionRollbackNotificationInterval { get; }

        public TimeSpan TransactionRollbackTimeout { get; }

        public TimeSpan TransactionCommitNotificationInterval { get; }

        public TimeSpan TransactionCommitTimeout { get; }

        private TimeSpan GetTimeSpanSettingOrDefault(string settingName, TimeSpan defaultValue)
        {
            var defaultTransactionTimeoutText = this.configSection.Parameters[settingName].Value;

            if (!TimeSpan.TryParse(defaultTransactionTimeoutText, out var transactionTimeout))
            {
                transactionTimeout = defaultValue;
            }

            return transactionTimeout;
        }
    }
}