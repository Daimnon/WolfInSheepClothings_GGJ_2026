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
            return transform;
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
                if (isHiddenInBush) return false;
                
                if(shepherdAI.AggroMeter >= 50) return true;
                
                return !isCrouching;
            }
        }

        public void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
        }

        public Sheep GetSheepSphereCast()
        {
            if (Physics.SphereCast(transform.position, playerSO.DetectionRadius, transform.forward, out RaycastHit hit, playerSO.DetectionRange))
            {
                Sheep sheep = hit.collider.GetComponent<Sheep>();
                if (sheep != null)
                {
                    return sheep;
                }
            }
            return null;
        }

        // private void Update()
        // {
        //     RotateToMoveInput(playerSO.RotationLerpSpeed);
        // }
        private void FixedUpdate()
        {
            Rotate();
        }
        private void Rotate()
        {
            if (Mathf.Abs(MoveInput.x) < 0.01f)
                return;

            float direction = Mathf.Sign(MoveInput.x);
            rb.AddTorque(Vector3.up * direction * turnForce, ForceMode.Force);
            rb.angularDamping = 5f;
        }
        // private void RotateToMoveInput(float lerpSpeed = 10f)
        // {
        //     var input = MoveInput;
        //     
        //     var targetDir = new Vector3(input.x, 0f, input.y);
        //     if (targetDir == Vector3.zero)
        //         return;
        //     lastLookDirection = targetDir;
        //     if (targetDir.sqrMagnitude < 0.0001f)
        //         return;
        //
        //     Quaternion targetRot = Quaternion.LookRotation(lastLookDirection.normalized, Vector3.up);
        //     transform.R
        // }
    }
}