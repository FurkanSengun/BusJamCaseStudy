using System;
using System.Collections.Generic;
using Core.Sound;
using Game.Bus;
using Game.Data;
using Game.Grid;
using Game.Interaction;
using Game.Level;
using Game.Navigation;
using Game.Queue;
using UnityEngine;
using Zenject;

namespace Game.Passenger
{
    /// <summary>
    /// Yolcuları init eder ve etkileşime girme durumunda hareket edebilme durumunu kontrol eder
    /// </summary>
    public class Passenger : MonoBehaviour, IPassenger, IInteractable
    {
        [SerializeField] private PassengerMovementController mover;
        [SerializeField] private PassengerVisualController visual;

        private IGridManager _gridManager;
        private IQueueManager _queueManager;
        private IPathManager _pathManager;
        private IBusManager _busManager;
        private ILevelManager _levelManager;
        private ISoundManager _soundManager;

        private bool _isBusy;
        private bool _isBoarded;

        public Vector2Int GridIndex { get; private set; }
        public PassengerColorType ColorType { get; private set; }
        public bool IsInQueue { get; private set; }
        public bool IsReadyForBoarding { get; private set; }

        public bool CanInteract => !_isBusy && !_isBoarded && !IsInQueue && _pathManager != null && _pathManager.CanReachFrontRow(GridIndex);

        [Inject]
        public void Construct( IGridManager gridManager, IQueueManager queueManager, IPathManager pathManager,
            IBusManager busManager, ILevelManager levelManager, ISoundManager soundManager)
        {
            _gridManager = gridManager;
            _queueManager = queueManager;
            _pathManager = pathManager;
            _busManager = busManager;
            _levelManager = levelManager;
            _soundManager = soundManager;
        }

        private void Awake()
        {
            if (mover == null)
            {
                mover = GetComponent<PassengerMovementController>();
            }

            if (visual == null)
            {
                visual = GetComponent<PassengerVisualController>();
            }
        }

        private void Start()
        {
            if (_gridManager != null)
            {
                _gridManager.OnGridStateChanged += RefreshInteractableVisual;
            }

            RefreshInteractableVisual();
        }

        private void OnDestroy()
        {
            if (_gridManager != null)
            {
                _gridManager.OnGridStateChanged -= RefreshInteractableVisual;
            }
        }

        public void Initialize(Vector2Int gridIndex, PassengerColorType colorType)
        {
            GridIndex = gridIndex;
            ColorType = colorType;

            IsInQueue = false;
            IsReadyForBoarding = false;

            _isBusy = false;
            _isBoarded = false;

            _gridManager?.Occupy(GridIndex);
            visual?.ApplyColor(colorType);
            visual?.PlayIdle();
            RefreshInteractableVisual();
        }

        public void Interact()
        {
            if (!CanInteract)
            {
                return;
            }

            _isBusy = true;
            IsReadyForBoarding = false;
            RefreshInteractableVisual();

            _gridManager?.Release(GridIndex);

            if (_busManager != null && _busManager.TryBoardSelectedPassenger(this))
            {
                return;
            }

            if (_queueManager == null || !_queueManager.TryReserveNextSlot(out QueueSlot slot))
            {
                _gridManager?.Occupy(GridIndex);
                _isBusy = false;
                RefreshInteractableVisual();
                return;
            }

            if (!slot.TryOccupy(this))
            {
                _gridManager?.Occupy(GridIndex);
                _isBusy = false;
                RefreshInteractableVisual();
                return;
            }
            
            visual?.PlayRunning();
            _soundManager?.PlayOneShot(SfxIds.PassengerClick);
            MoveToQueueSlot(slot);
        }

        public void MoveToQueueSlot(QueueSlot slot, Action onComplete = null)
        {
            if (slot == null)
            {
                return;
            }

            IsReadyForBoarding = false;
            transform.SetParent(null, true);
            visual?.PlayRunning();
            RefreshInteractableVisual();

            Action finalize = () =>
            {
                transform.position = slot.transform.position;
                transform.SetParent(slot.transform, true);

                IsInQueue = true;
                IsReadyForBoarding = true;
                _isBusy = false;

                visual?.PlayIdle();
                RefreshInteractableVisual();
                _queueManager?.NotifyQueueChanged();
                onComplete?.Invoke();
            };

            if (IsInQueue)
            {
                MoveDirect(slot.transform.position, finalize);
                return;
            }

            MoveViaFrontRow(slot.transform.position, finalize);
        }

        public void BoardBus(Transform boardingPoint, Transform seatTransform, Action onComplete = null)
        {
            if (seatTransform == null)
            {
                return;
            }

            IsReadyForBoarding = false;
            _isBusy = true;
            visual?.PlayRunning();
            RefreshInteractableVisual();

            transform.SetParent(null, true);

            Action finalize = () =>
            {
                IsInQueue = false;
                _isBoarded = true;
                _isBusy = false;

                visual?.PlaySitting();
                RefreshInteractableVisual();
                _soundManager?.PlayOneShot(SfxIds.PassengerClick);
                onComplete?.Invoke();
            };

            Vector3 boardingWorldPosition = boardingPoint != null
                ? boardingPoint.position
                : seatTransform.position;

            if (mover == null)
            {
                transform.position = seatTransform.position;
                transform.SetParent(seatTransform, true);
                finalize();
                return;
            }

            if (IsInQueue)
            {
                mover.MoveToBoardingPointThenSeat(boardingWorldPosition, seatTransform, finalize);
                return;
            }

            MoveViaFrontRow(boardingWorldPosition, () =>
            {
                mover.MoveToBoardingPointThenSeat(boardingWorldPosition, seatTransform, finalize);
            });
        }

        private void RefreshInteractableVisual()
        {
            visual?.SetOutlineEnabled(CanInteract);
        }

        private void MoveViaFrontRow(Vector3 finalWorldPosition, Action onComplete)
        {
            if (!TryBuildFrontRowWorldPath(out List<Vector3> worldPath) || worldPath.Count == 0)
            {
                MoveDirect(finalWorldPosition, onComplete);
                return;
            }

            if (mover == null)
            {
                MoveDirect(finalWorldPosition, onComplete);
                return;
            }

            mover.MoveAlongPath(worldPath, () => MoveDirect(finalWorldPosition, onComplete));
        }

        private void MoveDirect(Vector3 worldPosition, Action onComplete)
        {
            if (mover == null)
            {
                transform.position = worldPosition;
                onComplete?.Invoke();
                return;
            }

            mover.MoveTo(worldPosition, onComplete);
        }

        private bool TryBuildFrontRowWorldPath(out List<Vector3> worldPath)
        {
            worldPath = new();

            if (_pathManager == null || _levelManager == null)
            {
                return false;
            }

            if (!_pathManager.TryGetPathToFrontRow(GridIndex, out List<Vector2Int> gridPath))
            {
                return false;
            }

            for (int i = 1; i < gridPath.Count; i++)
            {
                if (!_levelManager.TryGetGroundTile(gridPath[i], out GameObject groundTile))
                {
                    continue;
                }

                Vector3 worldPosition = groundTile.transform.position;
                worldPosition.y = transform.position.y;
                worldPath.Add(worldPosition);
            }

            return true;
        }
        
        public void RelocateToQueueSlot(QueueSlot slot, Action onComplete = null)
        {
            if (slot == null)
            {
                return;
            }

            IsReadyForBoarding = false;
            _isBusy = true;

            transform.SetParent(null, true);
            visual?.PlayRunning();
            RefreshInteractableVisual();

            Action finalize = () =>
            {
                transform.position = slot.transform.position;
                transform.SetParent(slot.transform, true);

                IsInQueue = true;
                IsReadyForBoarding = true;
                _isBusy = false;

                visual?.PlayIdle();
                RefreshInteractableVisual();
                _queueManager?.NotifyQueueChanged();
                onComplete?.Invoke();
            };

            MoveDirect(slot.transform.position, finalize);
        }
    }
}