using UnityEngine;


namespace Multiplayer.TerrainGeneratorScripts.Spaceships___2
{
   public class InfoScript : MonoBehaviour
   {
       [SerializeField] BranchDivisionScript branchDivision;
       [SerializeField] SpawnerScript spawner;
       public static int RamaID = -1;


       private void Awake()
       {
           branchDivision = FindAnyObjectByType<BranchDivisionScript>();
           spawner = FindAnyObjectByType<SpawnerScript>();
       }
      
       private void Start()
       {
           if (gameObject.CompareTag("Intersection"))
           {
               if (!IsOverlapping())
               {
                   branchDivision.CerrarRama(RamaID, false);
                   RamaID = branchDivision.CrearRama();
                   branchDivision.AgregarAHilo(RamaID, this);
               }
               else
               {
                   branchDivision.AgregarAHilo(RamaID, this);
                   branchDivision.CerrarRama(RamaID, true);
               }
           }
           else if (gameObject.CompareTag("Room"))
           {
               if (!IsOverlapping())
               {
                   branchDivision.AgregarAHilo(RamaID, this);
                   branchDivision.CerrarRama(RamaID, false);
                   RamaID = branchDivision.CrearRama();
               }
               else
               {
                   branchDivision.AgregarAHilo(RamaID, this);
                   branchDivision.CerrarRama(RamaID, true);
               }
           }
           else if (gameObject.CompareTag("Hallway"))
           {
               branchDivision.AgregarAHilo(RamaID, this);
               if (IsOverlapping())
                   branchDivision.CerrarRama(RamaID, true);
           }
       }
       private bool IsOverlapping()
       {
           Collider myCol = GetComponent<Collider>();


           Collider[] others = Physics.OverlapBox(
               myCol.bounds.center,
               myCol.bounds.extents,
               transform.rotation
           );


           foreach (var other in others)
           {
               if (other.transform.root == transform.root)
                   continue;
              
               if (other.CompareTag("Room") || other.CompareTag("Hallway") || other.CompareTag("Intersection"))
               {
                   Vector3 dir;
                   float distance;
                  
                   if (Physics.ComputePenetration(
                           myCol, transform.position, transform.rotation,
                           other, other.transform.position, other.transform.rotation,
                           out dir, out distance))
                   {
                       if (distance > 0.1f)
                       {
                           Debug.Log($"Se destruyó {gameObject.name} porque chocó con {other.name}");
                           return true;
                       }
                   }
               }
           }


           return false;
       }
       
       public void OnDestroy()
       {
           if (gameObject.CompareTag("Room"))
           {
               spawner.rooms -= 1;
           }
       }
   }
}
