
using System;
using Characters.LifeSupportSystem.PlayerLifeSupport.ConcreteVitals;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
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
        
        public PlayerLifeSupportContextScript Context { get; private set; }
        
        public enum EVitals
        {
            Weight,
            Health,
            Stamina,
            Hunger
        }
        
        private void Awake() {
            _rb = GetComponent<Rigidbody>();
            Context = new PlayerLifeSupportContextScript(_stateMachine, _rb, _maxHealth, _maxStamina, _uiManager);
            
            ValidateReferences();
            InitializeVitals();
        }

        private void InitializeVitals() {
            // order *should* matter, modifiers will probably depend on each vital
            Vitals.Add(EVitals.Weight, new WeightVitalScript(Context, EVitals.Weight));
            Vitals.Add(EVitals.Stamina, new StaminaVitalScript(Context, EVitals.Stamina));
            Vitals.Add(EVitals.Hunger, new HungerVitalScript(Context, EVitals.Hunger));
            Vitals.Add(EVitals.Health, new HealthVitalScript(Context, EVitals.Health));
        }

        private void ValidateReferences() {
            Assert.IsNotNull(_rb, "Rigidbody is not assigned.");
            Assert.IsNotNull(_stateMachine, "StateMachine is not assigned.");
        }

        private BaseVitalScript<EVitals> WeightScript => Vitals[EVitals.Weight];
        private BaseVitalScript<EVitals> StaminaScript => Vitals[EVitals.Stamina];
        private BaseVitalScript<EVitals> HungerScript => Vitals[EVitals.Hunger];
        private BaseVitalScript<EVitals> HealthScript => Vitals[EVitals.Health];


        // public bool IsUnconscious => HealthScript.var;
        // public bool IsHeavy => WeightScript.var;
        // public bool IsTired => StaminaScript.var;
        // public bool IsStarved => HungerScript.var;
        
    }
}