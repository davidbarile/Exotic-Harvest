using UnityEngine;

public abstract class PlatformBasedModifierBase : MonoBehaviour
    {
        protected enum ECallType
        {
            Awake,
            Start,
            OnEnable,
            Update
        }

        [SerializeField] private ECallType _callType = ECallType.Start;

        private bool _isComplete;

#region Validation
        
        protected virtual void OnValidate()
        {
            CheckForNullOrDuplicates();
        }

        protected virtual void CheckForNullOrDuplicates()
        {
            //implement on derived classes
        }

#endregion

        private void Awake()
        {
            if (_callType == ECallType.Awake)
                Execute();
        }

        protected virtual void Start()
        {
            CheckForNullOrDuplicates();

            if (_callType == ECallType.Start)
                Execute();
        }

        private void OnEnable()
        {
            if (_callType == ECallType.OnEnable)
                Execute();
        }

        private void Update()
        {
            if (_isComplete) return;

            if (_callType == ECallType.Update)
                Execute();
        }

        protected virtual bool Execute()
        {
            _isComplete = true;

            if (PlatformManager.IN == null)
            {
                Debug.LogError($"PlatformService is not initialized!  PlatformBasedModifierBase on {name}.  Frame = {Time.frameCount}", gameObject);
                return false;
            }

            return true;
        }
    }