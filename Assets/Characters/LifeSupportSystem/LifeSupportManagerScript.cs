using System;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.LifeSupportSystem
{
    public abstract class LifeSupportManagerScript<EVitals> : MonoBehaviour where EVitals : Enum
    {

        public Dictionary<EVitals, BaseVitalScript<EVitals>> Vitals = new Dictionary<EVitals, BaseVitalScript<EVitals>>();

        // Flag para saber si este objeto tiene Context inicializado
        protected bool IsInitialized => Vitals != null && Vitals.Count > 0;

        public virtual void Start()
        {
            foreach (var vital in Vitals)
            {
                BaseVitalScript<EVitals> currentVital = vital.Value;
                currentVital.SetupVital();
            }
        }

        public virtual void Update()
        {
            // Solo procesamos si hay Vitals inicializados y el Context está listo
            if (!IsInitialized) return;

            foreach (var vital in Vitals)
            {
                if (vital.Value == null) continue;
                vital.Value.UpdateModifiers();
                vital.Value.UpdateVital();
            }
        }
    }
}
