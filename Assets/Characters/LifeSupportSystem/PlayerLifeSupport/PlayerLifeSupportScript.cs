
using System;
using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using UnityEngine;
using UnityEngine.Assertions;
using Assert = NUnit.Framework.Assert;

namespace Characters.LifeSupportSystem.PlayerLifeSupport {
    public class PlayerLifeSupportScript : LifeSupportManagerScript<PlayerLifeSupportScript.EVitals> {
        
        [Header("References")]
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private PlayerStateMachineScript _stateMachine;
        [SerializeField] private UIManagerScript _uiManager;
        
        [Header("Life support settings")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _maxStamina = 100f;
        
        private PlayerLifeSupportContextScript _context;
        
        public enum EVitals
        {
            Weight,
            Health,
            Stamina,
            Hunger
        }
        
        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            _context = new PlayerLifeSupportContextScript(_stateMachine, _rb, _maxHealth, _maxStamina, _uiManager);
            
            ValidateReferences();
            InitializeVitals();
        }

        private void InitializeVitals() {
            // order *should* matter, modifiers will probably depend on each vital
            Vitals.Add(EVitals.Weight, new WeightVitalScript(_context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(_context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(_context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(_context, EVitals.Health));
        }

        private void ValidateReferences() {
            Assert.IsNotNull(_rb, "Rigidbody is not assigned.");
            Assert.IsNotNull(_stateMachine, "StateMachine is not assigned.");
        }
    }
}