﻿using ECommon.IoC;
using ENode.Domain;
using ENode.Eventing;
using Forum.Events.Account;

namespace Forum.Domain.Accounts
{
    [Component]
    public class AccountService : IAccountService, IEventSynchronizer<AccountCreated>
    {
        private readonly IRepository _repository;
        private readonly IAccountRegistrationInfoRepository _accountRegistrationInfoRepository;

        public AccountService(IRepository repository, IAccountRegistrationInfoRepository accountRegistrationInfoRepository)
        {
            _repository = repository;
            _accountRegistrationInfoRepository = accountRegistrationInfoRepository;
        }

        public Account GetAccount(string accountName)
        {
            var accountRegistrationInfo = _accountRegistrationInfoRepository.GetByAccountName(accountName);
            if (accountRegistrationInfo != null &&
                accountRegistrationInfo.RegistrationStatus == AccountRegistrationStatus.Confirmed)
            {
                return _repository.Get<Account>(accountRegistrationInfo.AccountId);
            }
            return null;
        }

        void IEventSynchronizer<AccountCreated>.OnBeforePersisting(AccountCreated evnt)
        {
            _accountRegistrationInfoRepository.Add(new AccountRegistrationInfo(evnt.AggregateRootId, evnt.Name));
        }
        void IEventSynchronizer<AccountCreated>.OnAfterPersisted(AccountCreated evnt)
        {
            var registrationInfo = _accountRegistrationInfoRepository.GetByAccountName(evnt.Name);
            registrationInfo.ConfirmStatus();
            _accountRegistrationInfoRepository.Update(registrationInfo);
        }
    }
}
