using System;
using System.Collections;
using System.Collections.Generic;
using Generics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerHandler : MonoBehaviour, IStateMachineController, IShootable
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayerControlsHandler playerControlsHandler;
        [SerializeField] private PlayerSO playerSO;
        [SerializeField] private float turnForce;
        [SerializeField] private Transform graphics;
        [SerializeField] private UIHandler _uiHandler;
        [SerializeField] private float rotationSpeed = 720.0f;

        [FormerlySerializedAs("MeshRenderer")] [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private List<Material> stainedWolfMaterials;

        private WolfStateMachine stateMachine;
        private ShootableType shootableType;
        private bool isHiddenInBush = false;
        private bool isBloody = false;
        private bool isCrouching = false;
        private int stainCount;

        public Vector2 MoveInput => playerControlsHandler != null ? playerControlsHandler.moveVector : Vector2.zero;
        public Vector2 LookInput => playerControlsHandler != null ? playerControlsHandler.lookVector : Vector2.zero;
        public Vector3 lastLookDirection;

        public static Action<TutorialEntetyType> OnPlayerInProximityOf;
        public static Action OnPlayerBloodyAndNextToSheep;

        public void Awake()
        {
            RigidbodyUtility.Initialize(rb);
            stainCount = playerSO.MaxStainCount;
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

        public void StartCheckForSheepCoroutine(InputType inputType)
        {
            StartCoroutine(CheckForSheepCoroutine(inputType));
        }

        public void NotifyBushEnter()
        {
            isHiddenInBush = true;
        }

        public void NotifyBushExit()
        {
            isHiddenInBush = false;
        }

        public void NotifyPuddleEnter(PuddleType puddleType)
        {
            switch (puddleType)
            {
                case PuddleType.Blood:
                    isBloody = true;
                    SwapWolfMaterialToStained();
                    stainCount = playerSO.MaxStainCount;
                    break;
                case PuddleType.Water:
                    isBloody = false;
                    SwapWolfMaterialToNormal();
                    break;
            }
        }

        public GameObject GetGameObj()
        {
            return gameObject;
        }

        [ContextMenu("TryGotShot")]
        public void GotShot()
        {
            StartCoroutine(GameOverUIDelay(1.0f));
            GameManager.Instance.StopTimer();
        }
        private IEnumerator GameOverUIDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _uiHandler.OpenGameoverCanvas();
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
                return true;
            }
        }

        public void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
        }

        public IEnumerator CheckForSheepCoroutine(InputType inputType)
        {
            var timer = 0f;
            var targetFound = false;
            switch (inputType)
            {
                case InputType.Attack:
                    timer = playerSO.AttackDuration * 0.5f;
                    break;
                case InputType.Stain:
                    timer = playerSO.StainDuration * 0.5f;
                    break;
            }

            while (timer > 0f && !targetFound)
            {
                var attackDirection = graphics.forward;
                if (Physics.SphereCast(transform.position, playerSO.DetectionRadius, attackDirection,
                        out RaycastHit hit, playerSO.DetectionRange))
                {
                    if (hit.collider.gameObject.CompareTag("Sheep"))
                    {
                        var sheep = hit.collider.GetComponent<Sheep>();
                        if (sheep && sheep.isAlive)
                        {
                            switch (inputType)
                            {
                                case InputType.Attack:
                                    sheep.Die(KilledBy.Wolf);
                                    isBloody = true;
                                    SwapWolfMaterialToStained();
                                    stainCount = playerSO.MaxStainCount;
                                    break;
                                case InputType.Stain:
                                {
                                    if (isBloody && stainCount > 0)
                                    {
                                        sheep.SetStained();
                                        stainCount--;
                                        if (stainCount <= 0)
                                        {
                                            isBloody = false;
                                            SwapWolfMaterialToNormal();
                                        }
                                    }

                                    break;
                                }
                            }

                            targetFound = true;
                        }
                    }
                }

                timer -= Time.deltaTime;
                yield return null;
            }
        }

        private void SwapWolfMaterialToStained()
        {
            var randomIndex = Random.Range(1, stainedWolfMaterials.Count - 1);
            var outline = GetComponent<Outline>();
            outline.enabled = false;
            meshRenderer.materials = new[] { stainedWolfMaterials[randomIndex] };
            outline.enabled = true;
        }

        private void SwapWolfMaterialToNormal()
        {
            var outline = GetComponent<Outline>();
            outline.enabled = false;
            meshRenderer.materials = new[] { stainedWolfMaterials[0] };
            outline.enabled = true;
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

        private void OnDrawGizmos()
        {
            if (graphics == null || playerSO == null)
                return;

            var attackDirection = graphics.forward;
            var playerMove3 = new Vector3(attackDirection.x, 0f, attackDirection.z).normalized;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, playerMove3 * playerSO.DetectionRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerSO.DetectionRadius);
            Gizmos.DrawWireSphere(transform.position + playerMove3 * playerSO.DetectionRange, playerSO.DetectionRadius);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Sheep"))
            {
                OnPlayerInProximityOf?.Invoke(TutorialEntetyType.Sheep);

                if (isBloody)
                {
                    OnPlayerBloodyAndNextToSheep?.Invoke();
                }
            }

            if (other.CompareTag("Bush"))
            {
                OnPlayerInProximityOf?.Invoke(TutorialEntetyType.Bush);
            }

            if (other.CompareTag("Shepherd"))
            {
                OnPlayerInProximityOf?.Invoke(TutorialEntetyType.Shepherd);
            }
        }
    }
}

public enum TutorialEntetyType
{
    Sheep,
    Bush,
    Shepherd
}