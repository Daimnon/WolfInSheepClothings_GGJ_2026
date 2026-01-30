using System;
using Generics;
using UnityEngine;

namespace Player
{
    public class PlayerHandler : MonoBehaviour, IStateMachineController, IShootable
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayerControlsHandler playerControlsHandler;
        [SerializeField] private PlayerSO playerSO;
        [SerializeField] private ShepherdAI shepherdAI;
        [SerializeField] private float turnForce;
        [SerializeField] private Transform graphics;
        [SerializeField] private float rotationSpeed = 720.0f;
        private WolfStateMachine stateMachine;
        private ShootableType shootableType;
        private bool isHiddenInBush = false;
        private bool isBloody = false;
        private bool isCrouching = false;
        
        public Vector2 MoveInput => playerControlsHandler != null ? playerControlsHandler.moveVector : Vector2.zero;
        public Vector2 LookInput => playerControlsHandler != null ? playerControlsHandler.lookVector : Vector2.zero;
        public Vector3 lastLookDirection;

        public static event Action<Vector3> OnSheepKilled;

        public void Awake()
        {
            RigidbodyUtility.Initialize(rb);
        }

        private void Start()
        {
            stateMachine = new WolfStateMachine(this, playerControlsHandler, playerSO);
            stateMachine.Start();
            playerControlsHandler.OnInput += stateMachine.PassInput;
        }

        public void NotifyStateEnter(IState state)
        {
            //Debug.Log(state.GetType().Name + " entered.");
        }

        public void NotifyChangeInShootableType(ShootableType newType)
        {
            switch (newType)
            {
                case ShootableType.Wolf:
                    shootableType = newType;
                    isCrouching = false;
                    break;
                case ShootableType.Sheep:
                    shootableType = isBloody ? ShootableType.BloodySheep : newType;
                    isCrouching = true;
                    break;
                case ShootableType.BloodySheep:
                    shootableType = newType;
                    break;
            }
        }

        public Transform GetTransform()
        {
            return graphics;
        }

        public void NotifyBushEnter()
        {
            isHiddenInBush = true;
        }

        public void NotifyBushExit()
        {
            isHiddenInBush = false;
        }

        public void NotifyPuddleEnter()
        {
            isBloody = false;
        }

        public GameObject GetGameObj()
        {
            return gameObject;
        }

        public void GotShot()
        {
            // implement game over here
        }

        public ShootableType GetShootableType()
        {
            if (isHiddenInBush)
                return ShootableType.Hidden;
            return shootableType;
        }

        public bool CanBeTargeted
        {
            get
            {
                if (shootableType == ShootableType.Hidden) return false;
                
                if(shepherdAI.AggroMeter >= 50) return true;
                
                return !isCrouching;
            }
        }

        public void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
        }

        public Sheep GetSheepInSphereCast()
        {
            if (Physics.SphereCast(transform.position, playerSO.DetectionRadius, graphics.forward, out RaycastHit hit, playerSO.DetectionRange))
            {
                Sheep sheep = hit.collider.GetComponent<Sheep>();
                if (sheep)
                {
                    return sheep;
                }
            }
            return null;
        }
        private void Update()
        {
            RotateGraphics();
        }

        private void RotateGraphics()
        {
            if (MoveInput.sqrMagnitude < 0.01f)
                return;
            // convert 2D input to 3D world direction
            Vector3 moveDir = new Vector3(MoveInput.x, 0f, MoveInput.y).normalized;
            var moveDirRotated = Quaternion.Euler(0f, 45f, 0f) * moveDir.normalized;


            // calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirRotated, Vector3.up);

            // rotate graphics toward target rotation
            graphics.rotation = Quaternion.RotateTowards(
                graphics.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }
}