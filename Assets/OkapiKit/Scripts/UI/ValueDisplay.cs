using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OkapiKit
{
    public class ValueDisplay : OkapiElement
    {
        public enum ValueSource { Variable, VariableInstance, Resource };

        [SerializeField]
        protected ValueSource       valueSource = ValueSource.Variable;
        [SerializeField]
        protected VariableInstance  valueHandler;
        [SerializeField]
        protected Variable          variable;
        [SerializeField]
        protected TargetResource    resource;

        public TargetResource GetResource() => resource;

        public override string GetRawDescription(string ident, GameObject refObject)
        {
            return "";
        }

        protected override string Internal_UpdateExplanation()
        {
            _explanation = "";

            if (description != "") _explanation += description + "\n----------------\n";

            _explanation += GetRawDescription("", gameObject);

            return _explanation;
        }

        protected float GetCurrentValue()
        {
            switch (valueSource)
            {
                case ValueSource.Variable:
                    return variable?.currentValue ?? 0.0f;
                case ValueSource.VariableInstance:
                    return valueHandler?.GetVariable().currentValue ?? 0.0f;
                case ValueSource.Resource:
                    return resource.GetTarget(gameObject)?.resource ?? 0.0f;
                default:
                    break;
            }

            return 0.0f;
        }

        protected Vector2 GetRange()
        {
            switch (valueSource)
            {
                case ValueSource.Variable:
                    return variable?.GetRange() ?? Vector2.up;
                case ValueSource.VariableInstance:
                    return valueHandler?.GetVariable().GetRange() ?? Vector2.up;
                case ValueSource.Resource:
                    return resource.GetTarget(gameObject)?.GetRange() ?? Vector2.up;
                default:
                    break;
            }

            return Vector2.up;
        }

        protected Variable GetVariable()
        {
            switch (valueSource)
            {
                case ValueSource.Variable:
                    return variable;
                case ValueSource.VariableInstance:
                    return valueHandler.GetVariable();
            }

            return null;
        }

        protected override void CheckErrors()
        {
            base.CheckErrors();

            switch (valueSource)
            {
                case ValueSource.Variable:
                    if (variable == null)
                    {
                        _logs.Add(new LogEntry(LogEntry.Type.Error, "No variable is set!", "This will display the contents of a variable, so we need to define which one."));
                    }
                    break;
                case ValueSource.VariableInstance:
                    if (valueHandler == null)
                    {
                        _logs.Add(new LogEntry(LogEntry.Type.Error, "No value handler is set!", "This will display the contents of a variable, so we need to define which one."));
                    }
                    break;
                case ValueSource.Resource:
                    resource.CheckErrors(_logs, "source value", gameObject);
                    break;
                default:
                    break;
            }
            
        }
    }
}