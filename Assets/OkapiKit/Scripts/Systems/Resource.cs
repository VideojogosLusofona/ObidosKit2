using System;
using System.Collections.Generic;
using UnityEngine;

namespace OkapiKit
{
    public class Resource : OkapiElement
    {
        public enum ChangeType { Burst, OverTime };
        [Flags]
        public enum Flags
        {
            UseCooldownOnChanges = 1,
            UseCooldownPerSource = 2,
            OverrideStartValue = 4,
            EnableCombatText = 8,
            RecoveryOverTime = 16,
            WaitBeforeRecover = 32
        };

        public delegate void OnChange(ChangeType changeType, float deltaValue, Vector3 changeSrcPosition, Vector3 changeSrcDirection, GameObject changeSource);
        public event OnChange onChange;
        public delegate void OnResourceEmpty(GameObject changeSource);
        public event OnResourceEmpty onResourceEmpty;
        public delegate void OnResourceNotEmpty(GameObject healSource);
        public event OnResourceNotEmpty onResourceNotEmpty;

        [SerializeField]
        public ResourceType type;
        [SerializeField]
        private Flags       flags;
        [SerializeField]
        private float       globalCooldown = 0.1f;
        [SerializeField]
        private float       cooldownPerSource = 0.5f;
        [SerializeField]
        private float       startValue = 100.0f;
        [SerializeField]
        private float       recoveryPerSecond = 10.0f;
        [SerializeField]
        private float       timeBeforeRecovery = 1.0f;

        protected float _resource = 100.0f;
        protected bool  _resourceEmpty;
        protected float timeOfLastChange;
        protected Dictionary<GameObject, float> timeOfLastChangePerSource = new();
        
        public bool isOnGlobalCooldown => ((flags & Flags.UseCooldownOnChanges) != 0) && ((Time.time - timeOfLastChange) < globalCooldown);
        public bool isOnCooldown(GameObject src) => (isOnGlobalCooldown) || ((flags & Flags.UseCooldownPerSource) != 0) && (src != null) && (timeOfLastChangePerSource.ContainsKey(src) && (Time.time - timeOfLastChangePerSource[src]) < cooldownPerSource);
        public bool isCombatTextEnabled => (flags & Flags.EnableCombatText) != 0;
        public bool canRecover => (flags & Flags.RecoveryOverTime) != 0;
        public bool waitBeforeRecover => (flags & Flags.WaitBeforeRecover) != 0;

        public float maxValue => type?.maxValue ?? 0.0f;

        public Vector2 GetRange() => new Vector2(0.0f, type?.maxValue ?? 1.0f);

        public float resource
        {
            get { return _resource; }
        }

        public float normalizedResource
        {
            get { return _resource / type.maxValue; }
        }

        public bool isResourceEmpty => _resourceEmpty;
        public bool isResourceNotEmpty => !_resourceEmpty;

        void Start()
        {
            ResetResource();
            _resourceEmpty = false;
        }

        public bool Change(ChangeType changeType, float deltaValue, Vector3 changeSrcPosition, Vector3 changeSrcDirection, GameObject changeSource, bool canAddOnEmpty = true)
        {
            float prevValue = _resource;
            bool ret = true;

            if (isOnCooldown(changeSource))
            {
                ret = false;
            }
            else
            {
                if (deltaValue < 0)
                {
                    if (_resourceEmpty) ret = false;
                    else
                    {
                        _resource = Mathf.Clamp(_resource + deltaValue, 0.0f, type.maxValue);
                        if (_resource <= 0.0f)
                        {
                            _resource = 0.0f;
                            _resourceEmpty = true;

                            onResourceEmpty?.Invoke(changeSource);
                        }
                        else
                        {
                            onChange?.Invoke(changeType, deltaValue, changeSrcPosition, changeSrcDirection, changeSource);
                        }

                        if (isCombatTextEnabled)
                        {
                            CombatTextManager.SpawnText(gameObject, deltaValue, "Health {0}", type.displayNegativeTextColor, type.displayNegativeTextColor);
                        }
                    }
                }
                else if (deltaValue > 0)
                {
                    if (canAddOnEmpty)
                    {
                        if (_resource < type.maxValue)
                        {
                            _resource = Mathf.Clamp(_resource + deltaValue, 0.0f, type.maxValue);

                            onChange?.Invoke(changeType, deltaValue, changeSrcPosition, changeSrcDirection, changeSource);

                            if ((_resource > 0.0f) && (_resourceEmpty))
                            {
                                onResourceNotEmpty?.Invoke(changeSource);
                                _resourceEmpty = false;
                            }

                            if (isCombatTextEnabled)
                            {
                                CombatTextManager.SpawnText(gameObject, deltaValue, "Health {0}", type.displayTextColor, type.displayTextColor);
                            }
                        }
                    }
                    else if (_resourceEmpty) ret = false;
                    else
                    {
                        if (_resource < type.maxValue)
                        {
                            _resource = Mathf.Clamp(_resource + deltaValue, 0.0f, type.maxValue);

                            onChange?.Invoke(changeType, deltaValue, changeSrcPosition, changeSrcDirection, changeSource);

                            if (isCombatTextEnabled)
                            {
                                CombatTextManager.SpawnText(gameObject, deltaValue, "Health {0}", type.displayTextColor, type.displayTextColor);
                            }
                        }
                        else ret = false;
                    }
                }

                if (ret)
                {
                    if ((changeSource) && ((flags & Flags.UseCooldownPerSource) != 0) && (changeSource != gameObject))
                    {
                        timeOfLastChangePerSource[changeSource] = Time.time;
                    }
                    timeOfLastChange = Time.time;
                }
            }

            return ret;
        }

        public static List<Resource> FindAllByType(ResourceType type)
        {
            var allObjects = FindObjectsByType<Resource>(FindObjectsSortMode.None);
            var ret = new List<Resource>();
            foreach (var obj in allObjects)
            {
                if (obj.type == type) ret.Add(obj);
            }

            return ret;
        }

        public static List<Resource> FindAllInRadius(ResourceType type, Vector3 pos, float range)
        {
            List<Resource> ret = new();
            var resHandlers = FindObjectsByType<Resource>(FindObjectsSortMode.None);
            foreach (var h in resHandlers)
            {
                if ((h.type == type) && (Vector3.Distance(h.transform.position, pos) < range))
                {
                    ret.Add(h);
                }
            }

            return ret;
        }

        public void SetResource(float r, GameObject src = null)
        {
            if (isOnCooldown(src)) return;

            _resource = Mathf.Clamp(r, 0.0f, type.maxValue);
            _resourceEmpty = (_resource <= 0.0f);

            timeOfLastChange = Time.time;
        }

        public void ResetResource(GameObject src = null)
        {
            if (isOnCooldown(src)) return;

            float prevValue = _resource;

            if ((flags & Flags.OverrideStartValue) != 0)
                _resource = startValue;
            else
                _resource = type.defaultValue;

            _resourceEmpty = false;
            timeOfLastChange = Time.time;
        }

        public override string GetRawDescription(string ident, GameObject refObject)
        {
            if (type == null)
                return $"{ident}Resource is not configured with a valid type.\n";

            List<string> lines = new();

            lines.Add($"{ident}{type.displayName} resource (max: {type.maxValue:0.##}).");

            if ((flags & Flags.OverrideStartValue) != 0)
                lines.Add($"{ident}Starts at {startValue:0.##} instead of the default value.");

            if ((flags & Flags.UseCooldownOnChanges) != 0)
                lines.Add($"{ident}Global cooldown of {globalCooldown:0.##}s on changes.");

            if ((flags & Flags.UseCooldownPerSource) != 0)
                lines.Add($"{ident}Cooldown of {cooldownPerSource:0.##}s per source.");

            if ((flags & Flags.RecoveryOverTime) != 0)
            {
                lines.Add($"{ident}Recovers over time at {recoveryPerSecond:0.##} per second.");
                if ((flags & Flags.WaitBeforeRecover) != 0)
                    lines.Add($"{ident}Recovery starts after {timeBeforeRecovery:0.##}s without changes.");
            }

            if ((flags & Flags.EnableCombatText) != 0)
                lines.Add($"{ident}Combat text is enabled.");

            return string.Join("\n", lines);
        }


        private void Update()
        {
            if ((flags & Flags.UseCooldownPerSource) != 0)
            {
                var keysToRemove = new List<GameObject>();

                foreach (var kvp in timeOfLastChangePerSource)
                {
                    if (kvp.Key == null)
                        keysToRemove.Add(kvp.Key);
                    else if ((Time.time - kvp.Value) >= cooldownPerSource)
                        keysToRemove.Add(kvp.Key);
                }

                foreach (var key in keysToRemove)
                {
                    timeOfLastChangePerSource.Remove(key);
                }
            }
            if (canRecover)
            {
                if ((!waitBeforeRecover) || ((Time.time - timeOfLastChange) > timeBeforeRecovery))
                {
                    var prevTime = timeOfLastChange;
                    Change(ChangeType.OverTime, Time.deltaTime * recoveryPerSecond, transform.position, Vector3.zero, gameObject, true);
                    timeOfLastChange = prevTime;
                }
            }
        }
    }

    public static class ResourceExtensions
    {
        public static Resource FindResource(this Component component, ResourceType type)
        {
            var handlers = component.GetComponents<Resource>();
            foreach (var handler in handlers)
            {
                if (handler.type == type) return handler;
            }

            return null;
        }
    }

    [Serializable]
    public class TargetResource : OkapiTarget<Resource>
    {
        [SerializeField] public ResourceType resourceType;

        public override Resource GetTarget(GameObject parentGameObject)
        {
            var targets = GetTargets(parentGameObject);

            if (type == OkapiTarget<Resource>.Type.Object) return (targets.Count > 0) ? (targets[0]) : (null);

            foreach (var target in targets)
            {
                if (target.type == resourceType) return target;
            }

            return null;
        }

        public override string GetShortDescription(GameObject refObject)
        {
            var resName = resourceType?.displayName ?? "[UNDEFINED]";
            switch (type)
            {
                case Type.Hypertag:
                    if (tag) return $"[{tag.name}].{resName}";
                    else return $"[UNDEFINED].{resName}";
                case Type.Object:
                    if (obj)
                    {
                        resName = obj.type?.displayName ?? "[UNDEFINED]";
                        return $"[{obj.name}].{resName}";
                    }
                    else return $"[UNDEFINED].[UNDEFINED]";
                case Type.Self:
                    return $"{resName}";
                case Type.Collider:
                    return $"Collider.{resName}";
                default:
                    break;
            }

            return "[UNDEFINED]";
        }
    }
}
