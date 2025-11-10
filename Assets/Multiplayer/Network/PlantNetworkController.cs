using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Characters.ActiveRagdollSystem;
using Characters.Enemies.Scripts;
using Characters.Enemies.Scripts.Plant;
using Items.Scripts;
using Characters.LifeSupportSystem.EnemyLifeSupport;

namespace Characters.Enemies.Scripts.Plant
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(ActiveRagdollCoreScript))]
    [RequireComponent(typeof(EnemyLifeSupportScript))]
    [RequireComponent(typeof(NervousSystemScript))]
    [RequireComponent(typeof(AttackScript))]
    [RequireComponent(typeof(TargetingScript))]
    [RequireComponent(typeof(PlantAnimController))]
    public class PlantNetworkController : MonoBehaviourPun, IPunObservable
    {
        [Header("References")]
        [SerializeField] private ActiveRagdollCoreScript arCoreScript;
        [SerializeField] private EnemyLifeSupportScript lifeSupport;
        [SerializeField] private NervousSystemScript nervousSystem;
        [SerializeField] private AttackScript attackScript;
        [SerializeField] private TargetingScript targetingScript;
        [SerializeField] private PlantAnimController animController;

        [Header("Settings")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private bool debugLogs = false;

        // estado local (maestro lo mantiene)
        private float _attackTimer;
        private bool _isDead;
        private float _currentHealth;

        // helper
        private bool IsAuthoritative => PhotonNetwork.IsMasterClient;

        void Awake()
        {
            arCoreScript = GetComponent<ActiveRagdollCoreScript>();
            lifeSupport = GetComponent<EnemyLifeSupportScript>();
            nervousSystem = GetComponent<NervousSystemScript>();
            attackScript = GetComponent<AttackScript>();
            targetingScript = GetComponent<TargetingScript>();
            animController = GetComponent<PlantAnimController>();
        }

        void Start()
        {
            _attackTimer = attackCooldown;
            // tomar salud inicial desde el sistema de vida si existe
            try
            {
                _currentHealth = lifeSupport.Context.HealthVital.CurrentHealth;
            }
            catch
            {
                _currentHealth = 100f;
            }

            animController.TriggerByName(PlantAnimController.EPlantStates.Idle.ToString());

            // Solo el MasterClient debe controlar ataques y daños físicamente
            if (!IsAuthoritative)
            {
                // evitar que clientes remotos ejecuten lógica de daño/ataque local
                if (attackScript) attackScript.enabled = false;
                if (targetingScript) targetingScript.enabled = false;
            }
            else
            {
                // Si somos autoridad, asegurarnos que componentes estén activos
                if (attackScript) attackScript.enabled = true;
                if (targetingScript) targetingScript.enabled = true;
            }
        }

        void Update()
        {
            // Los clientes remotos no ejecutan IA; sólo reproducen estado vía RPCs.
            if (!IsAuthoritative) return;

            // Autoridad (MasterClient) ejecuta la IA
            if (_isDead) return;

            // Si el sistema nervioso detectó daño localmente, el MASTER procesa;
            // Si la detección ocurrió en otro cliente, éste debe enviar RPC al master (ver más abajo).
            if (nervousSystem.NervesTriggered)
            {
                var hurt = nervousSystem.HurtingScript;
                if (hurt != null)
                {
                    // Aplicar daño localmente (porque somos master) y notificar a todos
                    ApplyDamageAuthority(hurt.Damage, PhotonNetwork.LocalPlayer.ActorNumber);
                }
                nervousSystem.ResetNerves();
            }

            // IA: targeting y ataque (solo master)
            if (targetingScript.CurrentTargetTransform)
            {
                HandleTargeting();
            }

            if (_attackTimer > 0f) _attackTimer -= Time.deltaTime;
        }

        private void HandleTargeting()
        {
            var target = targetingScript.CurrentTargetTransform;
            if (target == null) return;

            // rotamos localmente para que se vea bien en host; las rotaciones no se sincronizan aquí
            Vector3 lookDir = target.position - transform.position;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            if (_attackTimer <= 0f)
            {
                // Realizar ataque (solo en master)
                PerformAttackAuthority();
                _attackTimer = attackCooldown;
            }
        }

        // ---------- ATAQUE (autoridad) ----------
        private void PerformAttackAuthority()
        {
            if (debugLogs) Debug.Log("[PlantNetwork] Master performing attack.");

            // Ejecuta la lógica de ataque local (daño via colliders, etc.)
            if (attackScript)
            {
                // Si tu AttackScript expone algún nombre/índice de la animación elegida,
                // sería ideal recuperarlo y enviarlo por RPC. En este ejemplo, disparo
                // la animación MediumAttack en los clientes remotos para representar el ataque.
                attackScript.PerformAttack(); // asumo que existe este método público
            }

            // Notificar a los demás clientes que reproduzcan la animación de ataque
            // uso el enum de PlantAnimController: MediumAttack = 1 (según tu enum).
            int attackStateIndex = (int)PlantAnimController.EPlantStates.MediumAttack;
            photonView.RPC(nameof(RPC_PlayAttackAnim), RpcTarget.Others, attackStateIndex);
            // también reproducir localmente la animación en el master
            animController.TriggerByName(PlantAnimController.EPlantStates.MediumAttack.ToString());
            animController.attack = true;
            // reset anim después de 1s (ajustá a lo que necesites)
            Invoke(nameof(ResetAttackAnimLocal), 1.0f);
        }

        [PunRPC]
        private void RPC_PlayAttackAnim(int stateIndex, PhotonMessageInfo info)
        {
            // Esto corre en clientes remotos: reproducir anim de ataque (sin generar daño)
            var stateName = ((PlantAnimController.EPlantStates)stateIndex).ToString();
            animController.TriggerByName(stateName);
            animController.attack = true;
            // reset anim
            Invoke(nameof(ResetAttackAnimLocal), 1.0f);
        }

        private void ResetAttackAnimLocal()
        {
            animController.attack = false;
            animController.TriggerByName(PlantAnimController.EPlantStates.Idle.ToString());
        }

        // ---------- DAÑO (solicitud / aplicación) ----------
        // Cuando un cliente (no-master) detecta que golpeó la planta, debe llamar:
        // photonView.RPC("RPC_RequestDamage", RpcTarget.MasterClient, damage, PhotonNetwork.LocalPlayer.ActorNumber);
        // En este script también procesamos daño si el master lo detecta localmente.

        [PunRPC]
        private void RPC_RequestDamage(float damage, int attackerActorNumber, PhotonMessageInfo info)
        {
            // Solo el MasterClient debe aceptar solicitudes
            if (!PhotonNetwork.IsMasterClient) return;

            if (debugLogs) Debug.Log($"[PlantNetwork] Master received damage request {damage} from actor {attackerActorNumber}");

            // Aplicar daño autoridad y notificar a todos el resultado
            ApplyDamageAuthority(damage, attackerActorNumber);
        }

        // Aplica el daño en el master y notifica al resto (RPC a todos)
        private void ApplyDamageAuthority(float damage, int attackerActorNumber)
        {
            if (!IsAuthoritative) return;

            if (lifeSupport != null)
            {
                lifeSupport.Context.HealthVital.TakeDamage(damage);
                _currentHealth = lifeSupport.Context.HealthVital.CurrentHealth;
            }
            else
            {
                _currentHealth -= damage;
            }

            // Notificar a todos el nuevo valor de vida (y si muere)
            photonView.RPC(nameof(RPC_ApplyDamageVisual), RpcTarget.All, _currentHealth);

            if (_currentHealth <= 0f && !_isDead)
            {
                // marca muerto en master y notifica a todos
                _isDead = true;
                photonView.RPC(nameof(RPC_SetAlive), RpcTarget.All, false);
            }
        }

        [PunRPC]
        private void RPC_ApplyDamageVisual(float newHealth, PhotonMessageInfo info)
        {
            // En todos los clientes actualizamos la visualizacion de vida local
            _currentHealth = newHealth;
            // Si necesitás mostrar HUD, barras, etc., hacelo aquí
        }

        [PunRPC]
        private void RPC_SetAlive(bool alive, PhotonMessageInfo info)
        {
            // Todos reciben la orden de cambiar el estado vivo/muerto
            if (!alive)
            {
                // morir (todos reproducen anim y ragdoll simulate según tu sistema)
                _isDead = true;
                animController.TriggerByName(PlantAnimController.EPlantStates.Dead.ToString());
                // activamos ragdoll en master y en clientes (si querés que clientes solo visualicen física,
                // podés optar por replicar poses en vez de activar física en cada cliente)
                arCoreScript.Kill(); // asume que Kill/Revive existen y funcionan en cualquier cliente
                if (attackScript) attackScript.enabled = false;
                if (targetingScript) targetingScript.enabled = false;
            }
            else
            {
                // revive
                _isDead = false;
                animController.TriggerByName(PlantAnimController.EPlantStates.Alive.ToString());
                arCoreScript.Revive();
                if (attackScript) attackScript.enabled = IsAuthoritative; // solo master vuelve a estar activo
                if (targetingScript) targetingScript.enabled = IsAuthoritative;
            }
        }

        // ---------- IPunObservable: sincronización adicional (opcional) ----------
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Escribimos el estado si somos autoridad
                stream.SendNext(_currentHealth);
                stream.SendNext(_isDead);
            }
            else
            {
                // Lectura en clientes remotos
                _currentHealth = (float)stream.ReceiveNext();
                bool readDead = (bool)stream.ReceiveNext();

                if (readDead != _isDead)
                {
                    // si cambió el estado, aplicarlo localmente (esto es redundante si usás RPC_SetAlive)
                    RPC_SetAlive(!readDead ? true : false, info);
                }
            }
        }

        // ----------------- UTIL para clientes que detectan un golpe localmente -----------------
        // Si tu NervousSystemScript dispara localmente en el cliente que ataca, llamá a este método
        // para notificar al master:
        //
        //    plantNetworkController.RequestDamage(damage);
        //
        public void RequestDamage(float damage)
        {
            if (IsAuthoritative)
            {
                // si somos master, aplicamos directo
                ApplyDamageAuthority(damage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                // enviamos solicitud al master
                photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, damage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }
    }
}
