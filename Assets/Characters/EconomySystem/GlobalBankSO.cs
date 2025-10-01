using System;
using UnityEngine;

namespace Characters.EconomySystem {
    [CreateAssetMenu(fileName = "GlobalBank", menuName = "Economy/Global Bank")]
    public class GlobalBankSO : ScriptableObject
    {
        [SerializeField] private int balance = 0;

        public event Action<int> OnBalanceChanged;

        public int Balance => balance;

        public void Add(int amount) {
            balance += amount;
            OnBalanceChanged?.Invoke(balance);
        }

        public bool Spend(int amount) {
            if (balance < amount) return false;
            balance -= amount;
            OnBalanceChanged?.Invoke(balance);
            return true;
        }
    }
}
